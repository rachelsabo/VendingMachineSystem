namespace VendingMachine.Domain.Exceptions;

public sealed class ShelfCapacityExceededException : DomainException
{
    public Guid ShelfId { get; }
    public int Capacity { get; }
    public int RequestedQuantity { get; }

    public ShelfCapacityExceededException(Guid shelfId, int capacity, int requestedQuantity)
        : base($"Loading {requestedQuantity} items exceeds capacity {capacity} for shelf '{shelfId}'.")
    {
        ShelfId = shelfId;
        Capacity = capacity;
        RequestedQuantity = requestedQuantity;
    }
}

