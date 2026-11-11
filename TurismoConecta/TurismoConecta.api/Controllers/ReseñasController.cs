using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.DTOs.Reseñas;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Constants;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReseñasController : ControllerBase
    {
        private readonly IReseñaService _reseñaService;
        public ReseñasController(IReseñaService reseñaService) => _reseñaService = reseñaService;

        private int UsuarioActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private bool EsAdminGeneral => User.IsInRole(Roles.AdminGeneral);

        // ═════════════ PÚBLICO ═════════════

        // GET api/reseñas?idMunicipio=1   |   GET api/reseñas?idNegocio=7
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Listar([FromQuery] int? idMunicipio, [FromQuery] int? idNegocio) =>
            Ok(await _reseñaService.ListarAsync(idMunicipio, idNegocio));

        // ═════════════ TURISTA ═════════════

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Crear([FromBody] CrearReseñaDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (resultado, error, creada) = await _reseñaService.CrearAsync(UsuarioActual, dto);
            return resultado == ResultadoReseña.Ok ? Ok(creada) : ARespuesta(resultado, error);
        }

        // GET api/reseñas/mia?idNegocio=7  → 200 con la reseña, o 204 si aún no ha opinado
        [HttpGet("mia")]
        [Authorize]
        public async Task<IActionResult> Mia([FromQuery] int? idMunicipio, [FromQuery] int? idNegocio)
        {
            var mia = await _reseñaService.ObtenerMiaAsync(UsuarioActual, idMunicipio, idNegocio);
            return mia is null ? NoContent() : Ok(mia);
        }

        // ═════════════ COMERCIANTE ═════════════

        [HttpGet("mis-negocios")]
        [Authorize(Roles = Roles.GeneralOEstablecimiento)]
        public async Task<IActionResult> DeMisNegocios() =>
            Ok(await _reseñaService.ListarDeMisNegociosAsync(UsuarioActual));

        [HttpPatch("{id:int}/responder")]
        [Authorize(Roles = Roles.GeneralOEstablecimiento)]
        public async Task<IActionResult> Responder(int id, [FromBody] ResponderReseñaDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (resultado, error) = await _reseñaService.ResponderAsync(id, UsuarioActual, dto.Respuesta);
            return ARespuesta(resultado, error);
        }

        // ═════════════ MODERACIÓN (HU-30) ═════════════

        [HttpGet("moderacion")]
        [Authorize(Roles = Roles.GeneralOMunicipio)]
        public async Task<IActionResult> Moderacion() =>
            Ok(await _reseñaService.ListarParaModeracionAsync(UsuarioActual, EsAdminGeneral));

        [HttpPatch("{id:int}/aprobar")]
        [Authorize(Roles = Roles.GeneralOMunicipio)]
        public async Task<IActionResult> Aprobar(int id)
        {
            var (resultado, error) = await _reseñaService.AprobarAsync(id, UsuarioActual, EsAdminGeneral);
            return ARespuesta(resultado, error);
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.GeneralOMunicipio)]
        public async Task<IActionResult> Eliminar(int id)
        {
            var (resultado, error) = await _reseñaService.EliminarAsync(id, UsuarioActual, EsAdminGeneral);
            return ARespuesta(resultado, error);
        }

        // Traduce el resultado del servicio al código HTTP correspondiente
        private IActionResult ARespuesta(ResultadoReseña resultado, string? error) => resultado switch
        {
            ResultadoReseña.Ok => NoContent(),
            ResultadoReseña.NoEncontrada => NotFound(new { mensaje = error }),
            ResultadoReseña.SinPermiso => StatusCode(StatusCodes.Status403Forbidden, new { mensaje = error }),
            _ => BadRequest(new { mensaje = error })
        };
    }
}
