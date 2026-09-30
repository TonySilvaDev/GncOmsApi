namespace GncOmsApi.DTO
{
    public class ResolveAlertResponseDto
    {
        public Guid AlertId { get; set; }
        public bool Resuelto { get; set; }
        public DateTime? ResueltoEn { get; set; }
    }
}
