using WarehouseRolls.Models;

namespace WarehouseRolls.Repositories
{
    public interface IRollRepository
    {
        /// <summary>
        /// Добавляет новый рулон
        /// </summary>
        Task<Roll> AddAsync(Roll roll, CancellationToken ct = default);

        /// <summary>
        /// Получить рулон по Id
        /// </summary>
        Task<Roll?> GetAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Удалить рулон
        /// </summary>
        Task<Roll> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Для гибкой фильтрации
        /// </summary>
        IQueryable<Roll> Query();

        /// <summary>
        /// Сохранить изменения
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}