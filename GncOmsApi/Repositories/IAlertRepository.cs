using GncOmsApi.Models;

namespace GncOmsApi.Repositories
{
    public interface IAlertRepository
    {
        Task<List<IntegrationAlerts>> GetAlertsAsync(bool? resuelto, string? tipo);
        Task<IntegrationAlerts?> GetAlertByIdAsync(Guid alertId);
        Task UpdateAlertAsync(IntegrationAlerts integrationAlert);
    }
}
