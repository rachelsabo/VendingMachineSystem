namespace VendingMachine.Domain.Exceptions;

public sealed class OutOfStockException : DomainException
{
    public Guid ProductId { get; }

    public OutOfStockException(Guid productId)
        : base($"Product '{productId}' is out of stock.")
    {
        ProductId = productId;
    }
}

