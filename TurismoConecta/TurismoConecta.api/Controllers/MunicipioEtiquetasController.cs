using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TurismoConecta.api.Constants;
using TurismoConecta.api.DTOs.Etiquetas;
using TurismoConecta.api.Services.Etiquetas;


namespace TurismoConecta.api.Controllers;
[ApiController]

[Route("api/municipios/{idMunicipio}/etiquetas")]
public class MunicipioEtiquetasController : ControllerBase
{
    private readonly IMunicipioEtiquetaService _service;
    public MunicipioEtiquetasController(IMunicipioEtiquetaService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> Get(int idMunicipio)
    {
        var resultado = await _service.ObtenerPorMunicipioAsync(idMunicipio);
        return resultado is null ? NotFound() : Ok(resultado);
    }

    [HttpPut]
    [Authorize(Roles = Roles.AdminMunicipio)]
    public async Task<IActionResult> Asignar(int idMunicipio, [FromBody] AsignarEtiquetasDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // User.FindFirst obtiene el claim "sub" (el Id de usuario) que quedó guardado
        // en el token cuando el admin municipal inició sesión (lo vimos en Identity, HU-02).
        var idUsuario = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var (exito, error) = await _service.AsignarAsync(idMunicipio, idUsuario, dto);
        return exito ? NoContent() : (error!.Contains("permiso") ? Forbid() : BadRequest(error));
    }
}
