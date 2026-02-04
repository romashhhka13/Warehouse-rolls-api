using WarehouseRolls.Dtos;
using WarehouseRolls.Models;

namespace WarehouseRolls.Tests;

public static class RollTestData
{
    public static readonly DateTimeOffset BaseDate = new(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

    public static List<Roll> GetFullDataset()
    {
        return new List<Roll>
        {
            // 1. Активный рулон (добавлен в период, не удалён)
            new()
            {
                Id = Guid.NewGuid(),
                Length = 10.0,
                Weight = 100.0,
                CreatedAt = BaseDate.AddDays(1)
            },
            
            // 2. Удалённый в периоде
            new()
            {
                Id = Guid.NewGuid(),
                Length = 20.0,
                Weight = 200.0,
                CreatedAt = BaseDate.AddDays(2),
                DeletedAt = BaseDate.AddDays(3)
            },
            
            // 3. Добавлен до периода (был на складе)
            new()
            {
                Id = Guid.NewGuid(),
                Length = 15.0,
                Weight = 150.0,
                CreatedAt = BaseDate.AddDays(-2)
            },
            
            // 4. Добавлен и удалён до периода
            new()
            {
                Id = Guid.NewGuid(),
                Length = 5.0,
                Weight = 50.0,
                CreatedAt = BaseDate.AddDays(-5),
                DeletedAt = BaseDate.AddDays(-3)
            }
        };
    }

    public static List<Roll> GetEmptyDataset() => new List<Roll>();

    public static RollFilterDto WeightFilter(double min = 120, double max = 180) => new()
    {
        WeightMin = min,
        WeightMax = max
    };

    public static RollFilterDto DateFilter(DateTimeOffset? min = null, DateTimeOffset? max = null) => new()
    {
        CreatedAtMin = min,
        CreatedAtMax = max
    };
}
