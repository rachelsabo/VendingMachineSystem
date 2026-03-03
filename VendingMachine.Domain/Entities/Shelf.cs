using VendingMachine.Domain.Exceptions;

namespace VendingMachine.Domain.Entities;

public sealed class Shelf
{
    private Product? _product;

    public Guid Id { get; }
    public Guid ProductCategoryId { get; }
    public int Capacity { get; private set; }
    public int Inventory { get; private set; }

    public Product? Product => _product;

    public Shelf(Guid id, Guid productCategoryId, int capacity)
    {
        if (capacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");
        }

        Id = id;
        ProductCategoryId = productCategoryId;
        Capacity = capacity;
        Inventory = 0;
    }

    public void LoadProduct(Product product, int quantity)
    {
        if (product is null)
        {
            throw new ArgumentNullException(nameof(product));
        }

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        _product ??= product;

        ValidateProductCategory(product);

        if (_product.Id != product.Id && Inventory > 0)
        {
            throw new ShelfCapacityExceededException(Id, Capacity, Inventory + quantity);
        }

        var newInventory = Inventory + quantity;

        if (newInventory > Capacity)
        {
            throw new ShelfCapacityExceededException(Id, Capacity, newInventory);
        }

        _product = product;
        Inventory = newInventory;

        ValidateCapacity();
    }

    public void Purchase(Guid productId)
    {
        if (_product is null || _product.Id != productId)
        {
            throw new ProductNotFoundException(productId);
        }

        if (Inventory <= 0)
        {
            throw new OutOfStockException(productId);
        }

        Inventory--;

        ValidateCapacity();
    }

    public void ValidateCapacity()
    {
        if (Inventory < 0 || Inventory > Capacity)
        {
            throw new ShelfCapacityExceededException(Id, Capacity, Inventory);
        }
    }

    public void ValidateProductCategory()
    {
        if (_product is null)
        {
            return;
        }

        ValidateProductCategory(_product);
    }

    private void ValidateProductCategory(Product product)
    {
        if (product.ProductCategoryId != ProductCategoryId)
        {
            throw new InvalidProductCategoryException(Id, ProductCategoryId, product.ProductCategoryId);
        }
    }
}

