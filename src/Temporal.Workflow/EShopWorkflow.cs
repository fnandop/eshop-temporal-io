using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Duende.AccessTokenManagement;
using Microsoft.Extensions.Logging;
using Refit;
using Temporalio.Activities;
using Temporalio.Common;
using Temporalio.Exceptions;
using Temporalio.Workflows;


namespace Temporal.Workflow
{
    [Workflow]
    public class EShopWorkflow
    {


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

            await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
               (EShopActivities act) => act.CreateOrder(orderRequest),
               new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5), RetryPolicy = retryPolicy });
        }

    }

    public class EShopActivities(IOrderService orderService)
    {
        
        [Activity]
        public async Task CreateOrder(OrderRequest orderRequest)
        {
            var requestId = Guid.NewGuid().ToString();
            await orderService.CreateOrderAsync(orderRequest, requestId);
            ActivityExecutionContext.Current.Logger.LogInformation("CreateOrder {requestId}", requestId);
        }

        //[Activity]
        //public static void WithdrawCompensation(TransferDetails d)
        //{
        //    ActivityExecutionContext.Current.Logger.LogInformation("Withdrawing Compensation {Amount} from account {FromAmount}. ReferenceId: {ReferenceId}", d.Amount, d.FromAmount, d.ReferenceId);
        //}

        //[Activity]
        //public static void Deposit(TransferDetails d)
        //{
        //    ActivityExecutionContext.Current.Logger.LogInformation("Depositing {Amount} into account {ToAmount}. ReferenceId: {ReferenceId}", d.Amount, d.ToAmount, d.ReferenceId);
        //}

        //[Activity]
        //public static void DepositCompensation(TransferDetails d)
        //{
        //    ActivityExecutionContext.Current.Logger.LogInformation("Depositing Compensation {Amount} int account {ToAmount}. ReferenceId: {ReferenceId}", d.Amount, d.ToAmount, d.ReferenceId);
        //}

        //[Activity]
        //public static void StepWithError(TransferDetails d)
        //{
        //    ActivityExecutionContext.Current.Logger.LogInformation("Simulate failure to trigger compensation. ReferenceId: {ReferenceId}", d.ReferenceId);
        //    throw new ApplicationFailureException("Simulated failure", nonRetryable: true);
        //}
    }

    public record OrderRequest(
    string UserId,
    string UserName,
    string City,
    string Street,
    string State,
    string Country,
    string ZipCode,
    string CardNumber,
    string CardHolderName,
    DateTime CardExpiration,
    string CardSecurityNumber,
    int CardTypeId,
    string Buyer,
    List<BasketItem> Items);

    public class BasketItem
    {
        public required string Id { get; init; }
        public int ProductId { get; init; }
        public required string ProductName { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal OldUnitPrice { get; init; }
        public int Quantity { get; init; }
        public string? PictureUrl { get; init; }
    }

    public interface IOrderService
    {


        [Post("/api/orders/create?api-version=1.0")]
        Task CreateOrderAsync(OrderRequest orderRequest, [Header("x-requestid")] string requestId);

        //Task<IEnumerable<Models.Orders.Order>> GetOrdersAsync();

        //Task<Models.Orders.Order> GetOrderAsync(int orderId);

        //Task<bool> CancelOrderAsync(int orderId);

        //OrderCheckout MapOrderToBasket(Models.Orders.Order order);
    }


    //public class OrderService(HttpClient httpClient) : IOrderService
    //{




    //    public async Task CreateOrderAsync(OrderRequest orderRequest)
    //    {
    //        // create a new GUID per request (or pass one in if you already have it)
    //        var requestId = Guid.NewGuid().ToString();

    //        using var request = new HttpRequestMessage(
    //            HttpMethod.Post,
    //            "/api/orders/create?api-version=1.0"   // keep your api-version if required
    //        );

    //        request.Headers.Add("x-requestid", requestId); // header name matches parameter (case-insensitive)
    //        request.Content = JsonContent.Create(orderRequest);

    //        var response = await httpClient.SendAsync(request);
    //        response.EnsureSuccessStatusCode();
    //    }

    //    //Task<IEnumerable<Models.Orders.Order>> GetOrdersAsync();

    //    //Task<Models.Orders.Order> GetOrderAsync(int orderId);

    //    //Task<bool> CancelOrderAsync(int orderId);

    //    //OrderCheckout MapOrderToBasket(Models.Orders.Order order);
    //}
}
