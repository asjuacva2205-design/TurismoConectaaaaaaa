using TurismoConecta.api.DTOs.Common;
using TurismoConecta.api.DTOs.Municipios;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface IMunicipioService
    {
        Task<ResultadoPaginado<MunicipioListadoDto>> ListarAsync(int pagina, int tamano, CancellationToken ct = default);
        Task<List<MunicipioListadoDto>> BuscarAsync(string? texto, int? idEtiqueta, CancellationToken ct = default);
        Task<List<MunicipioMapaDto>> ListarParaMapaAsync(CancellationToken ct = default);
        Task<MunicipioFichaDto?> ObtenerFichaAsync(int id, CancellationToken ct = default);
        Task<int> CrearAsync(MunicipioCrearDto dto, CancellationToken ct);
        Task<(bool exito, string? error)> EditarAsync(int id, int idAdminSolicitante, MunicipioEditarDto dto, CancellationToken ct = default);
    }
}
