namespace GncOmsApi.DTO
{
    public class OrdersFilterResponseDto
    {
        public int Total { get; set; }
        public List<OrderSummaryDto> Orders { get; set; } = new List<OrderSummaryDto>();
    }
}
