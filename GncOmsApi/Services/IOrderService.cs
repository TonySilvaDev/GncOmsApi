using GncOmsApi.DTO;
using GncOmsApi.Models;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;

namespace GncOmsApi.Services
{
    public interface IOrderService
    {
        Task<CreateOrderResponseDto> ProcessOrderAsync(CreateOrderRequestDto requestDto);
        Task<OrderLookupResponseDto> LookupOrderByNumber(string orderNumber);
        Task<UpdateOrderResponseDto> UpdateOrderAsync(Guid orderId, UpdateOrderRequestDto requestDto);
        Task<OrderDetailResponseDto?> GetOrderByIdWithDetailsAsync(Guid orderId);
        Task<OrderHistoryResponseDto> GetOrderHistoryByIdAsync(Guid orderId);
        Task<OrdersFilterResponseDto> GetOrdersByFilterAsync(OrdersFilterRequestDto requestDto);
        Task<AckIRouteResponseDto> ConfirmIrrouteSyncAsync(Guid orderId);
        Task<ChangeStatusResponseDto> ChangeStatusAsync(Guid orderId, ChangeStatusRequestDto requestDto);
    }
}
