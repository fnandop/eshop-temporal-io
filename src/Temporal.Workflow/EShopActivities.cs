using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Temporalio.Activities;
using static Temporal.Workflow.ICatalogService;


namespace Temporal.Workflow
{
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
        public async Task InitiatePaymentAsync(int orderId, string orderyGuid)
        {
            await _paymentsService.InitiatePaymentAsync(new PaymentRequest { OrderId = orderId, OrderyGuid = orderyGuid });
            ActivityExecutionContext.Current.Logger.LogInformation("Confirm that has not Stock {OrderId}", orderId);
        }
    }
}
