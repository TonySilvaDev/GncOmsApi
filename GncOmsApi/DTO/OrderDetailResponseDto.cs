namespace GncOmsApi.DTO
{
    public class OrderDetailResponseDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; } = null!;
        public string Channel { get; set; } = null!;
        public string Estatus { get; set; } = null!;
        public string? TiendaId { get; set; }
        public string? CarrierId { get; set; }
        public string? TrackingId { get; set; }
        public int IntentosReroute { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime? FechaAceptacion { get; set; }
        public DateTime? FechaSurtido { get; set; }
        public DateTime? FechaEntrega { get; set; }
        public bool SyncIrouteConfirmado { get; set; }
        public List<OrderDetailItemDto> Items { get; set; } = new List<OrderDetailItemDto>();
    }
}
