using GncOmsApi.DTO;
using GncOmsApi.Repositories;

namespace GncOmsApi.Services
{
    public class CarrierService(ICarrierRepository repository) : ICarrierService
    {
        public async Task<CarriersFilterResponseDto> GetCarriersAsync(CarriersFilterRequestDto requestDto)
        {
            var carriers = await repository.GetCarriersAsync(requestDto.Activo);

            var mappedCarriers = carriers.Select(x => new CarrierDetailItemDto
            {
                CarrierId = x.ExternalCarrierId,
                Nombre = x.Nombre,
                MetodoTracking = x.MetodoTracking,
                Activo = x.Activo
            }).ToList();

            return new CarriersFilterResponseDto
            {
                Carriers = mappedCarriers
            };
        }
    }
}
