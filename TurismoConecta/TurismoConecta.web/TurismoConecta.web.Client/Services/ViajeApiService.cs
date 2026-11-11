using System.Net.Http.Json;

namespace TurismoConecta.web.Client.Services
{
    /// <summary>Habla con api/viaje: pueblos con coordenadas y resumen de lo que ofrecen.</summary>
    public class ViajeApiService
    {
        private readonly HttpClient _http;
        public ViajeApiService(HttpClient http) => _http = http;

        public async Task<List<PuebloViajeDto>> ListarPueblosAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<PuebloViajeDto>>("api/viaje/pueblos", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Console.WriteLine($"[ViajeApiService] No se pudieron cargar los pueblos: {ex.Message}");
                return new();
            }
        }
    }

    public class PuebloViajeDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Ofrece { get; set; } = "";
    }
}
