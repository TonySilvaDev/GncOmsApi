using System.Text.Json.Nodes;

namespace GncOmsApi.DTO
{
    public class ChangeStatusRequestDto
    {
        public string NuevoEstatus { get; set; }
        public string Origen { get; set; }
        public JsonObject? Campos { get; set; }
        public JsonObject? Metadata { get; set; }
    }
}
