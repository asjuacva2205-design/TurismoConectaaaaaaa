using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TurismoConecta.api.DTOs.Asistente;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers;

[ApiController]
[Route("api/asistente")]
public class AsistenteController : ControllerBase
{
    private readonly IAsistenteService _asistenteService;
    public AsistenteController(IAsistenteService asistenteService) => _asistenteService = asistenteService;

    /// <summary>Chat del Cóndor Guía. Público, pero con límite por IP porque cada llamada cuesta dinero.</summary>
    [HttpPost("mensaje")]
    [EnableRateLimiting("asistente")]
    public async Task<IActionResult> Mensaje([FromBody] AsistenteMensajeDto dto, CancellationToken ct)
    {
        var respuesta = await _asistenteService.ResponderAsync(dto.Mensaje, dto.Historial, dto.Contexto, ct);
        return Ok(new AsistenteRespuestaDto { Respuesta = respuesta });
    }
}
