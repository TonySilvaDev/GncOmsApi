using GncOmsApi.DTO;
using GncOmsApi.Exceptions;
using GncOmsApi.Services;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.AspNetCore.Mvc;

namespace GncOmsApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController(IOrderService orderService) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(CreateOrderResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(CreateOrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto requestDto)
        {
            var response = await orderService.ProcessOrderAsync(requestDto);

            if (response.YaExistia)
            {
                return Ok(response);
            }

            return CreatedAtAction(nameof(CreateOrder), new { id = response.OrderId }, response);
        }

        [HttpGet("lookup")]
        [ProducesResponseType(typeof(OrderLookupResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> LookupOrderByNumber([FromQuery] SearchOrderQueryDto query)
        {
            var response = await orderService.LookupOrderByNumber(query.OrderNumber);

            return Ok(response);
        }

        [HttpPatch("{orderId:guid}")]
        [ProducesResponseType(typeof(UpdateOrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateOrder([FromRoute] Guid orderId, [FromBody] UpdateOrderRequestDto requestDto)
        {
            if (requestDto == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status400BadRequest,
                    errorCode: "PayloadInvalido",
                    mensaje: "Todos los campos estan vacios"
                    );
            }

            var response = await orderService.UpdateOrderAsync(orderId, requestDto);

            return Ok(response);
        }

        [HttpGet("{orderId:guid}")]
        [ProducesResponseType(typeof(OrderDetailResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderById([FromRoute] Guid orderId)
        {
            var response = await orderService.GetOrderByIdWithDetailsAsync(orderId);

            return Ok(response);
        }

        [HttpGet("{orderId:guid}/history")]
        [ProducesResponseType(typeof(OrderHistoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrderHistoryById([FromRoute] Guid orderId)
        {
            var response = await orderService.GetOrderHistoryByIdAsync(orderId);

            return Ok(response);
        }

        [HttpGet]
        [ProducesResponseType(typeof(OrdersFilterResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetOrders([FromQuery] OrdersFilterRequestDto requestDto)
        {
            var response = await orderService.GetOrdersByFilterAsync(requestDto);

            return Ok(response);
        }

        [HttpPost("{orderId:guid}/ack-irroute-sync")]
        [ProducesResponseType(typeof(AckIRouteResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AckIrroutSync([FromRoute] Guid orderId)
        {
            var response = await orderService.ConfirmIrrouteSyncAsync(orderId);

            return Ok(response);
        }

        [HttpPost("{orderId:guid}/status")]
        [ProducesResponseType(typeof(ChangeStatusResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiErrorResponseDto), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangeOrderStatys([FromRoute] Guid orderId, [FromBody] ChangeStatusRequestDto requestDto)
        {
            var response = await orderService.ChangeStatusAsync(orderId, requestDto);

            return Ok(response);
        }
    }
}
