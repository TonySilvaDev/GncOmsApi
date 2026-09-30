namespace GncOmsApi.DTO
{
    public class IntegrationAlertRequestDto
    {
        public bool Resuelto { get; set; } = false;
        public string? Tipo { get; set; }
    }
}
