using Microsoft.AspNetCore.Mvc;
using VendingMachine.Application.DTOs;
using VendingMachine.Application.Services;

namespace VendingMachine.Api.Controllers;

[ApiController]
[Route("api/machines")]
public sealed class VendingMachinesController : ControllerBase
{
    private readonly IVendingMachineService _service;

    public VendingMachinesController(IVendingMachineService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    [HttpPost]
    public async Task<ActionResult<VendingMachineDto>> CreateMachine(
        [FromBody] CreateVendingMachineRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateVendingMachineAsync(request.Name, request.Location, request.MaxShelves, cancellationToken);

        return CreatedAtAction(nameof(GetMachine), new { machineId = result.Id }, result);
    }

    [HttpGet("{machineId:guid}")]
    public async Task<ActionResult<VendingMachineDto>> GetMachine(Guid machineId, CancellationToken cancellationToken)
    {
        var result = await _service.GetVendingMachineAsync(machineId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{machineId:guid}/shelves")]
    public async Task<ActionResult<ShelfDto>> AddShelf(
        Guid machineId,
        [FromBody] AddShelfRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.AddShelfAsync(machineId, request.ProductCategoryId, request.Capacity, cancellationToken);
        return CreatedAtAction(nameof(GetMachine), new { machineId }, result);
    }

    [HttpPost("{machineId:guid}/shelves/{shelfId:guid}/inventory")]
    public async Task<IActionResult> LoadInventory(
        Guid machineId,
        Guid shelfId,
        [FromBody] LoadInventoryRequest request,
        CancellationToken cancellationToken)
    {
        var productDto = new ProductDto
        {
            Id = request.ProductId,
            Name = request.ProductName,
            ProductCategoryId = request.ProductCategoryId
        };

        await _service.LoadInventoryAsync(machineId, shelfId, productDto, request.Quantity, cancellationToken);

        return Ok();
    }

    [HttpPost("{machineId:guid}/purchase")]
    public async Task<IActionResult> Purchase(
        Guid machineId,
        [FromBody] PurchaseProductRequest request,
        CancellationToken cancellationToken)
    {
        await _service.PurchaseProductAsync(machineId, request.ProductId, cancellationToken);
        return Ok();
    }
}

public sealed class CreateVendingMachineRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public int MaxShelves { get; init; }
}

public sealed class AddShelfRequest
{
    public Guid ProductCategoryId { get; init; }
    public int Capacity { get; init; }
}

public sealed class LoadInventoryRequest
{
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid ProductCategoryId { get; init; }
    public int Quantity { get; init; }
}

public sealed class PurchaseProductRequest
{
    public Guid ProductId { get; init; }
}

