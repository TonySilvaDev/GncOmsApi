namespace GncOmsApi.DTO
{
    public class ApiErrorResponseDto
    {
        public string Error { get; set; } = null!;
        public string Mensaje { get; set; } = null!;
        public object? Detalle { get; set; }
    }
}
