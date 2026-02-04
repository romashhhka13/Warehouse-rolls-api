using Microsoft.EntityFrameworkCore;
using WarehouseRolls.Data;
using WarehouseRolls.Repositories;
using WarehouseRolls.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Npgsql;

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

builder.Services.AddScoped<IRollRepository, EfRollRepository>();
builder.Services.AddScoped<IRollService, RollService>();
builder.Services.AddDbContext<WarehouseRollsDbContext>();

var app = builder.Build();

// Middleware - обработка исключений
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        context.Response.ContentType = "application/json";

        if (exception is ArgumentException or ArgumentNullException)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("{\"error\":\"Данные не валидны\"}");

        }
        else if (exception is NpgsqlException || exception is DbUpdateException)
        {
            context.Response.StatusCode = 503;
            await context.Response.WriteAsync("{\"error\":\"Ошибка базы данных\"}");
        }
        else
        {
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("{\"error\":\"Внутренняя ошибка сервера\"}");
        }
    });
});


// --- Для разработки --- //
if (app.Environment.IsDevelopment())
{

    app.UseSwagger();
    app.UseSwaggerUI();
    // app.UseSwaggerUI(options =>
    // {
    //     options.DocumentTitle = "/openapi/v1.json";
    // });

    // app.MapOpenApi("/openapi/v1.json");
    // app.UseSwaggerUi(options =>
    // {
    //     options.DocumentPath = "/openapi/v1.json";
    // });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
