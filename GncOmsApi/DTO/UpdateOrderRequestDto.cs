namespace GncOmsApi.DTO
{
    public class UpdateOrderRequestDto
    {
        public string TiendaId { get; set; } = string.Empty;
        public string CarrierId { get; set; } = string.Empty;
        public string TrackingId { get; set; } = string.Empty;
    }
}
