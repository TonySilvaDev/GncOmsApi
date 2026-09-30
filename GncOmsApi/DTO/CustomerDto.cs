namespace GncOmsApi.DTO
{
    public class CustomerDto
    {
        public long? CustomerExternalId { get; set; }
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string? Telefono { get; set; }
    }
}
