using System.Net;
using System.Net.Http.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    public class ReseñaApiService
    {
        private readonly HttpClient _http;
        public ReseñaApiService(HttpClient http) => _http = http;

        // ─────────────── PÚBLICO ───────────────

        // HU-28: reseñas visibles de un municipio o de un negocio
        public async Task<List<ReseñaDto>> ListarAsync(int? idMunicipio, int? idNegocio, CancellationToken ct = default)
        {
            // Sin lugar no pedimos nada: el API devolvería TODAS las reseñas
            if (idMunicipio is null && idNegocio is null) return new();

            try
            {
                return await _http.GetFromJsonAsync<List<ReseñaDto>>($"api/reseñas?{Lugar(idMunicipio, idNegocio)}", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        public async Task<List<ReseñaDto>> ListarRecientesAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ReseñaDto>>("api/reseñas", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        // ─────────────── TURISTA ───────────────

        // La reseña que el usuario ya dejó en ese lugar, o null si aún no ha opinado (204)
        public async Task<ReseñaDto?> ObtenerMiaAsync(int? idMunicipio, int? idNegocio, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.GetAsync($"api/reseñas/mia?{Lugar(idMunicipio, idNegocio)}", ct);
                if (response.StatusCode == HttpStatusCode.NoContent || !response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<ReseñaDto>(cancellationToken: ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return null;
            }
        }

        public async Task<(bool Exito, ReseñaDto? Creada, string? Error)> CrearAsync(CrearReseñaRequest request, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/reseñas", request, ct);
                if (!response.IsSuccessStatusCode)
                    return (false, null, await ErrorApi.LeerMensajeAsync(response, "No se pudo publicar la reseña."));

                return (true, await response.Content.ReadFromJsonAsync<ReseñaDto>(cancellationToken: ct), null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, null, "No se pudo conectar con el servidor.");
            }
        }

        // ─────────────── COMERCIANTE ───────────────

        public async Task<List<ReseñaDto>> ListarDeMisNegociosAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ReseñaDto>>("api/reseñas/mis-negocios", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        // HU-29: una única respuesta pública por reseña
        public Task<(bool Exito, string? Error)> ResponderAsync(int idReseña, string respuesta, CancellationToken ct = default) =>
            EnviarAsync(() => _http.PatchAsJsonAsync($"api/reseñas/{idReseña}/responder", new { Respuesta = respuesta }, ct),
                        "No se pudo publicar la respuesta.");

        // ─────────────── MODERACIÓN (HU-30) ───────────────

        public async Task<List<ReseñaDto>> ListarParaModeracionAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<ReseñaDto>>("api/reseñas/moderacion", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        public Task<(bool Exito, string? Error)> AprobarAsync(int idReseña, CancellationToken ct = default) =>
            EnviarAsync(() => _http.PatchAsync($"api/reseñas/{idReseña}/aprobar", null, ct),
                        "No se pudo aprobar la reseña.");

        public Task<(bool Exito, string? Error)> EliminarAsync(int idReseña, CancellationToken ct = default) =>
            EnviarAsync(() => _http.DeleteAsync($"api/reseñas/{idReseña}", ct),
                        "No se pudo eliminar la reseña.");

        // ─────────────── AYUDANTES ───────────────

        private static string Lugar(int? idMunicipio, int? idNegocio) =>
            idNegocio.HasValue ? $"idNegocio={idNegocio}" : $"idMunicipio={idMunicipio}";

        private static async Task<(bool Exito, string? Error)> EnviarAsync(Func<Task<HttpResponseMessage>> peticion, string porDefecto)
        {
            try
            {
                var response = await peticion();
                return response.IsSuccessStatusCode
                    ? (true, null)
                    : (false, await ErrorApi.LeerMensajeAsync(response, porDefecto));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, "No se pudo conectar con el servidor.");
            }
        }
    }
}
