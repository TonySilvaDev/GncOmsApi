namespace GncOmsApi.DTO
{
    public class AckIRouteResponseDto
    {
        public Guid OrderId { get; set; }
        public bool SyncIrrouteConfirmado { get; set; }
        public DateTime ConfirmadoEn { get; set; }
    }
}
