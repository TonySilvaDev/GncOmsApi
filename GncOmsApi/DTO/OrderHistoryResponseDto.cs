namespace GncOmsApi.DTO
{
    public class OrderHistoryResponseDto
    {
        public Guid OrderId { get; set; }
        public List<OrderStatusHistoryDto> Historial { get; set; } = new List<OrderStatusHistoryDto>();
    }
}
