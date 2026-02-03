using System;

namespace WarehouseRolls.Models
{
    /// <summary>
    /// Сущность рулона металла на складе
    /// </summary>
    public class Roll
    {
        /// <summary>
        /// Уникальный идентификатор рулона
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Длина в метрах
        /// </summary>
        public double Length { get; set; }

        /// <summary>
        /// Вес рулона в килограммах
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// Дата добавления руллона на склад
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Дата удаления рулона со склада
        /// </summary>
        public DateTimeOffset? DeletedAt { get; set; }
    }
}