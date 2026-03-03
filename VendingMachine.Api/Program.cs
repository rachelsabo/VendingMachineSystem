using VendingMachine.Api.Middleware;
using VendingMachine.Application.Repositories;
using VendingMachine.Application.Services;
using VendingMachine.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IVendingMachineRepository, InMemoryVendingMachineRepository>();
builder.Services.AddScoped<IVendingMachineService, VendingMachineService>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
