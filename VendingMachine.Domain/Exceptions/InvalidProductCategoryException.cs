namespace VendingMachine.Domain.Exceptions;

public sealed class InvalidProductCategoryException : DomainException
{
    public Guid ShelfId { get; }
    public Guid ExpectedCategoryId { get; }
    public Guid ActualCategoryId { get; }

    public InvalidProductCategoryException(Guid shelfId, Guid expectedCategoryId, Guid actualCategoryId)
        : base($"Product category '{actualCategoryId}' does not match shelf '{shelfId}' category '{expectedCategoryId}'.")
    {
        ShelfId = shelfId;
        ExpectedCategoryId = expectedCategoryId;
        ActualCategoryId = actualCategoryId;
    }
}

