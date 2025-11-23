
using System.Text.Json.Serialization;
using Refit;

namespace Temporal.Workflow;

public interface IPaymentsService
{
    [Post("/api/payment/confirm?api-version=1.0")]
    Task ConfirmPaymentAsync([Body] PaymentRequest paymentRequest);
}

public class PaymentRequest
{
    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    // Keeping the exact name from your JSON: "orderyGuid"
    [JsonPropertyName("orderyGuid")]
    public required string  OrderyGuid { get; set; } 
}

