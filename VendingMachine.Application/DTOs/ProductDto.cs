namespace VendingMachine.Application.DTOs;

public sealed class ProductDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid ProductCategoryId { get; init; }
}

