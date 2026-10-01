using System.ComponentModel.DataAnnotations;

namespace GncOmsApi.DTO
{
    public class CreateOrderRequestDto
    {
        [Required(ErrorMessage = "El campo OrderNumber es requerido")]
        public string OrderNumber { get; set; }
        [Required(ErrorMessage = "El campo Channel es requerido")]
        public string Channel { get; set; }
        [Required(ErrorMessage = "El campo ExternalDestinationId es requerido")]
        public string ExternalDestinationId { get; set; }
        [Required(ErrorMessage = "El campo Origen es requerido")]
        public string Origen { get; set; }
        [Required(ErrorMessage = "Customer es requerido")]
        public CustomerDto Customer { get; set; }
        [Required(ErrorMessage = "ShippingAddress es requerido")]
        public ShippingAddressDto ShippingAddress { get; set; }
        [Required]
        [MinLength(1, ErrorMessage = "La orden debe contar con al menos un articulo")]
        public List<OrderItemDto> Items { get; set; }
    }
}
