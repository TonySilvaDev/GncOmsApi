using GncOmsApi.DTO;

namespace GncOmsApi.Services
{
    public interface ICarrierService
    {
        Task<CarriersFilterResponseDto> GetCarriersAsync(CarriersFilterRequestDto requestDto);
    }
}
