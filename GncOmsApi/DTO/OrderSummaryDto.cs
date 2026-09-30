namespace GncOmsApi.DTO
{
    public class OrderSummaryDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Estatus { get; set; }
        public string CarrierId { get; set; }
    }
}
