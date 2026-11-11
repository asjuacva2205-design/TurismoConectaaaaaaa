using System.Text.Json;

namespace TurismoConecta.web.Client.Services;

/// <summary>
/// Convierte cualquier respuesta de error de la API en un mensaje legible.
/// </summary>
public static class ErrorApi
{
    public static async Task<string> LeerMensajeAsync(
        HttpResponseMessage respuesta,
        string porDefecto = "Ocurrió un error. Intenta de nuevo.")
    {
        var texto = await respuesta.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(texto)) return porDefecto;

        try
        {
            using var documento = JsonDocument.Parse(texto);
            var raiz = documento.RootElement;

            if (raiz.ValueKind == JsonValueKind.Object)
            {
                // Formato 1: { "mensaje": "..." }  → lo que devuelven nuestros controladores
                if (raiz.TryGetProperty("mensaje", out var mensaje) &&
                    mensaje.ValueKind == JsonValueKind.String)
                    return mensaje.GetString()!;

                // Formato 2: { "errors": { "Email": ["..."], "Password": ["..."] } }
                //            → lo que devuelve [ApiController] cuando falla una validación
                if (raiz.TryGetProperty("errors", out var errores) &&
                    errores.ValueKind == JsonValueKind.Object)
                {
                    var mensajes = errores.EnumerateObject()
                        .SelectMany(campo => campo.Value.EnumerateArray())
                        .Select(m => m.GetString())
                        .Where(m => !string.IsNullOrWhiteSpace(m));

                    var unidos = string.Join(" ", mensajes);
                    if (unidos.Length > 0) return unidos;
                }
            }
        }
        catch (JsonException)
        {
            // Formato 3: texto plano, como BadRequest("No se pudo...")
            return texto;
        }

        return porDefecto;
    }
}
