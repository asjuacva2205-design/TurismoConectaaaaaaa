using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.DTOs.Negocios;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Constants;
namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NegociosController : ControllerBase
    {
        private readonly INegocioService _negocioService;
        public NegociosController(INegocioService negocioService) => _negocioService = negocioService;

        private int UsuarioActual => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpPost]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio + "," + Roles.AdminEstablecimiento)]
        public async Task<IActionResult> Crear([FromBody] NegocioCrearDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            return Ok(await _negocioService.CrearAsync(UsuarioActual, dto));
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio + "," + Roles.AdminEstablecimiento)]
        public async Task<IActionResult> Editar(int id, [FromBody] NegocioEditarDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var (exito, error) = await _negocioService.EditarAsync(id, UsuarioActual, dto);
            return exito ? NoContent() : BadRequest(new { mensaje = error });
        }

        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio )]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var (exito, error) = await _negocioService.CambiarEstadoAsync(id, UsuarioActual, nuevoEstado);
            if (exito) return NoContent();
            return error!.Contains("permiso") ? Forbid() : BadRequest(new { mensaje = error });
        }

        // HU-24: bandeja de pendientes del admin municipal y general
        [HttpGet("pendientes")]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio )]
        public async Task<IActionResult> Pendientes() =>
            Ok(await _negocioService.ListarPendientesAsync(UsuarioActual));

        // KPIs del dashboard municipal (aprobados / pendientes / rechazados)
        [HttpGet("resumen")]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio)]
        public async Task<IActionResult> Resumen() =>
            Ok(await _negocioService.ObtenerResumenAsync(UsuarioActual));


        [HttpGet("por-municipio/{idMunicipio:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> PorMunicipio(int idMunicipio, [FromQuery] int? idCategoria) =>
            Ok(await _negocioService.ListarPorMunicipioAsync(idMunicipio, idCategoria));

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> Ficha(int id)
        {
            var negocio = await _negocioService.ObtenerFichaAsync(id);
            return negocio is null ? NotFound() : Ok(negocio);
        }


        [HttpGet("mis-negocios")]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio + "," + Roles.AdminEstablecimiento)]
        public async Task<IActionResult> MisNegocios()
        {
            var listado = await _negocioService.ListarMisNegociosAsync(UsuarioActual);
            return Ok(listado);
        }


    }
}
