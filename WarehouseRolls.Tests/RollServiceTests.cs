using WarehouseRolls.Dtos;
using Moq;
using WarehouseRolls.Repositories;
using WarehouseRolls.Services;
using WarehouseRolls.Models;

namespace WarehouseRolls.Tests
{
    public class RollServiceTests
    {
        private readonly Mock<IRollRepository> _mockRepo;
        private readonly RollService _service;

        public RollServiceTests()
        {
            _mockRepo = new Mock<IRollRepository>();
            _service = new RollService(_mockRepo.Object);
        }


        // ========== Тесты CreateRollAsync ========== //


        // Валидные данные, корректный запрос
        [Fact]
        public async Task CreateRollAsync_ValidDto_ReturnsCreatedRoll()
        {
            var dto = new CreateRollDto { Length = 10.5, Weight = 100.0 };
            var expectedRoll = new Roll { Id = Guid.NewGuid(), Length = 10.5, Weight = 100.0, CreatedAt = DateTimeOffset.UtcNow };
            _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Roll>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedRoll);

            var result = await _service.CreateRollAsync(dto);

            Assert.NotNull(result);
            Assert.Equal(10.5, result.Length);
            Assert.Equal(100, result.Weight);
        }

        // Длина = 0
        [Fact]
        public async Task CreateRollAsync_ZeroLength_ThrowsValidationException()
        {
            var dto = new CreateRollDto { Length = 0, Weight = 100.0 };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateRollAsync(dto));
        }

        // Вес отрицательный 
        [Fact]
        public async Task CreateRollAsync_NegativeWeight_ThrowsValidationException()
        {
            var dto = new CreateRollDto { Length = 10.5, Weight = -50.0 };
            await Assert.ThrowsAsync<ArgumentException>(() => _service.CreateRollAsync(dto));
        }

        // Dto = null
        [Fact]
        public async Task CreateRollAsync_NullDto_ThrowsArgumentNullException()
        {
            CreateRollDto nullDto = null!;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.CreateRollAsync(nullDto!));
        }


        // ========== Тесты DeleteRollAsync ========== //


        // Корректное удаление
        [Fact]
        public async Task DeleteRollAsync_ExistingId_ReturnsDeletedRoll()
        {
            var rollId = Guid.NewGuid();
            var existingRoll = new Roll
            {
                Id = rollId,
                Length = 15.0,
                Weight = 150.0,
                CreatedAt = DateTimeOffset.UtcNow
            };
            _mockRepo.Setup(r => r.DeleteAsync(rollId, It.IsAny<CancellationToken>())).ReturnsAsync(existingRoll);

            var result = await _service.DeleteRollAsync(rollId);

            Assert.NotNull(result);
            Assert.Equal(rollId, result.Id);
        }

        // Неизвестный id, вернёт null 
        [Fact]
        public async Task DeleteRollAsync_NonExistentId_ReturnsNull()
        {
            var nonExistentId = Guid.NewGuid();
            _mockRepo.Setup(r => r.DeleteAsync(nonExistentId, It.IsAny<CancellationToken>()))
                     .ReturnsAsync((Roll?)null);

            var result = await _service.DeleteRollAsync(nonExistentId);

            Assert.Null(result);
        }


        // ========== Тесты GetFilteredRollsAsync ========== //


        private readonly List<Roll> _fullDataset = RollTestData.GetFullDataset();

        // Корректно возвращает все рулоны
        [Fact]
        public async Task GetFilteredRolls_NoFilter_ReturnsAllRolls()
        {
            _mockRepo.Setup(r => r.Query()).Returns(_fullDataset.AsQueryable());
            var result = await _service.GetFilteredRollsAsync(new RollFilterDto());
            Assert.Equal(4, result.Count);
        }

        // Фильтрация по весу
        [Fact]
        public async Task GetFilteredRolls_WeightRange_ReturnsFilteredRolls()
        {
            var filter = RollTestData.WeightFilter(120, 180);
            _mockRepo.Setup(r => r.Query()).Returns(_fullDataset.AsQueryable());

            var result = await _service.GetFilteredRollsAsync(filter);

            Assert.Single(result);  // рулоны 200кг и 150кг
            Assert.Contains(result, r => r.Weight == 150);
        }

        // Фильтрация по дате добавления рулона на склад
        [Fact]
        public async Task GetFilteredRolls_DateRange_ReturnsFilteredRolls()
        {
            var filter = RollTestData.DateFilter(RollTestData.BaseDate, RollTestData.BaseDate.AddDays(4));
            _mockRepo.Setup(r => r.Query()).Returns(_fullDataset.AsQueryable());

            var result = await _service.GetFilteredRollsAsync(filter);

            Assert.Equal(2, result.Count);  // рулоны 1 и 2 (CreatedAt в периоде)
        }

        // Нет рулонов, подходящих под фильтр веса
        [Fact]
        public async Task GetFilteredRolls_InvalidFilter_ReturnsEmpty()
        {
            var invalidFilter = new RollFilterDto { WeightMin = 1000 };
            _mockRepo.Setup(r => r.Query()).Returns(_fullDataset.AsQueryable());

            var result = await _service.GetFilteredRollsAsync(invalidFilter);

            Assert.Empty(result);
        }


        // ========== Тесты GetStatisticsAsync ========== //


        // Корректное получение статистики
        [Fact]
        public async Task GetStatisticsAsync_WithFullDataset_ReturnsCorrectStats()
        {
            var periodStart = RollTestData.BaseDate;
            var periodEnd = RollTestData.BaseDate.AddDays(4);

            _mockRepo.Setup(r => r.Query()).Returns(_fullDataset.AsQueryable());

            var stats = await _service.GetStatisticsAsync(periodStart, periodEnd);

            Assert.Equal(2, stats.CreatedCount);        // рулоны 1, 2 добавлены в период
            Assert.Equal(1, stats.DeletedCount);        // рулон 2 удалён в период

            Assert.Equal(15, stats.AverageLength);       // (10+20+15)/3
            Assert.Equal(150, stats.AverageWeight);      // (100+200+150)/3

            Assert.Equal(20, stats.MaxLength);
            Assert.Equal(10, stats.MinLength);
            Assert.Equal(200, stats.MaxWeight);
            Assert.Equal(100, stats.MinWeight);

            Assert.Equal(450, stats.TotalWeight);

            Assert.Equal(TimeSpan.FromDays(1), stats.MaxStorageDuration);
            Assert.Equal(TimeSpan.FromDays(1), stats.MinStorageDuration);

            Assert.NotNull(stats.DateWithMaxRolls);
            Assert.NotNull(stats.DateWithMinRolls);
            Assert.NotNull(stats.DateWithMaxWeight);
            Assert.NotNull(stats.DateWithMinWeight);
        }

        // Пусто на складе
        [Fact]
        public async Task GetStatisticsAsync_EmptyDataset_ReturnsZeroStats()
        {
            var periodStart = RollTestData.BaseDate;
            var periodEnd = RollTestData.BaseDate.AddDays(1);

            _mockRepo.Setup(r => r.Query()).Returns(RollTestData.GetEmptyDataset().AsQueryable());

            var stats = await _service.GetStatisticsAsync(periodStart, periodEnd);

            Assert.Equal(0, stats.CreatedCount);
            Assert.Equal(0, stats.DeletedCount);
            Assert.Null(stats.AverageLength);
            Assert.Null(stats.TotalWeight);
        }

        // Некорректный период
        [Fact]
        public async Task GetStatisticsAsync_InvalidPeriod_ReturnsEmptyStats()
        {
            var periodStart = RollTestData.BaseDate.AddDays(2);
            var periodEnd = RollTestData.BaseDate;

            _mockRepo.Setup(r => r.Query()).Returns(_fullDataset.AsQueryable());

            var stats = await _service.GetStatisticsAsync(periodStart, periodEnd);

            // Assert — просто пустая статистика (или можно добавить ArgumentException)
            Assert.Equal(0, stats.CreatedCount);
            Assert.Equal(0, stats.DeletedCount);
        }

        // Проверка на минимальный и максимальный промежутки времени
        [Fact]
        public async Task GetStatisticsAsync_MaxMinDuration_ReturnsCorrectLifetimes()
        {
            var periodStart = RollTestData.BaseDate;
            var periodEnd = RollTestData.BaseDate.AddDays(5);

            var testRolls = new List<Roll>
            {
                new() { Length = 10, Weight = 100, CreatedAt = periodStart.AddDays(1), DeletedAt = periodStart.AddDays(2) },
                new() { Length = 20, Weight = 200, CreatedAt = periodStart.AddDays(3), DeletedAt = periodEnd }
            };

            _mockRepo.Setup(r => r.Query()).Returns(testRolls.AsQueryable());

            var stats = await _service.GetStatisticsAsync(periodStart, periodEnd);

            Assert.Equal(2, stats.CreatedCount);
            Assert.Equal(2, stats.DeletedCount);
            Assert.Equal(TimeSpan.FromDays(2), stats.MaxStorageDuration);  // самый долгий
            Assert.Equal(TimeSpan.FromDays(1), stats.MinStorageDuration);  // самый короткий
        }
    }
}