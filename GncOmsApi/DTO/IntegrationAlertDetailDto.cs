namespace GncOmsApi.DTO
{
    public class IntegrationAlertDetailDto
    {
        public Guid AlertId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public Guid OrderId { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public DateTime CreadoEn { get; set; }
        public bool Resuelto { get; set; }
    }
}
