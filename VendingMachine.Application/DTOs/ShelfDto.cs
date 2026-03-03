namespace VendingMachine.Application.DTOs;

public sealed class ShelfDto
{
    public Guid Id { get; init; }
    public Guid ProductCategoryId { get; init; }
    public int Capacity { get; init; }
    public int Inventory { get; init; }
}

