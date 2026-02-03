using Microsoft.EntityFrameworkCore;
using WarehouseRolls.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Подключение к PostgreSQL через ENV / appsettings.json --- //
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Host=localhost;Port=5432;Database=warehouse_rolls;Username=postgres;Password=postgres";

builder.Services.AddDbContext<WarehouseRollsDbContext>(options =>
    options.UseNpgsql(connectionString));

// --- Добавление контроллеров и Swagger --- //
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Для разработки --- //
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
