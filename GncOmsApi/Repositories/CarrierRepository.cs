using GncOmsApi.Data;
using GncOmsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GncOmsApi.Repositories
{
    public class CarrierRepository(AppDbContext context) : ICarrierRepository
    {
        public async Task<List<CarrierCatalogo>> GetCarriersAsync(bool? activo)
        {
            var query = context.CarrierCatalogo
                        .AsNoTracking()
                        .AsQueryable();

            if (activo.HasValue)
            {
                query = query.Where(x => x.Activo == activo.Value);
            }

            return await query.ToListAsync();
        }
    }
}
