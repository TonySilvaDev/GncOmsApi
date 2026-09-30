using System.ComponentModel.DataAnnotations;

namespace GncOmsApi.DTO
{
    public class ShippingAddressDto
    {
        public string Address1 { get; set; } = null!;
        public string? Address2 { get; set; }
        public string Ciudad { get; set; } = null!;
        public string Estado { get; set; } = null!;
        public string CodigoPostal { get; set; } = null!;
        public string Pais { get; set; } = null!;
        public double? Latitud { get; set; }
        public double? Longitud { get; set; }
        public string? Referencia { get; set; }
    }
}
