namespace GncOmsApi.DTO
{
    public class ChangeStatusResponseDto
    {
        public Guid OrderId { get; set; }
        public string EstatusAnterior { get; set; }
        public string EstatusNuevo { get; set; }
        public DateTime ActualizadoEn { get; set; }
    }
}
