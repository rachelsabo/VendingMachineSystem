namespace VendingMachine.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; }
    public string Name { get; private set; }
    public Guid ProductCategoryId { get; }

    public Product(Guid id, string name, Guid productCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
        ProductCategoryId = productCategoryId;
    }
}

