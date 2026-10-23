using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TurismoConecta.api.DTOs.Notificaciones;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificacionesController : ControllerBase
    {
        private readonly INotificacionService _service;
        public NotificacionesController(INotificacionService service) => _service = service;

        // GET api/notificaciones?soloNoLeidas=false
        [HttpGet]
        [ProducesResponseType(typeof(List<NotificacionDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] bool soloNoLeidas = false, CancellationToken ct = default)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            return Ok(await _service.ListarAsync(idUsuario.Value, soloNoLeidas, ct));
        }

        // PATCH api/notificaciones/15/leida
        [HttpPatch("{id:int}/leida")]
        public async Task<IActionResult> MarcarLeida(int id, CancellationToken ct = default)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var encontrada = await _service.MarcarLeidaAsync(id, idUsuario.Value, ct);
            return encontrada ? NoContent() : NotFound(new { mensaje = "Notificación no encontrada." });
        }

        // PATCH api/notificaciones/leer-todas
        [HttpPatch("leer-todas")]
        public async Task<IActionResult> MarcarTodas(CancellationToken ct = default)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var cantidad = await _service.MarcarTodasLeidasAsync(idUsuario.Value, ct);
            return Ok(new { marcadas = cantidad });
        }

        private int? ObtenerIdUsuario()
            => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    }
}
