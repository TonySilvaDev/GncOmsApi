using GncOmsApi.DTO;
using GncOmsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GncOmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlertsController(IAlertService alertService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IntegrationAlertResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAlerts([FromQuery] IntegrationAlertRequestDto requestDto)
        {
            var response = await alertService.GetAlertsAsync(requestDto);

            return Ok(response);
        }

        [HttpPost("{alertId:guid}/resolver")]
        [ProducesResponseType(typeof(ResolveAlertResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ResolveAlert([FromRoute] Guid alertId)
        {
            var response = await alertService.ResolveAlertAsync(alertId);

            return Ok(response);
        }
    }
}
