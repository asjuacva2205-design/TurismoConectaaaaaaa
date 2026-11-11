using TurismoConecta.api.DTOs.Reseñas;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface IReseñaService
    {
        // HU-28: un turista califica un municipio o un negocio (una reseña por lugar)
        Task<(ResultadoReseña resultado, string? error, ReseñaDto? creada)> CrearAsync(int idUsuario, CrearReseñaDto dto);

        // HU-29: el dueño del negocio responde una sola vez
        Task<(ResultadoReseña resultado, string? error)> ResponderAsync(int idReseña, int idDuenioNegocio, string respuesta);

        // Público: solo las reseñas visibles
        Task<List<ReseñaDto>> ListarAsync(int? idMunicipio, int? idNegocio);

        // La reseña que el usuario ya dejó en ese lugar (para no mostrarle el formulario otra vez)
        Task<ReseñaDto?> ObtenerMiaAsync(int idUsuario, int? idMunicipio, int? idNegocio);

        // Panel del comerciante: reseñas de sus establecimientos
        Task<List<ReseñaDto>> ListarDeMisNegociosAsync(int idDuenio);

        // HU-30: panel municipal (retenidas por el filtro primero)
        Task<List<ReseñaDto>> ListarParaModeracionAsync(int idAdmin, bool esAdminGeneral);
        Task<(ResultadoReseña resultado, string? error)> AprobarAsync(int idReseña, int idAdmin, bool esAdminGeneral);

        // HU-30: el admin municipal elimina una reseña reportada por spam/ofensiva
        Task<(ResultadoReseña resultado, string? error)> EliminarAsync(int idReseña, int idAdmin, bool esAdminGeneral);
    }
}
