using GncOmsApi.Models;

namespace GncOmsApi.Repositories
{
    public interface ICarrierRepository
    {
        Task<List<CarrierCatalogo>> GetCarriersAsync(bool? activo);
    }
}
