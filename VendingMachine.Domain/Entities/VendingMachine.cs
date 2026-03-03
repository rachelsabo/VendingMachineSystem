using VendingMachine.Domain.Exceptions;

namespace VendingMachine.Domain.Entities;

public sealed class VendingMachine
{
    private readonly List<Shelf> _shelves = new();

    public Guid Id { get; }
    public string Name { get; private set; }
    public string? Location { get; private set; }
    public int MaxShelves { get; private set; }

    public IReadOnlyCollection<Shelf> Shelves => _shelves.AsReadOnly();

    public VendingMachine(Guid id, string name, int maxShelves, string? location = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Vending machine name cannot be empty.", nameof(name));
        }

        if (maxShelves <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxShelves), "Max shelves must be greater than zero.");
        }

        Id = id;
        Name = name;
        MaxShelves = maxShelves;
        Location = location;
    }

    public Shelf AddShelf(Guid categoryId, int capacity)
    {
        if (_shelves.Count >= MaxShelves)
        {
            throw new MaxShelvesReachedException(Id, MaxShelves);
        }

        var shelf = new Shelf(Guid.NewGuid(), categoryId, capacity);
        _shelves.Add(shelf);
        return shelf;
    }

    public void LoadInventory(Guid shelfId, Product product, int quantity)
    {
        var shelf = _shelves.SingleOrDefault(s => s.Id == shelfId);

        if (shelf is null)
        {
            throw new ProductNotFoundException(product.Id);
        }

        shelf.LoadProduct(product, quantity);
    }

    public void Purchase(Guid productId)
    {
        var shelf = _shelves.FirstOrDefault(s => s.Product is not null && s.Product.Id == productId);

        if (shelf is null)
        {
            throw new ProductNotFoundException(productId);
        }

        shelf.Purchase(productId);
    }
}

