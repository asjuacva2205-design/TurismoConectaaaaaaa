using System.Net.Http.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>Llamadas HTTP a api/notificaciones (historial y marcar leídas).</summary>
    public class NotificacionApiService
    {
        private readonly HttpClient _http;
        public NotificacionApiService(HttpClient http) => _http = http;

        public async Task<List<NotificacionDto>> ListarAsync(bool soloNoLeidas = false, CancellationToken ct = default)
        {
            try
            {
                var url = $"api/notificaciones?soloNoLeidas={soloNoLeidas.ToString().ToLower()}";
                return await _http.GetFromJsonAsync<List<NotificacionDto>>(url, ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Console.WriteLine($"[NotificacionApiService] No se pudieron cargar: {ex.Message}");
                return new();
            }
        }

        public async Task<bool> MarcarLeidaAsync(int idNotificacion, CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.PatchAsync($"api/notificaciones/{idNotificacion}/leida", content: null, ct);
                return respuesta.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return false;
            }
        }

        public async Task<bool> MarcarTodasLeidasAsync(CancellationToken ct = default)
        {
            try
            {
                var respuesta = await _http.PatchAsync("api/notificaciones/leer-todas", content: null, ct);
                return respuesta.IsSuccessStatusCode;
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return false;
            }
        }
    }
}
