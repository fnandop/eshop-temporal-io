using Temporalio.Activities;
using Temporalio.Client;
using Temporalio.Workflows;

namespace PaymentProcessor
{

    /// <summary>
    /// This work flow is just to simulate the delay between the payment sesstion start and the payment confirmation, otherwise we could consider 
    /// to create a payment worklow as child of the main order workflow EShopWorkflow
    /// </summary>
    [Workflow]
    public class PaymentWorkflowMockDelay
    {



        [WorkflowRun]
        public async Task RunAsync(int OrderId, string orderyGuid)
        {

            //Simulate payment flow delay, between the checkout and the receivment of  callback from an payment service
            await Temporalio.Workflows.Workflow.DelayAsync(TimeSpan.FromSeconds(5));

            // After delay, simulate that a callback from the payment service has been received  signal the workflow that payment succeeded or failed
            // similar to https://docs.stripe.com/payments/checkout/how-checkout-works?payment-ui=stripe-hosted#complete-transaction
            await Temporalio.Workflows.Workflow.ExecuteActivityAsync(
                            (PaymentWorkflowMockDelayActivities act) => act.NotifyOrderPaymentResult(OrderId, orderyGuid),
                            new ActivityOptions { StartToCloseTimeout = TimeSpan.FromMinutes(5) });
        }
    }

    public class PaymentWorkflowMockDelayActivities
    {


        readonly ITemporalClient _temporalClient;
        readonly IOptionsMonitor<PaymentOptions> _options;
        readonly ILogger<PaymentWorkflowMockDelay> _logger;
        public PaymentWorkflowMockDelayActivities(ITemporalClient temporalClient,
                                        IOptionsMonitor<PaymentOptions> options,
                                        ILogger<PaymentWorkflowMockDelay> logger)
        {
            _temporalClient = temporalClient;
            _options = options;
            _logger = logger;
        }



        [Activity]
        public async Task NotifyOrderPaymentResult(int OrderId, string orderyGuid)
        {
            var handle = _temporalClient.GetWorkflowHandle(orderyGuid);

            if (_options.CurrentValue.PaymentSucceeded)
            {
                _logger.LogInformation("Payment succeeded for OrderId: {OrderId}, signaling workflow.", OrderId);
                await handle.SignalAsync("NotifyOrderPaymentSucceeded", []);


            }
            else
            {
                _logger.LogWarning("Payment failed for OrderId: {OrderId}, signaling workflow.", OrderId);
                await handle.SignalAsync("NotifyOrderPaymentFailed", []);
            }
        }
    }
}
