using VendingMachine.Application.DTOs;
using VendingMachine.Application.Repositories;
using VendingMachine.Domain.Entities;
using VendingMachineEntity = VendingMachine.Domain.Entities.VendingMachine;

namespace VendingMachine.Application.Services;

public sealed class VendingMachineService : IVendingMachineService
{
    private readonly IVendingMachineRepository _repository;

    public VendingMachineService(IVendingMachineRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<VendingMachineDto> CreateVendingMachineAsync(string name, string? location, int maxShelves, CancellationToken cancellationToken = default)
    {
        var vendingMachine = new VendingMachineEntity(Guid.NewGuid(), name, maxShelves, location);

        await _repository.AddAsync(vendingMachine, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDto(vendingMachine);
    }

    public async Task<VendingMachineDto> GetVendingMachineAsync(Guid vendingMachineId, CancellationToken cancellationToken = default)
    {
        var vendingMachine = await GetVendingMachineOrThrowAsync(vendingMachineId, cancellationToken).ConfigureAwait(false);
        return MapToDto(vendingMachine);
    }

    public async Task<ShelfDto> AddShelfAsync(Guid vendingMachineId, Guid productCategoryId, int capacity, CancellationToken cancellationToken = default)
    {
        var vendingMachine = await GetVendingMachineOrThrowAsync(vendingMachineId, cancellationToken).ConfigureAwait(false);

        var shelf = vendingMachine.AddShelf(productCategoryId, capacity);

        await _repository.UpdateAsync(vendingMachine, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return MapToDto(shelf);
    }

    public async Task LoadInventoryAsync(Guid vendingMachineId, Guid shelfId, ProductDto productDto, int quantity, CancellationToken cancellationToken = default)
    {
        if (productDto is null)
        {
            throw new ArgumentNullException(nameof(productDto));
        }

        var vendingMachine = await GetVendingMachineOrThrowAsync(vendingMachineId, cancellationToken).ConfigureAwait(false);

        var product = new Product(productDto.Id, productDto.Name, productDto.ProductCategoryId);

        vendingMachine.LoadInventory(shelfId, product, quantity);

        await _repository.UpdateAsync(vendingMachine, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task PurchaseProductAsync(Guid vendingMachineId, Guid productId, CancellationToken cancellationToken = default)
    {
        var vendingMachine = await GetVendingMachineOrThrowAsync(vendingMachineId, cancellationToken).ConfigureAwait(false);

        vendingMachine.Purchase(productId);

        await _repository.UpdateAsync(vendingMachine, cancellationToken).ConfigureAwait(false);
        await _repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<VendingMachineEntity> GetVendingMachineOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var vendingMachine = await _repository.GetByIdAsync(id, cancellationToken).ConfigureAwait(false);

        if (vendingMachine is null)
        {
            throw new InvalidOperationException($"Vending machine '{id}' was not found.");
        }

        return vendingMachine;
    }

    private static VendingMachineDto MapToDto(VendingMachineEntity vendingMachine)
    {
        return new VendingMachineDto
        {
            Id = vendingMachine.Id,
            Name = vendingMachine.Name,
            Location = vendingMachine.Location,
            MaxShelves = vendingMachine.MaxShelves
        };
    }

    private static ShelfDto MapToDto(Shelf shelf)
    {
        return new ShelfDto
        {
            Id = shelf.Id,
            ProductCategoryId = shelf.ProductCategoryId,
            Capacity = shelf.Capacity,
            Inventory = shelf.Inventory
        };
    }
}

