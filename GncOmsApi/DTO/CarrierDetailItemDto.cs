namespace GncOmsApi.DTO
{
    public class CarrierDetailItemDto
    {
        public string CarrierId { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string MetodoTracking { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
