using System.Net;
using System.Text.Json;
using VendingMachine.Domain.Exceptions;

namespace VendingMachine.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, code, message) = MapException(exception);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var payload = new
        {
            code,
            message
        };

        var json = JsonSerializer.Serialize(payload);
        await context.Response.WriteAsync(json);
    }

    private static (HttpStatusCode StatusCode, string Code, string Message) MapException(Exception exception)
    {
        return exception switch
        {
            ProductNotFoundException productNotFound => (HttpStatusCode.NotFound, "PRODUCT_NOT_FOUND", productNotFound.Message),
            MaxShelvesReachedException maxShelves => (HttpStatusCode.BadRequest, "MAX_SHELVES_REACHED", maxShelves.Message),
            InvalidProductCategoryException invalidCategory => (HttpStatusCode.BadRequest, "INVALID_PRODUCT_CATEGORY", invalidCategory.Message),
            ShelfCapacityExceededException capacityExceeded => (HttpStatusCode.BadRequest, "SHELF_CAPACITY_EXCEEDED", capacityExceeded.Message),
            OutOfStockException outOfStock => (HttpStatusCode.BadRequest, "OUT_OF_STOCK", outOfStock.Message),
            DomainException domainEx => (HttpStatusCode.BadRequest, "DOMAIN_ERROR", domainEx.Message),
            ArgumentException argEx => (HttpStatusCode.BadRequest, "BAD_REQUEST", argEx.Message),
            InvalidOperationException invalidOp => (HttpStatusCode.NotFound, "NOT_FOUND", invalidOp.Message),
            _ => (HttpStatusCode.InternalServerError, "INTERNAL_SERVER_ERROR", "An unexpected error occurred.")
        };
    }
}

