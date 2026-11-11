using TurismoConecta.api.DTOs.SitiosTuristicos;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface ISitioTuristicoService
    {
        // Público
        Task<List<SitioTuristicoDto>> ListarAsync(bool soloDestacados, int? idCategoria, CancellationToken ct = default);

        // Gestión (AdminGeneral / AdminMunicipio)
        Task<PanelSitiosDto?> ObtenerPanelAsync(int idUsuario, CancellationToken ct = default);
        Task<(bool exito, int id, string? error)> CrearAsync(SitioTuristicoCrearDto dto, int idUsuarioSolicitante, CancellationToken ct = default);
        Task<(bool exito, string? error)> EditarAsync(int id, SitioTuristicoEditarDto dto, int idUsuarioSolicitante, CancellationToken ct = default);
        Task<(bool exito, string? error)> CambiarEstadoAsync(int id, bool activo, int idUsuarioSolicitante, CancellationToken ct = default);
    }
}
