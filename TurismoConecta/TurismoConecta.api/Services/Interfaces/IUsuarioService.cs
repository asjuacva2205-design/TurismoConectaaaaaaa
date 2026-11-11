using TurismoConecta.api.DTOs.Usuarios;

namespace TurismoConecta.api.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<bool> AsignarRolAsync(AssignRoleRequestDto dto);
        Task<PerfilResponseDto?> ObtenerPerfilAsync(int idUsuario);
        Task<bool> ActualizarPerfilAsync(int idUsuario, ActualizarPerfilRequestDto dto);
        Task<List<UsuarioAdminDto>> ListarUsuariosAsync();
        Task<List<RolDto>> ListarRolesAsync();
        Task<bool> ActualizarUsuarioAdminAsync(int idUsuario, UsuarioAdminEdicionDto dto);
        Task<bool> EliminarUsuarioAsync(int idUsuario);
        Task<bool> CambiarEstadoAsync(int idUsuario, bool activo);
        Task<(bool exito, string? error)> CrearUsuarioAdminAsync(UsuarioAdminCrearDto dto);
    }
}
