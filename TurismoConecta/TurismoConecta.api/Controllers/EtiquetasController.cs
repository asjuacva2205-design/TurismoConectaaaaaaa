namespace TurismoConecta.api.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TurismoConecta.api.DTOs.Etiquetas;
using TurismoConecta.api.Services.Etiquetas;
using TurismoConecta.api.Constants;

[ApiController]
[Route("api/[controller]")]
public class EtiquetasController : ControllerBase
{
    private readonly IEtiquetaService _service;

    public EtiquetasController(IEtiquetaService service) => _service = service;

    // GET queda abierto a cualquier usuario autenticado (no solo AdminPrincipal),
    // porque HU-13 y HU-14 necesitan LEER las etiquetas (para asignarlas o filtrar).
    // Solo escribir (crear/editar/borrar) es exclusivo del AdminPrincipal.
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.ObtenerTodasAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var etiqueta = await _service.ObtenerPorIdAsync(id);
        return etiqueta is null ? NotFound() : Ok(etiqueta);
    }

    // [Authorize(Roles = "AdminPrincipal")] revisa el token del usuario que hace la petición
    // y verifica que tenga el claim de rol "AdminPrincipal". Si no lo tiene, ASP.NET Core
    // responde automáticamente 403 Forbidden ANTES de que tu código del método se ejecute.
    [HttpPost]
    [Authorize(Roles = Roles.AdminGeneral)]
    public async Task<IActionResult> Create([FromBody] EtiquetaCrearDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState); // valida los [Required]/[MaxLength] del DTO

        var creada = await _service.CrearAsync(dto);
        // CreatedAtAction devuelve 201 (Created) + una cabecera "Location" apuntando a GetById.
        // Es la forma "correcta" REST de responder a un POST exitoso, no solo un 200.
        return CreatedAtAction(nameof(GetById), new { id = creada.IdEtiqueta }, creada);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = Roles.AdminGeneral)]
    public async Task<IActionResult> Update(int id, [FromBody] EtiquetaActualizarDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var actualizado = await _service.ActualizarAsync(id, dto);
        return actualizado ? NoContent() : NotFound(); // 204 = "hecho, no tengo nada que devolverte"
    }

    [HttpPatch("{id}/desactivar")]
    [Authorize(Roles = Roles.AdminGeneral)]
    public async Task<IActionResult> Desactivar(int id)
    {
        var (exito, error) = await _service.DesactivarAsync(id);
        return exito ? NoContent() : NotFound(error);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.AdminGeneral)]
    public async Task<IActionResult> Delete(int id)
    {
        var (exito, error) = await _service.EliminarAsync(id);
        // Si falló por la regla de negocio (etiqueta en uso), devolvemos 409 Conflict,
        // que es el código HTTP correcto para "tu petición es válida, pero choca con el estado actual".
        return exito ? NoContent() : Conflict(error);
    }
}
