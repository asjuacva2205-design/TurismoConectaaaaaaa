    using Microsoft.AspNetCore.Mvc;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Controllers;

[ApiController]
[Route("api/viaje")]
public class ViajeController : ControllerBase
{
    private readonly IViajeService _viajeService;
    public ViajeController(IViajeService viajeService) => _viajeService = viajeService;

    /// <summary>Todos los pueblos con coordenadas y un resumen de lo que ofrecen (para narrar el viaje).</summary>
    [HttpGet("pueblos")]
    public async Task<IActionResult> Pueblos(CancellationToken ct)
        => Ok(await _viajeService.ObtenerPueblosAsync(ct));
}
