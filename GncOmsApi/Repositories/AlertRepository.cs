using GncOmsApi.Data;
using GncOmsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GncOmsApi.Repositories
{
    public class AlertRepository(AppDbContext context) : IAlertRepository
    {
        public async Task<List<IntegrationAlerts>> GetAlertsAsync(bool? resuelto, string? tipo)
        {
            var query = context.IntegrationAlerts
                        .AsNoTracking()
                        .AsQueryable();

            if (resuelto.HasValue)
            {
                query = query.Where(x => x.Resuelto == resuelto.Value);
            }

            if (!string.IsNullOrWhiteSpace(tipo))
            {
                query = query.Where(x => x.Tipo.ToLower() == tipo.ToLower());
            }

            return await query
                        .OrderByDescending(x => x.CreadoEn)
                        .ToListAsync();
        }

        public async Task<IntegrationAlerts?> GetAlertByIdAsync(Guid alertId)
        {
            return await context.IntegrationAlerts
                        .FirstOrDefaultAsync(x => x.AlertId == alertId);
        }

        public async Task UpdateAlertAsync(IntegrationAlerts integrationAlert)
        {
            context.IntegrationAlerts.Update(integrationAlert);
            await context.SaveChangesAsync();
        }
    }
}
