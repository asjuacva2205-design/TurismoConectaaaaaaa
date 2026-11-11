using System.Globalization;
using System.Text;

namespace TurismoConecta.web.Client.Services;

/// <summary>
/// Guarda la dirección de la API (leída de wwwroot/appsettings.json)
/// y construye URLs completas para imágenes y archivos que sirve la API.
/// </summary>
public class ApiConfig
{
    public const string ImagenPorDefecto =
        "https://images.unsplash.com/photo-1518105779142-d975f22f1b0a?q=80&w=1400&auto=format&fit=crop";

    public string BaseUrl { get; }

    public ApiConfig(string baseUrl)
    {
        BaseUrl = baseUrl.TrimEnd('/');
    }

    public string Url(string ruta)
    {
        if (ruta.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            ruta.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            ruta.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return ruta;
        }

        return $"{BaseUrl}/{ruta.TrimStart('/')}";
    }

    /// <summary>
    /// Devuelve la foto de portada de un municipio:
    /// 1) la que tiene guardada en la BD, 2) la foto por nombre, 3) una genérica.
    /// </summary>
    public string ImagenMunicipio(string? nombre, string? imagenUrl)
    {
        if (!string.IsNullOrWhiteSpace(imagenUrl))
            return Url(imagenUrl);

        if (string.IsNullOrWhiteSpace(nombre))
            return ImagenPorDefecto;

        return Url($"images/municipios/{LimpiarNombre(nombre)}ImgPerfil.jpg");
    }

    /// <summary>
    /// "Villa de Leyva" → "VillaLeyva", "Ráquira" → "Raquira", "Güicán" → "Guican".
    /// </summary>
    private static string LimpiarNombre(string nombre)
    {
        var sinDe = nombre.Replace(" de ", "");
        var descompuesto = sinDe.Normalize(NormalizationForm.FormD);

        var resultado = new StringBuilder(descompuesto.Length);
        foreach (var caracter in descompuesto)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(caracter) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsWhiteSpace(caracter))
                continue;

            resultado.Append(caracter);
        }

        return resultado.ToString().Normalize(NormalizationForm.FormC);
    }
}
