using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WarehouseRolls.Dtos;
using WarehouseRolls.Services;

namespace WarehouseRolls.Controllers
{
    [ApiController]
    [Route("api/rolls")]
    public class RollsController : ControllerBase
    {
        private readonly RollService _rollService;
        private readonly ILogger<RollsController> _logger;

        public RollsController(RollService rollService, ILogger<RollsController> logger)
        {
            _rollService = rollService;
            _logger = logger;
        }

        /// <summary>
        /// Добавить новый рулон на склад
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RollDto>> Create(
            [FromBody] CreateRollDto dto,
             CancellationToken ct)
        {
            try
            {
                var roll = await _rollService.CreateRollAsync(dto, ct);
                return CreatedAtAction(nameof(GetById), new { id = roll.Id }, roll);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка создания руллона");
                return StatusCode(500, new { error = "Ошибка базы данных при создании рулона" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при создании рулона");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить рулон по идентификатору
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RollDto>> GetById(
            [FromRoute] Guid id,
             CancellationToken ct)
        {
            try
            {
                var roll = await _rollService.GetRollAsync(id, ct);
                if (roll == null)
                    return NotFound(new { error = "Рулон не найден" });

                return Ok(roll);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка базы данных при выборке руллона: {RollId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Удалить рулон со склада
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<RollDto>> Delete(
            [FromRoute] Guid id,
             CancellationToken ct)
        {
            try
            {
                var roll = await _rollService.DeleteRollAsync(id, ct);
                if (roll == null)
                    return NotFound(new { error = "Рулон не найден" });

                return Ok(roll);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка базы данных при удалении руллона: {RollId}", id);
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить список рулонов с фильтрацией
        /// </summary>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<RollDto>>> GetFiltered(
            [FromQuery] RollFilterDto filter,
             CancellationToken ct)
        {
            try
            {
                var rolls = await _rollService.GetFilteredRollsAsync(filter, ct);
                return Ok(rolls);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка базы данных при выборке отфильтрованных списков");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить список всех рулонов (без фильтрации)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RollDto>>> GetAll(CancellationToken ct)
        {
            try
            {
                var filter = new RollFilterDto();
                var rolls = await _rollService.GetFilteredRollsAsync(filter, ct);
                return Ok(rolls);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка базы данных при выборке всех списков");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

        /// <summary>
        /// Получить статистику по рулонам за период
        /// </summary>
        [HttpGet("statistics")]
        public async Task<ActionResult<RollStatisticsDto>> GetStatistics(
            [FromQuery] DateTimeOffset periodStart,
            [FromQuery] DateTimeOffset periodEnd,
            CancellationToken ct)
        {
            try
            {
                if (periodStart > periodEnd)
                    return BadRequest(new { error = "periodStart должен быть меньше или равен periodEnd" });

                var stats = await _rollService.GetStatisticsAsync(periodStart, periodEnd, ct);
                return Ok(stats);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Ошибка базы данных при получении статистики");
                return StatusCode(500, new { error = "Ошибка базы данных" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Непредвиденная ошибка при получении статистики");
                return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
            }
        }

    }
}