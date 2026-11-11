using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.Constants;
using TurismoConecta.api.DTOs.SitiosTuristicos;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/sitios-turisticos")]
    public class SitiosTuristicosController : ControllerBase
    {
        private const string RolesGestion = Roles.AdminGeneral + "," + Roles.AdminMunicipio;
        private const long TamanoMaximoImagen = 5 * 1024 * 1024; // 5 MB
        private static readonly string[] ExtensionesPermitidas = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly ISitioTuristicoService _service;
        private readonly IWebHostEnvironment _env;

        public SitiosTuristicosController(ISitioTuristicoService service, IWebHostEnvironment env)
        {
            _service = service;
            _env = env;
        }

        // ═════════════ PÚBLICO ═════════════

        // GET api/sitios-turisticos?soloDestacados=true&idCategoria=5
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<SitioTuristicoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar(
            [FromQuery] bool soloDestacados = false,
            [FromQuery] int? idCategoria = null,
            CancellationToken ct = default)
            => Ok(await _service.ListarAsync(soloDestacados, idCategoria, ct));

        // ═════════════ GESTIÓN ═════════════

        // GET api/sitios-turisticos/gestion
        [HttpGet("gestion")]
        [Authorize(Roles = RolesGestion)]
        [ProducesResponseType(typeof(PanelSitiosDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Gestion(CancellationToken ct)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var panel = await _service.ObtenerPanelAsync(idUsuario.Value, ct);
            return panel is null ? Forbid() : Ok(panel);
        }

        [HttpPost]
        [Authorize(Roles = RolesGestion)]
        public async Task<IActionResult> Crear([FromBody] SitioTuristicoCrearDto dto, CancellationToken ct)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var (exito, id, error) = await _service.CrearAsync(dto, idUsuario.Value, ct);
            return exito ? Ok(new { idSitioTuristico = id }) : BadRequest(new { mensaje = error });
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = RolesGestion)]
        public async Task<IActionResult> Editar(int id, [FromBody] SitioTuristicoEditarDto dto, CancellationToken ct)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var (exito, error) = await _service.EditarAsync(id, dto, idUsuario.Value, ct);
            return exito ? Ok() : BadRequest(new { mensaje = error });
        }

        // PATCH api/sitios-turisticos/5/estado?activo=false
        [HttpPatch("{id:int}/estado")]
        [Authorize(Roles = RolesGestion)]
        public async Task<IActionResult> CambiarEstado(int id, [FromQuery] bool activo, CancellationToken ct)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var (exito, error) = await _service.CambiarEstadoAsync(id, activo, idUsuario.Value, ct);
            return exito ? Ok() : BadRequest(new { mensaje = error });
        }

        // DELETE = ocultar (se conserva por compatibilidad)
        [HttpDelete("{id:int}")]
        [Authorize(Roles = RolesGestion)]
        public async Task<IActionResult> Eliminar(int id, CancellationToken ct)
        {
            var idUsuario = ObtenerIdUsuario();
            if (idUsuario is null) return Unauthorized();

            var (exito, error) = await _service.CambiarEstadoAsync(id, false, idUsuario.Value, ct);
            return exito ? Ok() : BadRequest(new { mensaje = error });
        }

        // POST api/sitios-turisticos/subir-imagen   (multipart/form-data, campo "archivo")
        [HttpPost("subir-imagen")]
        [Authorize(Roles = RolesGestion)]
        [RequestSizeLimit(TamanoMaximoImagen + 1024 * 1024)]
        public async Task<IActionResult> SubirImagen(IFormFile archivo, CancellationToken ct)
        {
            if (archivo is null || archivo.Length == 0)
                return BadRequest(new { mensaje = "No se seleccionó ningún archivo." });

            if (archivo.Length > TamanoMaximoImagen)
                return BadRequest(new { mensaje = "La imagen no puede superar 5 MB." });

            var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
            if (!ExtensionesPermitidas.Contains(extension) || !archivo.ContentType.StartsWith("image/"))
                return BadRequest(new { mensaje = "Formato no permitido. Usa JPG, PNG o WebP." });

            var carpeta = Path.Combine(_env.WebRootPath, "images", "sitios");
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"sitio_{Guid.NewGuid():N}{extension}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

            await using (var stream = new FileStream(rutaCompleta, FileMode.Create))
            {
                await archivo.CopyToAsync(stream, ct);
            }

            return Ok(new { url = $"images/sitios/{nombreArchivo}" });
        }

        private int? ObtenerIdUsuario()
            => int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;
    }
}
