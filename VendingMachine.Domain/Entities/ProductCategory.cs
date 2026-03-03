namespace VendingMachine.Domain.Entities;

public sealed class ProductCategory
{
    public Guid Id { get; }
    public string Name { get; private set; }

    public ProductCategory(Guid id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name cannot be empty.", nameof(name));
        }

        Id = id;
        Name = name;
    }
}

