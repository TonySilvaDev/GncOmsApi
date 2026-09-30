namespace GncOmsApi.DTO
{
    public class CreateOrderResponseDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public string Estatus { get; set; }
        public DateTime? FechaCreacion { get; set; }
        public bool YaExistia { get; set; }
    }
}
