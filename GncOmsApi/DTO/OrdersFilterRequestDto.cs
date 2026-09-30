namespace GncOmsApi.DTO
{
    public class OrdersFilterRequestDto
    {
        public string? Estatus { get; set; }
        public string? CarrierId { get; set; }
        public string? MetodoTracking { get; set; }
        public bool? PendienteSyncIRoute { get; set; }
        public int Limit { get; set; } = 100;
    }
}
