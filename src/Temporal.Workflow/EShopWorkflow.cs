using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Temporalio.Activities;
using Temporalio.Common;
using Temporalio.Workflows;
using static Temporal.Workflow.ICatalogService;


namespace Temporal.Workflow
{
    [Workflow]
    public class EShopWorkflow
    {

        int _orderId = default;
        private PaymentStatus _paymentStatus = PaymentStatus.Unknown;
        

        [WorkflowRun]
        public async Task RunAsync(OrderRequest orderRequest)
        {

            // Retry policy
            var retryPolicy = new RetryPolicy
            {
                InitialInterval = TimeSpan.FromSeconds(1),
                MaximumInterval = TimeSpan.FromSeconds(100),
                BackoffCoefficient = 2,
                MaximumAttempts = 3,
                NonRetryableErrorTypes = new[] { "InvalidAccountException", "InsufficientFundsException" }
            };

            _orderId = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (EShopActivities act) => act.CreateOrder(orderRequest),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });


            //Grace period
            await Temporalio.Workflows.Workflow.DelayAsync(TimeSpan.FromSeconds(5));

            await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (EShopActivities act) => act.SetAwaitingValidation(_orderId),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });

            var checkStockResult = await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                (EShopActivities act) => act.CheckStock(_orderId, orderRequest.Items),
                new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });

            if (checkStockResult.StockConfirmed)
                await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
               (EShopActivities act) => act.ConfirmThatHasStock(_orderId),
               new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });
            else
                await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
             (EShopActivities act) => act.ConfirmThatHasNoStock(_orderId, checkStockResult.OrderStockItems.Select(i => new IOrderService.ConfirmedOrderStockItem(i.ProductId, i.HasStock))),
             new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });


            await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
             (EShopActivities act) => act.ConfirmPaymentAsync(_orderId, orderRequest.OrderyGuid),
             new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });

            // Wait for purchase
            await Temporalio.Workflows.Workflow.WaitConditionAsync(() => _paymentStatus != PaymentStatus.Unknown);

            if (_paymentStatus == PaymentStatus.Succeeded)
            {
                // Payment succeeded logic
                await Temporalio.Workflows.Workflow.ExecuteActivityAsync((EShopActivities act) => act.SetPaidOrderStatus(_orderId),
                       new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });

                //TODO update stock in catalog service

            }
            else if (_paymentStatus == PaymentStatus.Failed)
            {
                await Temporalio.Workflows.Workflow.ExecuteActivityAsync((EShopActivities act) => act.CancelOrder(_orderId),
                       new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });

            }


        }

        enum PaymentStatus
        {
            Unknown,
            Succeeded,
            Failed
        }

        [WorkflowSignal("NotifyOrderPaymentSucceeded")]
        public async Task NotifyOrderPaymentSucceededAsync() => _paymentStatus = PaymentStatus.Succeeded;

        [WorkflowSignal("NotifyOrderPaymentFailed")]
        public async Task NotifyOrderPaymentFailedAsync() => _paymentStatus = PaymentStatus.Failed;



    }



    public class EShopActivities
    {
        private readonly IPaymentsService _paymentsService;

        public IOrderService _orderService { get; }
        public ICatalogService _catalogService { get; }

        public EShopActivities(IOrderService orderService, ICatalogService catalogService, IPaymentsService paymentsService)
        {
            _orderService = orderService;
            _catalogService = catalogService;
            _paymentsService = paymentsService;
        }

        [Activity]
        public async Task<int> CreateOrder(OrderRequest orderRequest)
        {
            var requestId = Guid.NewGuid().ToString();
            int orderId = await _orderService.CreateOrderAsync(orderRequest, requestId);
            ActivityExecutionContext.Current.Logger.LogInformation("CreateOrder {requestId}", requestId);
            return orderId;
        }

        public record SetAwaitingValidationRequest(int OrderId);

        [Activity]
        public async Task SetAwaitingValidation(int orderId)
        {
            await _orderService.SetAwaitingValidation(orderId);
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm if has Stock {OrderId}", orderId);
        }


        [Activity]
        public async Task<CheckStockResult> CheckStock(int orderId, IEnumerable<BasketItem> basketItems)
        {
            var result = await _catalogService.CheckStock(new CheckStockRequest(orderId, basketItems.Select(i => new OrderStockItem(i.ProductId, i.Quantity))));
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm if has Stock {OrderId}", orderId);
            return result;
        }


        [Activity]
        public async Task ConfirmThatHasStock(int orderId)
        {
            await _orderService.ConfirmStockAsync(orderId);
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm that has Stock {OrderId}", orderId);
        }

        [Activity]
        public async Task ConfirmThatHasNoStock(int orderId, IEnumerable<IOrderService.ConfirmedOrderStockItem> orderStockItems)
        {
            await _orderService.ConfirmStockRejectedAsync(orderId, orderStockItems);
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm that has not Stock {OrderId}", orderId);
        }
        [Activity]
        public async Task SetPaidOrderStatus(int orderId)
        {
            await _orderService.SetPaidOrderStatusAsync(orderId);
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm that has not Stock {OrderId}", orderId);
        }

        [Activity]
        public async Task CancelOrder(int orderId)
        {
            await _orderService.CancelOrderAsync(orderId);
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm that has not Stock {OrderId}", orderId);
        }

        [Activity]
        public async Task ConfirmPaymentAsync(int orderId, string orderyGuid)
        {
            await _paymentsService.ConfirmPaymentAsync(new PaymentRequest { OrderId = orderId, OrderyGuid = orderyGuid });
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm that has not Stock {OrderId}", orderId);
        }



    }




}
