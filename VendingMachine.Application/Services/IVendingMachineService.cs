using VendingMachine.Application.DTOs;

namespace VendingMachine.Application.Services;

public interface IVendingMachineService
{
    Task<VendingMachineDto> CreateVendingMachineAsync(string name, string? location, int maxShelves, CancellationToken cancellationToken = default);

    Task<VendingMachineDto> GetVendingMachineAsync(Guid vendingMachineId, CancellationToken cancellationToken = default);

    Task<ShelfDto> AddShelfAsync(Guid vendingMachineId, Guid productCategoryId, int capacity, CancellationToken cancellationToken = default);

    Task LoadInventoryAsync(Guid vendingMachineId, Guid shelfId, ProductDto product, int quantity, CancellationToken cancellationToken = default);

    Task PurchaseProductAsync(Guid vendingMachineId, Guid productId, CancellationToken cancellationToken = default);
}

