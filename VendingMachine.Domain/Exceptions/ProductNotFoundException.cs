namespace VendingMachine.Domain.Exceptions;

public sealed class ProductNotFoundException : DomainException
{
    public Guid ProductId { get; }

    public ProductNotFoundException(Guid productId)
        : base($"Product '{productId}' was not found in the vending machine.")
    {
        ProductId = productId;
    }
}

