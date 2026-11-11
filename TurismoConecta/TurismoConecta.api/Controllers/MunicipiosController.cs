using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.Constants;
using TurismoConecta.api.DTOs.Common;
using TurismoConecta.api.DTOs.Municipios;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MunicipiosController : ControllerBase
    {
        private readonly IMunicipioService _municipioService;
        public MunicipiosController(IMunicipioService municipioService) => _municipioService = municipioService;

        /// <summary>Lista municipios activos, paginados.</summary>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ResultadoPaginado<MunicipioListadoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamano = 10, CancellationToken ct = default) =>
            Ok(await _municipioService.ListarAsync(pagina, tamano, ct));

        /// <summary>Busca municipios por texto y/o etiqueta.</summary>
        [HttpGet("buscar")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<MunicipioListadoDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Buscar([FromQuery] string? texto, [FromQuery] int? idEtiqueta, CancellationToken ct = default) =>
            Ok(await _municipioService.BuscarAsync(texto, idEtiqueta, ct));

        /// <summary>Todos los municipios activos con coordenadas (mapa y buscador de inicio).</summary>
        [HttpGet("mapa")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(List<MunicipioMapaDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> Mapa(CancellationToken ct = default) =>
            Ok(await _municipioService.ListarParaMapaAsync(ct));

        /// <summary>Ficha detallada de un municipio.</summary>
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(MunicipioFichaDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Ficha(int id, CancellationToken ct = default)
        {
            var ficha = await _municipioService.ObtenerFichaAsync(id, ct);
            return ficha is null ? NotFound(new { mensaje = "Municipio no encontrado." }) : Ok(ficha);
        }

        /// <summary>Edita la ficha de un municipio. Solo el AdminMunicipal, AdminGeneral asignado a ese municipio.</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.AdminMunicipio + "," + Roles.AdminGeneral)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Editar(int id, [FromBody] MunicipioEditarDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var (exito, error) = await _municipioService.EditarAsync(id, idUsuario, dto, ct);

            if (exito) return NoContent();
            return error!.Contains("permiso") ? Forbid() : BadRequest(new { mensaje = error });
        }

        /// <summary>Crea un nuevo municipio en el catálogo. Solo AdminGeneral.</summary>
        [HttpPost]
        [Authorize(Roles = Roles.AdminGeneral)]
        [ProducesResponseType(typeof(int), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Crear([FromBody] MunicipioCrearDto dto, CancellationToken ct = default)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var id = await _municipioService.CrearAsync(dto, ct);
                return CreatedAtAction(nameof(Ficha), new { id }, new { idMunicipio = id });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>Sube una imagen de portada para un municipio o establecimiento comercial.</summary>
        [HttpPost("subir-imagen")]
        [Authorize(Roles = Roles.AdminGeneral + "," + Roles.AdminMunicipio + "," + Roles.AdminEstablecimiento)]
        public async Task<IActionResult> SubirImagen(IFormFile archivo, [FromServices] IWebHostEnvironment env, [FromServices] IConfiguration config)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest(new { mensaje = "No se seleccionó ningún archivo." });

            if (!archivo.ContentType.StartsWith("image/"))
                return BadRequest(new { mensaje = "El archivo debe ser una imagen válida (JPG, PNG, WebP)." });

            // Carpeta destino dentro de la API
            var folderPath = Path.Combine(env.WebRootPath, "images", "municipios");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generamos un nombre único para evitar sobrescribir otras fotos
            var extension = Path.GetExtension(archivo.FileName).ToLower();
            var fileName = $"mun_{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Devolvemos la URL pública accesible
            var apiUrl = config["App:ApiUrl"]?.TrimEnd('/')
            ?? throw new InvalidOperationException("Falta 'App:ApiUrl' en appsettings.json");
            var urlPublica = $"{apiUrl}/images/municipios/{fileName}";
            return Ok(new { url = urlPublica });
        }
    }
}
