namespace Temporal.Workflow
{
    public record OrderRequest(
      string OrderyGuid,
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

    
}
