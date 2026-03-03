using System.Collections.Concurrent;
using VendingMachine.Application.Repositories;
using VendingMachine.Domain.Entities;
using VendingMachineEntity = VendingMachine.Domain.Entities.VendingMachine;

namespace VendingMachine.Infrastructure.Repositories;

public sealed class InMemoryVendingMachineRepository : IVendingMachineRepository
{
    // Key: VendingMachine Id, Value: VendingMachine aggregate
    private readonly ConcurrentDictionary<Guid, VendingMachineEntity> _store = new();

    public Task<VendingMachineEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var vendingMachine);
        return Task.FromResult(vendingMachine);
    }

    public Task AddAsync(VendingMachineEntity vendingMachine, CancellationToken cancellationToken = default)
    {
        if (vendingMachine is null)
        {
            throw new ArgumentNullException(nameof(vendingMachine));
        }

        if (!_store.TryAdd(vendingMachine.Id, Clone(vendingMachine)))
        {
            // Overwrite existing entry to behave like an "upsert" for simplicity
            _store[vendingMachine.Id] = Clone(vendingMachine);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(VendingMachineEntity vendingMachine, CancellationToken cancellationToken = default)
    {
        if (vendingMachine is null)
        {
            throw new ArgumentNullException(nameof(vendingMachine));
        }

        _store[vendingMachine.Id] = Clone(vendingMachine);

        return Task.CompletedTask;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // In-memory implementation has no persistence boundary,
        // so SaveChangesAsync is effectively a no-op.
        return Task.CompletedTask;
    }

    private static VendingMachineEntity Clone(VendingMachineEntity source)
    {
        // Shallow clone of the aggregate with its current shelves to avoid
        // external callers mutating internal state without going through the repository.
        var clone = new VendingMachineEntity(source.Id, source.Name, source.MaxShelves, source.Location);

        foreach (var shelf in source.Shelves)
        {
            var newShelf = clone.AddShelf(shelf.ProductCategoryId, shelf.Capacity);

            if (shelf.Product is not null && shelf.Inventory > 0)
            {
                var product = new Product(shelf.Product.Id, shelf.Product.Name, shelf.Product.ProductCategoryId);
                clone.LoadInventory(newShelf.Id, product, shelf.Inventory);
            }
        }

        return clone;
    }
}

