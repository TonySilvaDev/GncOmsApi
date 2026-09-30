namespace GncOmsApi.DTO
{
    public class OrderLookupResponseDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Estatus { get; set; }
    }
}
