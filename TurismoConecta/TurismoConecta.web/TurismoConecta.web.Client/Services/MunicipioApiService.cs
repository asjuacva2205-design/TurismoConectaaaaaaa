// Services/MunicipioApiService.cs
using System.Net.Http.Json;
using System.Text.Json;

namespace TurismoConecta.web.Client.Services
{
    public class MunicipioApiService
    {
        private readonly HttpClient _http;
        public MunicipioApiService(HttpClient http) => _http = http;

        public async Task<ResultadoPaginado<MunicipioListadoDto>?> ListarAsync(
            int pagina = 1,
            int tamano = 9,
            string? ordenarPor = null,
            bool ascendente = true,
            CancellationToken ct = default)
        {
            var qs = $"?pagina={pagina}&tamano={tamano}";
            if (!string.IsNullOrEmpty(ordenarPor))
                qs += $"&ordenarPor={ordenarPor}&ascendente={ascendente}";

            try { return await _http.GetFromJsonAsync<ResultadoPaginado<MunicipioListadoDto>>($"api/municipios{qs}", ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

        /// <summary>Todos los municipios activos con coordenadas (endpoint api/municipios/mapa).</summary>
        public async Task<List<MunicipioListadoDto>> ListarParaMapaAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<MunicipioListadoDto>>("api/municipios/mapa", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                Console.WriteLine($"[MunicipioApiService] No se pudo cargar el mapa: {ex.Message}");
                return new();
            }
        }

        public async Task<MunicipioFichaDto?> ObtenerFichaAsync(int id, CancellationToken ct = default)
        {
            try { return await _http.GetFromJsonAsync<MunicipioFichaDto>($"api/municipios/{id}", ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }
        // Forma de la respuesta de POST api/municipios: { "idMunicipio": 7 }
        private sealed record RespuestaCrearMunicipio(int IdMunicipio);
        public async Task<(bool Exito, int Id, string? Error)> CrearAsync(
            MunicipioCrearDto dto, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/municipios", dto, ct);
                if (response.IsSuccessStatusCode)
                {
                    var respuesta = await response.Content.ReadFromJsonAsync<RespuestaCrearMunicipio>(cancellationToken: ct);
                    return (true, respuesta?.IdMunicipio ?? 0, null);
                }
                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, 0, err);
            }
            catch (Exception ex) { return (false, 0, ex.Message); }
        }

        public async Task<(bool Exito, string? Error)> EditarAsync(
            int id, MunicipioEditarDto dto, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/municipios/{id}", dto, ct);
                if (response.IsSuccessStatusCode) return (true, null);
                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, err);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        public async Task<List<EtiquetaDto>> ListarEtiquetasAsync(CancellationToken ct = default)
        {
            try { return await _http.GetFromJsonAsync<List<EtiquetaDto>>("api/municipios/etiquetas", ct) ?? new(); }
            catch { return new(); }
        }

        public async Task<(bool Exito, string? Url, string? Error)> SubirImagenAsync(
            Microsoft.AspNetCore.Components.Forms.IBrowserFile archivo,
            CancellationToken ct = default)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(archivo.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(archivo.ContentType);
                content.Add(fileContent, "archivo", archivo.Name);

                var response = await _http.PostAsync("api/municipios/subir-imagen", content, ct);
                if (response.IsSuccessStatusCode)
                {
                    var res = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: ct);
                    if (res.TryGetProperty("url", out var urlProp))
                    {
                        return (true, urlProp.GetString(), null);
                    }
                    return (true, null, null);
                }
                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, null, err);
            }
            catch (Exception ex) { return (false, null, ex.Message); }
        }

        public async Task<List<MunicipioListadoDto>?> BuscarAsync(
            string? texto,
            List<int>? etiquetaIds = null,
            CancellationToken ct = default)
        {
            var query = new List<string>();
            if (!string.IsNullOrWhiteSpace(texto))
                query.Add($"texto={Uri.EscapeDataString(texto)}");

            if (etiquetaIds is { Count: > 0 })
                query.AddRange(etiquetaIds.Select(id => $"etiquetaIds={id}"));

            var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";

            try { return await _http.GetFromJsonAsync<List<MunicipioListadoDto>>($"api/municipios/buscar{qs}", ct); }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException) { return null; }
        }

        public async Task<List<NegocioResumenDto>?> ObtenerNegociosPorMunicipioAsync(int idMunicipio, CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<NegocioResumenDto>>($"api/negocios/por-municipio/{idMunicipio}", ct);
            }
            catch (Exception)
            {
                return new List<NegocioResumenDto>();
            }
        }
    }

    public class ResultadoPaginado<T>
    {
        public List<T> Items { get; set; } = new();
        public int Total { get; set; }
        public int Pagina { get; set; }
        public int Tamano { get; set; }
        public int TotalPaginas { get; set; }
    }

    public class EtiquetaDto
    {
        public int IdEtiqueta { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Icono { get; set; }
        public string? Categoria { get; set; }
        public bool Activo { get; set; }
    }

    public class MunicipioListadoDto
    {
        public int IdMunicipio { get; set; }
        public string Nombre { get; set; } = "";
        public string? ImagenUrl { get; set; }
        public List<string> Etiquetas { get; set; } = new();
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
    }

    public class MunicipioFichaDto : MunicipioListadoDto
    {
        public string? Descripcion { get; set; }
        public string? Clima { get; set; }
        public string? Historia { get; set; }
        public List<FechaRelevanteDto> FechasRelevantes { get; set; } = new();    

    }

    public class FechaRelevanteDto
    {
        public int IdFecha { get; set; }
        // Alias de compatibilidad — EditarMunicipio.razor usa este nombre
        public int IdFechaRelevante
        {
            get => IdFecha;
            set => IdFecha = value;
        }
        public string NombreFestividad { get; set; } = "";
        public DateOnly FechaInicio { get; set; }
        public DateOnly FechaFin { get; set; }
        public string? TipoFestividad { get; set; }
        public string? Descripcion { get; set; }
        public bool EsRecurrente { get; set; } = true;
        // Campo legacy — se conserva por compatibilidad con EditarMunicipio.razor
        public int? MesCelebracion { get; set; }
    }

    public class MunicipioCrearDto
    {
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Clima { get; set; }
        public string? Historia { get; set; }
        public string? ImagenUrl { get; set; }
        public List<FechaRelevanteDto> FechasRelevantes { get; set; } = new();
        public List<string> Etiquetas { get; set; } = new();
    }

    public class MunicipioEditarDto
    {
        public string Nombre { get; set; } = "";
        public string? Descripcion { get; set; }
        public string? Clima { get; set; }
        public string? Historia { get; set; }
        public string? ImagenUrl { get; set; }
        public List<FechaRelevanteDto> FechasRelevantes { get; set; } = new();
        public List<string> Etiquetas { get; set; } = new();
    }

    public class NegocioResumenDto
    {
        public int IdNegocio { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string Estado { get; set; } = string.Empty;
        public int IdCategoria { get; set; }
        public int IdMunicipio { get; set; }
        public string? Telefono { get; set; }
        public string? Horario { get; set; }
        public string? Direccion { get; set; }
        public decimal? Latitud { get; set; }
        public decimal? Longitud { get; set; }
        public double? PromedioCalificacion { get; set; }
        public List<string> Galeria { get; set; } = new();

        // Propiedades de apoyo para UI y enlaces de contacto
        public string? WhatsApp => Telefono?.Replace(" ", "").Replace("+", "");
        public string? BookingUrl { get; set; }

        public string NombreCategoria { get; set; } = string.Empty;
    }
}
