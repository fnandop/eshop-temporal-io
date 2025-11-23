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



            v1.MapPost("confirm", async (
                ConfirmPaymentRequest @event,
                ITemporalClient temporalClient,
                IOptionsMonitor<PaymentOptions> options,
                ILogger<ConfirmPaymentRequest> logger) => // Use non-generic ILogger to avoid CS0718
            {
                logger.LogInformation("Processing payment confirmation request for OrderId: {OrderId}, OrderGuid: {OrderGuid}", @event.OrderId, @event.OrderyGuid);

                var handle = temporalClient.GetWorkflowHandle(@event.OrderyGuid);


                if (options.CurrentValue.PaymentSucceeded)
                {
                    logger.LogInformation("Payment succeeded for OrderId: {OrderId}, signaling workflow.", @event.OrderId);
                    await handle.SignalAsync("NotifyOrderPaymentSucceeded", []);


                }
                else
                {
                    logger.LogWarning("Payment failed for OrderId: {OrderId}, signaling workflow.", @event.OrderId);
                    await handle.SignalAsync("NotifyOrderPaymentFailed", []);
                }

                logger.LogInformation("Payment confirmation request handled successfully for OrderId: {OrderId}.", @event.OrderId);
                return Results.Ok(new { Message = "Integration event handled successfully." });
            }).WithName("ConfirmPayment")
                .WithSummary("Confirm Payment")
                .WithDescription("Request payment confirmation")
                .WithTags("payment");
        }

        public record ConfirmPaymentRequest(int OrderId, string OrderyGuid);
    }
}
