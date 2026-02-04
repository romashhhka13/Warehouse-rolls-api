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

        public RollsController(RollService rollService)
        {
            _rollService = rollService;
        }

        /// <summary>
        /// Добавить новый рулон на склад
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RollDto>> Create(
            [FromBody] CreateRollDto dto,
             CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var roll = await _rollService.CreateRollAsync(dto, ct);
            return CreatedAtAction(nameof(GetById), new { id = roll.Id }, roll);
        }

        /// <summary>
        /// Получить рулон по идентификатору
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RollDto>> GetById(
            [FromRoute] Guid id,
             CancellationToken ct)
        {
            var roll = await _rollService.GetRollAsync(id, ct);
            if (roll == null)
                return NotFound(new { error = "Рулон не найден" });

            return Ok(roll);
        }

        /// <summary>
        /// Удалить рулон со склада
        /// </summary>
        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<RollDto>> Delete(
            [FromRoute] Guid id,
             CancellationToken ct)
        {
            var roll = await _rollService.DeleteRollAsync(id, ct);
            if (roll == null)
                return NotFound(new { error = "Рулон не найден" });

            return Ok(roll);
        }

        /// <summary>
        /// Получить список рулонов с фильтрацией
        /// </summary>
        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<RollDto>>> GetFiltered(
            [FromQuery] RollFilterDto filter,
             CancellationToken ct)
        {
            var rolls = await _rollService.GetFilteredRollsAsync(filter, ct);
            return Ok(rolls);
        }

        /// <summary>
        /// Получить список всех рулонов (без фильтрации)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RollDto>>> GetAll(CancellationToken ct)
        {
            var filter = new RollFilterDto();
            var rolls = await _rollService.GetFilteredRollsAsync(filter, ct);
            return Ok(rolls);
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

            if (periodStart > periodEnd)
                return BadRequest(new { error = "periodStart должен быть меньше или равен periodEnd" });

            var stats = await _rollService.GetStatisticsAsync(periodStart, periodEnd, ct);
            return Ok(stats);
        }

    }
}