namespace VendingMachine.Application.DTOs;

public sealed class VendingMachineDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public int MaxShelves { get; init; }
}

