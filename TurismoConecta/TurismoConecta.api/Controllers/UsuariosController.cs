using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.DTOs.Usuarios;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Constants;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly IUsuarioService _usuarioService;

        public UsuariosController(IUsuarioService usuarioService)
        {
            _usuarioService = usuarioService;
        }

        [HttpGet]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _usuarioService.ListarUsuariosAsync();
            return Ok(usuarios);
        }

        [HttpGet("roles")]
        [Authorize(Roles = Roles.AdminGeneral )]
        public async Task<IActionResult> ListarRoles()
        {
            var roles = await _usuarioService.ListarRolesAsync();
            return Ok(roles);
        }

        [HttpPost]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> CrearUsuario([FromBody] UsuarioAdminCrearDto dto)
        {
            var (exito, error) = await _usuarioService.CrearUsuarioAdminAsync(dto);

            return exito
                ? StatusCode(StatusCodes.Status201Created)
                : BadRequest(new { mensaje = error });
        }


        [HttpPut("asignar-rol")]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> AsignarRol(AssignRoleRequestDto dto)
        {
            var exito = await _usuarioService.AsignarRolAsync(dto);

            if (!exito)
                return BadRequest("No se pudo asignar el rol. Verifica el idUsuario y el nombre del rol.");

            return Ok("Rol asignado correctamente.");
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> ActualizarUsuario(int id, UsuarioAdminEdicionDto dto)
        {
            var exito = await _usuarioService.ActualizarUsuarioAdminAsync(id, dto);

            if (!exito)
                return NotFound("No se encontró el usuario a actualizar.");

            return Ok("Usuario actualizado correctamente.");
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            if (id == ObtenerIdUsuarioActual())
                return BadRequest("No puedes eliminar tu propia cuenta de administrador mientras estés en sesión.");

            var exito = await _usuarioService.EliminarUsuarioAsync(id);

            if (!exito)
                return NotFound("No se encontró el usuario a eliminar.");

            return Ok("Usuario procesado/eliminado correctamente.");
        }

        [HttpPut("{id}/estado")]
        [Authorize(Roles = Roles.AdminGeneral)]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] bool activo)
        {
            if (id == ObtenerIdUsuarioActual() && !activo)
                return BadRequest("No puedes desactivar tu propia cuenta mientras estés en sesión.");

            var exito = await _usuarioService.CambiarEstadoAsync(id, activo);

            if (!exito)
                return NotFound("No se encontró el usuario.");

            return Ok("Estado del usuario actualizado.");
        }

        [HttpGet("perfil")]
        [Authorize]
        public async Task<IActionResult> ObtenerPerfil()
        {
            var idUsuario = ObtenerIdUsuarioActual();
            var perfil = await _usuarioService.ObtenerPerfilAsync(idUsuario);

            if (perfil == null)
                return NotFound("No se encontró el usuario.");

            return Ok(perfil);
        }

        [HttpPut("perfil")]
        [Authorize]
        public async Task<IActionResult> ActualizarPerfil(ActualizarPerfilRequestDto dto)
        {
            var idUsuario = ObtenerIdUsuarioActual();
            var exito = await _usuarioService.ActualizarPerfilAsync(idUsuario, dto);

            if (!exito)
                return NotFound("No se encontró el usuario.");

            return Ok("Perfil actualizado correctamente.");
        }

        private int ObtenerIdUsuarioActual()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(idClaim!);
        }
    }
}
