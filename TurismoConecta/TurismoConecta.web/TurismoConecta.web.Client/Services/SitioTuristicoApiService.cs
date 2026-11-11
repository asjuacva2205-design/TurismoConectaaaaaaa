using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Components.Forms;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>
    /// Habla con api/sitios-turisticos y api/categorias/sitios.
    /// Lo usan Inicio.razor (lectura pública) y GestionSitios.razor (administración).
    /// </summary>
    public class SitioTuristicoApiService
    {
        private const long TamanoMaximoImagen = 5 * 1024 * 1024;
        private const string ErrorConexion = "No se pudo conectar con el servidor. Revisa que la API esté encendida.";

        private readonly HttpClient _http;
        public SitioTuristicoApiService(HttpClient http) => _http = http;

        // ═════════════ LECTURA PÚBLICA ═════════════

        public async Task<List<SitioTuristicoDto>> ListarAsync(
            bool soloDestacados = false,
            int? idCategoria = null,
            CancellationToken ct = default)
        {
            var url = $"api/sitios-turisticos?soloDestacados={soloDestacados.ToString().ToLower()}";
            if (idCategoria.HasValue)
                url += $"&idCategoria={idCategoria.Value}";

            try
            {
                return await _http.GetFromJsonAsync<List<SitioTuristicoDto>>(url, ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Console.WriteLine($"[SitioTuristicoApiService] No se pudieron cargar los sitios: {ex.Message}");
                return new();
            }
        }

        public async Task<List<CategoriaDto>> ListarCategoriasAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<CategoriaDto>>("api/categorias/sitios", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Console.WriteLine($"[SitioTuristicoApiService] No se pudieron cargar las categorías: {ex.Message}");
                return new();
            }
        }

        // ═════════════ GESTIÓN ═════════════

        public async Task<(PanelSitiosDto? Panel, string? Error)> ObtenerPanelAsync(CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.GetAsync("api/sitios-turisticos/gestion", ct);

                if (respuesta.IsSuccessStatusCode)
                    return (await respuesta.Content.ReadFromJsonAsync<PanelSitiosDto>(cancellationToken: ct), null);

                if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                    return (null, "Tu sesión expiró o no tienes permiso para gestionar sitios turísticos.");

                return (null, await ErrorApi.LeerMensajeAsync(respuesta, "No se pudo cargar el panel."));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (null, ErrorConexion);
            }
        }

        public async Task<(bool Exito, int Id, string? Error)> CrearAsync(SitioTuristicoFormulario formulario, CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.PostAsJsonAsync("api/sitios-turisticos", formulario, ct);
                if (!respuesta.IsSuccessStatusCode)
                    return (false, 0, await ErrorApi.LeerMensajeAsync(respuesta, "No se pudo crear el sitio."));

                var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaCrear>(cancellationToken: ct);
                return (true, cuerpo?.IdSitioTuristico ?? 0, null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, 0, ErrorConexion);
            }
        }

        public async Task<(bool Exito, string? Error)> EditarAsync(int id, SitioTuristicoFormulario formulario, CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.PutAsJsonAsync($"api/sitios-turisticos/{id}", formulario, ct);
                return respuesta.IsSuccessStatusCode
                    ? (true, null)
                    : (false, await ErrorApi.LeerMensajeAsync(respuesta, "No se pudo guardar el sitio."));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, ErrorConexion);
            }
        }

        public async Task<(bool Exito, string? Error)> CambiarEstadoAsync(int id, bool activo, CancellationToken ct = default)
        {
            try
            {
                var url = $"api/sitios-turisticos/{id}/estado?activo={activo.ToString().ToLower()}";
                var respuesta = await _http.PatchAsync(url, content: null, ct);
                return respuesta.IsSuccessStatusCode
                    ? (true, null)
                    : (false, await ErrorApi.LeerMensajeAsync(respuesta, "No se pudo cambiar el estado."));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, ErrorConexion);
            }
        }

        public async Task<(bool Exito, string? Url, string? Error)> SubirImagenAsync(IBrowserFile archivo, CancellationToken ct = default)
        {
            if (archivo.Size > TamanoMaximoImagen)
                return (false, null, "La imagen no puede superar 5 MB.");

            try
            {
                using var contenido = new MultipartFormDataContent();
                await using var stream = archivo.OpenReadStream(TamanoMaximoImagen, ct);

                var parteArchivo = new StreamContent(stream);
                parteArchivo.Headers.ContentType = new MediaTypeHeaderValue(archivo.ContentType);
                contenido.Add(parteArchivo, "archivo", archivo.Name);

                var respuesta = await _http.PostAsync("api/sitios-turisticos/subir-imagen", contenido, ct);
                if (!respuesta.IsSuccessStatusCode)
                    return (false, null, await ErrorApi.LeerMensajeAsync(respuesta, "No se pudo subir la imagen."));

                var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaSubida>(cancellationToken: ct);
                return (true, cuerpo?.Url, null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException)
            {
                return (false, null, ErrorConexion);
            }
        }

        // Formas de las respuestas JSON (solo se usan aquí dentro)
        private sealed class RespuestaCrear { public int IdSitioTuristico { get; set; } }
        private sealed class RespuestaSubida { public string? Url { get; set; } }
    }

    // ═════════════ DTOs (copias de los de la API) ═════════════

    public class SitioTuristicoDto
    {
        public int IdSitioTuristico { get; set; }
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? ImagenUrl { get; set; }

        public int IdMunicipio { get; set; }
        public string NombreMunicipio { get; set; } = "";
        public string? Clima { get; set; }

        public int? Altitud { get; set; }
        public bool Destacado { get; set; }
        public bool Activo { get; set; }

        public int? IdCategoria { get; set; }
        public string? NombreCategoria { get; set; }
        public string? IconoCategoria { get; set; }

        public double? Calificacion { get; set; }
    }

    public class PanelSitiosDto
    {
        public bool EsAdminGeneral { get; set; }
        public int? IdMunicipioAsignado { get; set; }
        public string? NombreMunicipioAsignado { get; set; }
        public List<SitioTuristicoDto> Sitios { get; set; } = new();
    }
}
