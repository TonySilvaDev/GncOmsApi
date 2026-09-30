namespace GncOmsApi.DTO
{
    public class IntegrationAlertResponseDto
    {
        public List<IntegrationAlertDetailDto> Alerts { get; set; } = new List<IntegrationAlertDetailDto>();
    }
}
