using GncOmsApi.DTO;
using GncOmsApi.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace GncOmsApi.Middlewares
{
    public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            int statusCode = StatusCodes.Status500InternalServerError;
            string errorCode = "ErrorInterno";
            string mensaje = "Ocurrió un error inesperado al procesar la solicitud";
            object? detalle = null;

            if (exception is ApiException apiEx)
            {
                statusCode = apiEx.StatusCode;
                errorCode = apiEx.ErrorCode;
                mensaje = apiEx.Message;
                detalle = apiEx.Detalle;
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = StatusCodes.Status404NotFound;
                errorCode = "RecursoNoEncontrado";
                mensaje = exception.Message;
            }

            var errorResponse = new ApiErrorResponseDto
            {
                Error = errorCode,
                Mensaje = mensaje,
                Detalle = detalle
            };

            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }
    }
}
