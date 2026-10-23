using System.Net.Http.Json;
using System.Text.Json;
using TurismoConecta.web.Client.Models;

namespace TurismoConecta.web.Client.Services
{
    public class NegocioApiService
    {
        private readonly HttpClient _http;

        public NegocioApiService(HttpClient http)
        {
            _http = http;
        }

        // 1. Listar los negocios del comerciante en sesión
        public async Task<List<NegocioItemDto>> ListarMisNegociosAsync(CancellationToken ct = default)
        {
            try
            {
                var resultado = await _http.GetFromJsonAsync<List<NegocioItemDto>>("api/negocios/mis-negocios", ct);
                return resultado ?? new List<NegocioItemDto>();
            }
            catch
            {
                return new List<NegocioItemDto>();
            }
        }

        // 2. Obtener la ficha / detalle de un negocio específico por Id
        public async Task<NegocioItemDto?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<NegocioItemDto>($"api/negocios/{id}", ct);
            }
            catch
            {
                return null;
            }
        }

        // 3. Crear nuevo negocio (AdminComercio)
        public async Task<(bool Exito, NegocioItemDto? NegocioCreado, string? Error)> CrearAsync(FormNegocioDto dto, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PostAsJsonAsync("api/negocios", dto, ct);
                if (response.IsSuccessStatusCode)
                {
                    var creado = await response.Content.ReadFromJsonAsync<NegocioItemDto>(cancellationToken: ct);
                    return (true, creado, null);
                }

                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, null, err);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        // 4. Editar negocio existente (AdminComercio)
        public async Task<(bool Exito, string? Error)> EditarAsync(int id, FormNegocioDto dto, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PutAsJsonAsync($"api/negocios/{id}", dto, ct);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // 5. Bandeja de establecimientos pendientes (AdminMunicipio - HU-24)
        public async Task<List<NegocioPendienteAprobacionDto>> ListarPendientesAsync(CancellationToken ct = default)
        {
            try
            {
                var listado = await _http.GetFromJsonAsync<List<NegocioPendienteAprobacionDto>>("api/negocios/pendientes", ct);
                return listado ?? new List<NegocioPendienteAprobacionDto>();
            }
            catch
            {
                return new List<NegocioPendienteAprobacionDto>();
            }
        }

        // 5b. KPIs del dashboard municipal
        public async Task<NegocioConteoEstadosDto?> ObtenerResumenAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<NegocioConteoEstadosDto>("api/negocios/resumen", ct);
            }
            catch
            {
                return null;
            }
        }


        // 6. Cambiar estado: "Aprobado" o "Rechazado" (AdminMunicipio)
        public async Task<(bool Exito, string? Error)> CambiarEstadoAsync(int id, string nuevoEstado, CancellationToken ct = default)
        {
            try
            {
                var response = await _http.PatchAsJsonAsync($"api/negocios/{id}/estado", nuevoEstado, ct);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, err);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // 7. Subir foto a la galería (usa el endpoint multiparte de imágenes)
        public async Task<(bool Exito, string? Url, string? Error)> SubirFotoGaleriaAsync(
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
                }

                var err = await response.Content.ReadAsStringAsync(ct);
                return (false, null, err);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        public async Task<List<CategoriaDto>> ListarCategoriasAsync(CancellationToken ct = default)
        {
            try
            {
                return await _http.GetFromJsonAsync<List<CategoriaDto>>("api/categorias/negocios", ct) ?? new();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return new();
            }
        }
    }
}
