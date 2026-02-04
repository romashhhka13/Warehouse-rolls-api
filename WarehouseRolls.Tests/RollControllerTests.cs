using Microsoft.AspNetCore.Mvc;
using Moq;
using WarehouseRolls.Controllers;
using WarehouseRolls.Dtos;
using WarehouseRolls.Services;

namespace WarehouseRolls.Tests;

public class RollsControllerTests
{
    private readonly Mock<IRollService> _mockService;
    private readonly RollsController _controller;

    public RollsControllerTests()
    {
        _mockService = new Mock<IRollService>();
        _controller = new RollsController(_mockService.Object);
    }


    // ========== CREATE ==========


    // Корректный запрос на добавление рулона на склад
    [Fact]
    public async Task Create_ValidDto_ReturnsCreated()
    {
        var dto = new CreateRollDto { Length = 15.0, Weight = 150.0 };
        var createdRoll = new RollDto
        {
            Id = Guid.NewGuid(),
            Length = 15.0,
            Weight = 150.0
        };
        _mockService.Setup(s => s.CreateRollAsync(dto, It.IsAny<CancellationToken>()))
             .ReturnsAsync(createdRoll);

        var result = await _controller.Create(dto, default);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        var returnedRoll = Assert.IsType<RollDto>(createdResult.Value);
        Assert.Equal(createdRoll.Id, returnedRoll.Id);
        Assert.Equal(15.0, returnedRoll.Length);
    }

    // Невалиндые параметры
    [Fact]
    public async Task Create_InvalidDto_ReturnsBadRequest()
    {
        var invalidDto = new CreateRollDto { Length = 0, Weight = -50.0 };
        _controller.ModelState.AddModelError("Length", "Длина должна быть больше 0");
        _controller.ModelState.AddModelError("Weight", "Вес должен быть больше 0");

        var result = await _controller.Create(invalidDto, default);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }


    // ========== DELETE ==========


    // Коректное удаление
    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        var rollId = Guid.NewGuid();
        var deletedRoll = new RollDto { Id = rollId, Length = 10.0, Weight = 100.0 };
        _mockService.Setup(s => s.DeleteRollAsync(rollId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(deletedRoll);

        var result = await _controller.Delete(rollId, default);

        var deletedResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRoll = Assert.IsType<RollDto>(deletedResult.Value);
        Assert.Equal(deletedRoll.Id, returnedRoll.Id);
        Assert.Equal(deletedRoll.Length, returnedRoll.Length);
    }

    // Удаление не существующего рулона
    [Fact]
    public async Task Delete_NonExistentId_ReturnsNotFound()
    {
        var nonExistentId = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteRollAsync(nonExistentId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((RollDto?)null);

        var result = await _controller.Delete(nonExistentId, default);

        Assert.IsType<NotFoundObjectResult>(result.Result);
    }


    // ========== GET ==========


    // Получение всех рулонов без фильтра
    [Fact]
    public async Task GetAll_NoFilter_ReturnsOkWithRolls()
    {
        var rolls = new List<RollDto>
        {
            new() { Id = Guid.NewGuid(), Length = 10.0, Weight = 100.0 },
            new() { Id = Guid.NewGuid(), Length = 20.0, Weight = 200.0 }
        };
        _mockService.Setup(s => s.GetFilteredRollsAsync(It.IsAny<RollFilterDto>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(rolls);

        var result = await _controller.GetAll(default);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRolls = Assert.IsType<List<RollDto>>(okResult.Value);
        Assert.Equal(2, returnedRolls.Count);
    }

    // Получние рулонов с фильтрацией
    [Fact]
    public async Task GetFiltered_WeightLengthFilter_ReturnsOkWithRolls()
    {
        var rolls = new List<RollDto>
        {
            new() { Id = Guid.NewGuid(), Length = 20.0, Weight = 200.0 },
        };
        var filter = new RollFilterDto
        {
            LengthMin = 15.0,
            LengthMax = 25.0,
            WeightMin = 150.0,
            WeightMax = 220.0
        };

        _mockService.Setup(s => s.GetFilteredRollsAsync(It.IsAny<RollFilterDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(rolls);

        var result = await _controller.GetFiltered(filter, default);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedRolls = Assert.IsType<List<RollDto>>(okResult.Value);
        Assert.Single(returnedRolls);
        Assert.Contains(returnedRolls, r => r.Weight == 200.0);
        Assert.Contains(returnedRolls, r => r.Length == 20.0);
    }

    // Получение статистики
    [Fact]
    public async Task GetStatistics_ValidPeriod_ReturnsOkWithStats()
    {
        var periodStart = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 2, 3, 23, 59, 59, TimeSpan.Zero);
        var stats = new RollStatisticsDto
        {
            CreatedCount = 5,
            DeletedCount = 2,
            AverageLength = 15.5
        };
        _mockService.Setup(s => s.GetStatisticsAsync(periodStart, periodEnd, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(stats);

        var result = await _controller.GetStatistics(periodStart, periodEnd, default);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedStats = Assert.IsType<RollStatisticsDto>(okResult.Value);
        Assert.Equal(5, returnedStats.CreatedCount);
        _mockService.Verify(s => s.GetStatisticsAsync(periodStart, periodEnd, It.IsAny<CancellationToken>()), Times.Once);
    }

    // Получение статистики - periosStart > periodEnd
    [Fact]
    public async Task GetStatistics_InvalidPeriod_ReturnsBadRequest()
    {
        var periodStart = new DateTimeOffset(2026, 2, 3, 23, 59, 59, TimeSpan.Zero);
        var periodEnd = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);

        var result = await _controller.GetStatistics(periodStart, periodEnd, default);

        var okResult = Assert.IsType<BadRequestObjectResult>(result.Result);
    }
}
