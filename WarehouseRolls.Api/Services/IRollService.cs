using Microsoft.EntityFrameworkCore;
using WarehouseRolls.Dtos;
using WarehouseRolls.Models;
using WarehouseRolls.Repositories;

/// <summary>
/// Бизнес-логика для рулонов
/// </summary>
namespace WarehouseRolls.Services
{
    public interface IRollService
    {
        /// <summary>
        /// Создать новый рулон
        /// </summary>
        Task<RollDto> CreateRollAsync(CreateRollDto dto, CancellationToken ct = default);

        /// <summary>
        /// Получить рулон по Id
        /// </summary>
        Task<RollDto?> GetRollAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Удалить рулон
        /// </summary>
        Task<RollDto?> DeleteRollAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Получить список рулонов с фильтрацией
        /// </summary>
        Task<List<RollDto>> GetFilteredRollsAsync(RollFilterDto filter, CancellationToken ct = default);

        /// <summary>
        /// Получить статистику за период
        /// </summary>
        Task<RollStatisticsDto> GetStatisticsAsync(
            DateTimeOffset periodStart,
            DateTimeOffset periodEnd,
            CancellationToken ct = default);
    }
}