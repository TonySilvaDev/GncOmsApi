using GncOmsApi.DTO;
using GncOmsApi.Exceptions;
using GncOmsApi.Repositories;

namespace GncOmsApi.Services
{
    public class AlertService(IAlertRepository repository) : IAlertService
    {
        public async Task<IntegrationAlertResponseDto> GetAlertsAsync(IntegrationAlertRequestDto requestDto)
        {
            var alerts = await repository.GetAlertsAsync(requestDto.Resuelto, requestDto.Tipo);

            var mapperAlerts = alerts.Select(x => new IntegrationAlertDetailDto
            {
                AlertId = x.AlertId,
                OrderId = x.PedidoId,
                Tipo = x.Tipo,
                Detalle = x.Detalle,
                CreadoEn = x.CreadoEn,
                Resuelto = x.Resuelto
            }).ToList();

            return new IntegrationAlertResponseDto
            {
                Alerts = mapperAlerts
            };
        }

        public async Task<ResolveAlertResponseDto> ResolveAlertAsync(Guid alertId)
        {
            var alert = await repository.GetAlertByIdAsync(alertId);

            if (alert == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "AlertaNoEncontrada",
                    mensaje: "No se encontró la alerta",
                    detalle: $"No se encontró la alerta con ID {alertId}"
                    );
            }

            if (alert.Resuelto)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status409Conflict,
                    errorCode: "AlertaYaResuelta",
                    mensaje: "La alerta especificada ya ha sido resuelta previamente",
                    detalle: new { alertId = alert.AlertId, resueltoEn = alert.ResueltoEn }
                    );
            }

            alert.Resuelto = true;
            alert.ResueltoEn = DateTime.UtcNow;

            await repository.UpdateAlertAsync(alert);

            return new ResolveAlertResponseDto
            {
                AlertId = alert.AlertId,
                Resuelto = alert.Resuelto,
                ResueltoEn = alert.ResueltoEn
            };
        }
    }
}
