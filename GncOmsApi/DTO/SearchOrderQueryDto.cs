using System.ComponentModel.DataAnnotations;

namespace GncOmsApi.DTO
{
    public class SearchOrderQueryDto
    {
        [Required(ErrorMessage = "El parámetro 'orderNumber' es requerido")]
        public string OrderNumber { get; set; } = string.Empty;
    }
}
