namespace GncOmsApi.DTO
{
    public class OrderItemDto
    {
        public string Sku { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Impuesto { get; set; }
    }
}
