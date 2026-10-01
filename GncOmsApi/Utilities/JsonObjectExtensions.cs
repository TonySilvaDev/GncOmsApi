using System.Globalization;
using System.Text.Json.Nodes;
using GncOmsApi.Exceptions;

namespace GncOmsApi.Utilities;

public static class JsonObjectExtensions
{
    public static bool HasField(this JsonObject? fields, string fieldName)
    {
        return fields?.ContainsKey(fieldName) == true;
    }

    public static string? GetStringField(this JsonObject? fields, string fieldName)
    {
        if (fields == null || !fields.TryGetPropertyValue(fieldName, out var node))
        {
            return null;
        }

        if (node is JsonValue value && value.TryGetValue<string>(out var result))
        {
            return result;
        }

        throw new ApiException(
            statusCode: StatusCodes.Status400BadRequest,
            errorCode: "CampoInvalido",
            mensaje: $"El campo '{fieldName}' debe ser una cadena de texto.");
    }

    public static DateTime? GetDateTimeField(this JsonObject? fields, string fieldName)
    {
        if (fields == null || !fields.TryGetPropertyValue(fieldName, out var node))
        {
            return null;
        }

        if (node is JsonValue value &&
            value.TryGetValue<string>(out var rawValue) &&
            DateTimeOffset.TryParse(
                rawValue,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedValue))
        {
            return parsedValue.UtcDateTime;
        }

        throw new ApiException(
            statusCode: StatusCodes.Status400BadRequest,
            errorCode: "FechaInvalida",
            mensaje: $"El campo '{fieldName}' debe ser una fecha y hora ISO 8601 válida.");
    }
}
