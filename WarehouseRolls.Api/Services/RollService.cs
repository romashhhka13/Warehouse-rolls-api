using Microsoft.EntityFrameworkCore;
using WarehouseRolls.Dtos;
using WarehouseRolls.Models;
using WarehouseRolls.Repositories;

/// <summary>
/// Бизнес-логика для рулонов
/// </summary>
namespace WarehouseRolls.Services
{
    public class RollService
    {
        private readonly IRollRepository _repository;
        public RollService(IRollRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Создать новый рулон
        /// </summary>
        public async Task<RollDto> CreateRollAsync(CreateRollDto dto, CancellationToken ct = default)
        {
            var roll = new Roll
            {
                Id = Guid.NewGuid(),
                Length = dto.Length,
                Weight = dto.Weight,
                CreatedAt = DateTimeOffset.UtcNow
            };

            var created = await _repository.CreateAsync(roll, ct);
            return MapToDto(created);
        }

        /// <summary>
        /// Получить рулон по Id
        /// </summary>
        public async Task<RollDto?> GetRollAsync(Guid id, CancellationToken ct = default)
        {
            var roll = await _repository.GetAsync(id, ct);
            return roll == null ? null : MapToDto(roll);
        }

        /// <summary>
        /// Удалить рулон
        /// </summary>
        public async Task<RollDto?> DeleteRollAsync(Guid id, CancellationToken ct = default)
        {
            var roll = await _repository.DeleteAsync(id, ct);
            return roll == null ? null : MapToDto(roll);
        }

        /// <summary>
        /// Получить список рулонов с фильтрацией
        /// </summary>
        public async Task<List<RollDto>> GetFilteredRollsAsync(RollFilterDto filter, CancellationToken ct = default)
        {
            var query = _repository.Query();

            // Фильтрация по ID 
            // ID как UUID редко сортируют как диапазон (лучше сделать ID int'ом)
            if (filter.IdMin.HasValue)
                query = query.Where(r => r.Id.CompareTo(filter.IdMin.Value) >= 0);
            if (filter.IdMax.HasValue)
                query = query.Where(r => r.Id.CompareTo(filter.IdMax.Value) <= 0);

            // Фильтрация по весу
            if (filter.WeightMin.HasValue)
                query = query.Where(r => r.Weight >= filter.WeightMin.Value);
            if (filter.WeightMax.HasValue)
                query = query.Where(r => r.Weight <= filter.WeightMax.Value);

            // Фильтрация по длине
            if (filter.LengthMin.HasValue)
                query = query.Where(r => r.Length >= filter.LengthMin.Value);
            if (filter.LengthMax.HasValue)
                query = query.Where(r => r.Length <= filter.LengthMax.Value);

            // Фильтрация по дате добавления
            if (filter.CreatedAtMin.HasValue)
                query = query.Where(r => r.CreatedAt >= filter.CreatedAtMin.Value);
            if (filter.CreatedAtMax.HasValue)
                query = query.Where(r => r.CreatedAt <= filter.CreatedAtMax.Value);

            // Фильтрация по дате удаления
            if (filter.DeletedAtMin.HasValue)
                query = query.Where(r => r.DeletedAt.HasValue && r.DeletedAt >= filter.DeletedAtMin.Value);
            if (filter.DeletedAtMax.HasValue)
                query = query.Where(r => r.DeletedAt.HasValue && r.DeletedAt <= filter.DeletedAtMax.Value);

            var rolls = await query.ToListAsync(ct);
            return rolls.Select(MapToDto).ToList();
        }

        /// <summary>
        /// Получить статистику за период
        /// </summary>
        public async Task<RollStatisticsDto> GetStatisticsAsync(
            DateTimeOffset periodStart,
            DateTimeOffset periodEnd,
            CancellationToken ct = default)
        {
            List<Roll> rolls = await _repository.Query().ToListAsync(ct) ?? new List<Roll>();

            // Рулоны, добавленные в период
            var createdInPeriod = rolls
                .Where(r => r.CreatedAt >= periodStart && r.CreatedAt <= periodEnd)
                .ToList();

            // Рулоны, удалённые в период
            var deletedInPeriod = rolls
                .Where(r => r.DeletedAt.HasValue && r.DeletedAt >= periodStart && r.DeletedAt <= periodEnd)
                .ToList();

            // Рулоны, которые находились на складе в этот период
            // (добавлены до конца периода и либо не удалены, либо удалены после начала периода)
            var onWarehouseDuringPeriod = rolls
                .Where(r => r.CreatedAt <= periodEnd &&
                           (!r.DeletedAt.HasValue || r.DeletedAt >= periodStart))
                .ToList();

            var stats = new RollStatisticsDto
            {
                CreatedCount = createdInPeriod.Count,
                DeletedCount = deletedInPeriod.Count,

                AverageLength = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Average(r => r.Length) : null,
                AverageWeight = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Average(r => r.Weight) : null,

                MaxLength = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Max(r => r.Length) : null,
                MinLength = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Min(r => r.Length) : null,

                MaxWeight = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Max(r => r.Weight) : null,
                MinWeight = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Min(r => r.Weight) : null,

                TotalWeight = onWarehouseDuringPeriod.Any() ? onWarehouseDuringPeriod.Sum(r => r.Weight) : null,
            };

            // Промежутки между добавлением и удалением
            // Берутся в расчёт только те рулоны, которые были добавлены и удалены в указанный период
            var removedDurations = rolls
                .Where(r => r.DeletedAt.HasValue
                            && r.CreatedAt >= periodStart
                            && r.CreatedAt <= periodEnd
                            && r.DeletedAt.Value >= periodStart
                            && r.DeletedAt.Value <= periodEnd)
                .Select(r => r.DeletedAt!.Value - r.CreatedAt)
                .ToList();

            if (removedDurations.Any())
            {
                stats.MaxStorageDuration = removedDurations.Max();
                stats.MinStorageDuration = removedDurations.Min();
            }

            // День с минимальным и максимальным количеством рулонов
            CalculateDailyRollCounts(onWarehouseDuringPeriod, periodStart, periodEnd, stats);

            // День с минимальным и максимальным суммарным весом
            CalculateDailyWeights(onWarehouseDuringPeriod, periodStart, periodEnd, stats);

            return stats;
        }

        /// <summary>
        /// Вспомогательный метод для расчёта дневных количеств рулонов
        /// </summary>
        private void CalculateDailyRollCounts(
            List<Roll> rollsOnWarehouse,
            DateTimeOffset periodStart,
            DateTimeOffset periodEnd,
            RollStatisticsDto stats)
        {
            var dailyCounts = new Dictionary<DateOnly, int>();

            // Для каждого дня в периоде считаем, сколько рулонов было на складе
            var currentDate = periodStart.Date;
            while (currentDate <= periodEnd.Date)
            {
                var dateOnly = DateOnly.FromDateTime(currentDate);
                var count = rollsOnWarehouse.Count(r =>
                    r.CreatedAt.Date <= currentDate &&
                    (!r.DeletedAt.HasValue || r.DeletedAt.Value.Date > currentDate));
                dailyCounts[dateOnly] = count;
                currentDate = currentDate.AddDays(1);
            }

            if (dailyCounts.Any())
            {
                var minDay = dailyCounts.OrderBy(x => x.Value).First();
                var maxDay = dailyCounts.OrderByDescending(x => x.Value).First();

                stats.DateWithMinRolls = minDay.Key;
                stats.DateWithMaxRolls = maxDay.Key;
            }
        }

        /// <summary>
        /// Вспомогательный метод для расчёта дневных весов
        /// </summary>
        private void CalculateDailyWeights(
            List<Roll> rollsOnWarehouse,
            DateTimeOffset periodStart,
            DateTimeOffset periodEnd,
            RollStatisticsDto stats)
        {
            var dailyWeights = new Dictionary<DateOnly, double>();

            var currentDate = periodStart.Date;
            while (currentDate <= periodEnd.Date)
            {
                var dateOnly = DateOnly.FromDateTime(currentDate);
                var weight = rollsOnWarehouse
                    .Where(r =>
                        r.CreatedAt.Date <= currentDate &&
                        (!r.DeletedAt.HasValue || r.DeletedAt.Value.Date > currentDate))
                    .Sum(r => r.Weight);
                dailyWeights[dateOnly] = weight;
                currentDate = currentDate.AddDays(1);
            }

            if (dailyWeights.Any())
            {
                var minDay = dailyWeights.OrderBy(x => x.Value).First();
                var maxDay = dailyWeights.OrderByDescending(x => x.Value).First();

                stats.DateWithMinWeight = minDay.Key;
                stats.DateWithMaxWeight = maxDay.Key;
            }
        }

        /// <summary>
        /// Конвертация в DTO
        /// </summary>
        private static RollDto MapToDto(Roll roll) => new()
        {
            Id = roll.Id,
            Length = roll.Length,
            Weight = roll.Weight,
            CreatedAt = roll.CreatedAt,
            DeletedAt = roll.DeletedAt
        };


    }
}