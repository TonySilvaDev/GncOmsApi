namespace GncOmsApi.DTO
{
    public class OrderStatusHistoryDto
    {
        public string? EstatusAnterior { get; set; }
        public string EstatusNuevo { get; set; } = null!;
        public string Origen { get; set; } = null!;
        public DateTime Timestamp { get; set; }
    }
}
