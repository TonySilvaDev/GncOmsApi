using GncOmsApi.DTO;
using Microsoft.AspNetCore.Mvc;

namespace GncOmsApi.Middlewares.Extensions
{
    public static class ApiBehaviorExtensions
    {
        public static IServiceCollection ConfigureApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                // Control de errores para Payload inválido o incompleto en el request
                options.InvalidModelStateResponseFactory = actionContext =>
                {
                    var errores = actionContext.ModelState
                                  .Where(x => x.Value?.Errors.Count > 0)
                                  .ToDictionary(
                                    x => x.Key,
                                    x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                                    );
                    var apiError = new ApiErrorResponseDto
                    {
                        Error = "PayloadInvalido",
                        Mensaje = "La solicitud contiene campos requeridos faltantes o con formato incorrecto",
                        Detalle = errores
                    };

                    return new BadRequestObjectResult(apiError);
                };
            });

            return services;
        }
    }
}
