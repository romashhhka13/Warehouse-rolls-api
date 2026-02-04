

using System.ComponentModel.DataAnnotations;

namespace WarehouseRolls.Dtos
{
    /// <summary>
    /// DTO для отправки/получения рулона через API
    /// </summary>
    public class RollDto
    {
        public Guid Id { get; set; }
        public double Length { get; set; }
        public double Weight { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
    }

    /// <summary>
    /// DTO для создания нового рулона
    /// </summary>
    public class CreateRollDto
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Длина должна быть больше 0.01 метра")]
        public double Length { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Вес должен быть больше 0.001 кг")]
        public double Weight { get; set; }
    }

    /// <summary>
    /// Параметры фильтрации для GET списка рулонов
    /// </summary>
    public class RollFilterDto
    {
        public Guid? IdMin { get; set; }
        public Guid? IdMax { get; set; }

        public double? WeightMin { get; set; }
        public double? WeightMax { get; set; }

        public double? LengthMin { get; set; }
        public double? LengthMax { get; set; }

        public DateTimeOffset? CreatedAtMin { get; set; }
        public DateTimeOffset? CreatedAtMax { get; set; }

        public DateTimeOffset? DeletedAtMin { get; set; }
        public DateTimeOffset? DeletedAtMax { get; set; }
    }

    /// <summary>
    /// Статистика по рулонам за период
    /// </summary>
    public class RollStatisticsDto
    {
        public int CreatedCount { get; set; }
        public int DeletedCount { get; set; }

        public double? AverageLength { get; set; }
        public double? AverageWeight { get; set; }

        public double? MaxLength { get; set; }
        public double? MinLength { get; set; }

        public double? MaxWeight { get; set; }
        public double? MinWeight { get; set; }

        public double? TotalWeight { get; set; }

        public TimeSpan? MaxStorageDuration { get; set; }
        public TimeSpan? MinStorageDuration { get; set; }

        public DateOnly? DateWithMinRolls { get; set; }
        public DateOnly? DateWithMaxRolls { get; set; }

        public DateOnly? DateWithMinWeight { get; set; }
        public DateOnly? DateWithMaxWeight { get; set; }
    }
}