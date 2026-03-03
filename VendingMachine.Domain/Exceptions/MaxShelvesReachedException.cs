namespace VendingMachine.Domain.Exceptions;

public sealed class MaxShelvesReachedException : DomainException
{
    public Guid VendingMachineId { get; }
    public int MaxShelves { get; }

    public MaxShelvesReachedException(Guid vendingMachineId, int maxShelves)
        : base($"Vending machine '{vendingMachineId}' has reached the maximum number of shelves ({maxShelves}).")
    {
        VendingMachineId = vendingMachineId;
        MaxShelves = maxShelves;
    }
}

