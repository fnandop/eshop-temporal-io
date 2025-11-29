using System.Threading;
using Microsoft.Extensions.Options;
using Temporalio.Client;

namespace PaymentProcessor
{
    public static class PaymentProcessorApi
    {
        public static void MapPaymentProcessorEndpoints(this IEndpointRouteBuilder endpoints)
        {

            // RouteGroupBuilder for catalog endpoints
            var vApi = endpoints.NewVersionedApi("Payment");
            var v1 = vApi.MapGroup("/api/payment");



            v1.MapPost("confirm", async (CancellationToken cancellationToken,
                ConfirmPaymentRequest @event,
                ITemporalClient temporalClient,
                IOptionsMonitor<PaymentOptions> options,
                ILogger<ConfirmPaymentRequest> logger) => // Use non-generic ILogger to avoid CS0718
            {
                logger.LogInformation("Processing payment confirmation request for OrderId: {OrderId}, OrderGuid: {OrderGuid}", @event.OrderId, @event.OrderyGuid);

                // Simulate payment flow
                //await Task.Delay(5000, cancellationToken);

                var workflowId = $"Payment_Processing_mock{@event.OrderyGuid}";
                await temporalClient.StartWorkflowAsync((PaymentWorkflowMockDelay wf) => wf.RunAsync(@event.OrderId, @event.OrderyGuid), new WorkflowOptions(workflowId, "eshop-payment-mock-task-queue") );

                return Results.Ok(new { Message = "Processing payment confirmation started." });
            }).WithName("ConfirmPayment")
                .WithSummary("Confirm Payment")
                .WithDescription("Request payment confirmation")
                .WithTags("payment");
        }

        public record ConfirmPaymentRequest(int OrderId, string OrderyGuid);
    }
}
