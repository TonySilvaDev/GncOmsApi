namespace GncOmsApi.Exceptions
{
    public class ApiException(int statusCode, string errorCode, string mensaje, object? detalle = null) : Exception(mensaje)
    {
        public int StatusCode { get; } = statusCode;
        public string ErrorCode { get; } = errorCode;
        public object? Detalle { get; } = detalle;
    }
}
