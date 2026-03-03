using VendingMachine.Domain.Entities;
using VendingMachineEntity = VendingMachine.Domain.Entities.VendingMachine;

namespace VendingMachine.Application.Repositories;

public interface IVendingMachineRepository
{
    Task<VendingMachineEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(VendingMachineEntity vendingMachine, CancellationToken cancellationToken = default);

    Task UpdateAsync(VendingMachineEntity vendingMachine, CancellationToken cancellationToken = default);

    Task SaveChangesAsync(CancellationToken cancellationToken = default);
}

