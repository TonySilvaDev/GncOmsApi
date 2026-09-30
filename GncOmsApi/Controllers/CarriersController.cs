using GncOmsApi.DTO;
using GncOmsApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GncOmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CarriersController(ICarrierService carrierService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(CarriersFilterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCarriers([FromQuery] CarriersFilterRequestDto requestDto)
        {
            var response = await carrierService.GetCarriersAsync(requestDto);

            return Ok(response);
        }
    }
}
