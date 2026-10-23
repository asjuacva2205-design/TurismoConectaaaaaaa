using System.Net.Http.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    public class FavoritoApiService
    {
        private readonly HttpClient _http;
        public FavoritoApiService(HttpClient http) => _http = http;

        // HU-27: municipios y negocios guardados por el usuario en sesión
        public async Task<List<FavoritoItemDto>> ListarMiosAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<FavoritoItemDto>>("api/favoritos/mios", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }

        // Marca o desmarca. Devuelve el estado final (true = quedó guardado)
        public async Task<(bool Exito, bool Marcado, string? Error)> ToggleAsync(int? idMunicipio, int? idNegocio, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/favoritos/toggle", new { IdMunicipio = idMunicipio, IdNegocio = idNegocio }, ct);
                if (!response.IsSuccessStatusCode)
                    return (false, false, await response.Content.ReadAsStringAsync(ct));

                var res = await response.Content.ReadFromJsonAsync<FavoritoToggleRespuesta>(cancellationToken: ct);
                return (true, res?.Marcado ?? false, null);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return (false, false, ex.Message);
            }
        }
    }
}
