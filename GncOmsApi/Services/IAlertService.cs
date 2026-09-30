using GncOmsApi.DTO;

namespace GncOmsApi.Services
{
    public interface IAlertService
    {
        Task<IntegrationAlertResponseDto> GetAlertsAsync(IntegrationAlertRequestDto requestDto);
        Task<ResolveAlertResponseDto> ResolveAlertAsync(Guid alertId);
    }
}
