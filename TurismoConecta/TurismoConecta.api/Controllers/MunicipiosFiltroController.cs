using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/municipios")]
public class MunicipiosFiltroController : ControllerBase
{
    private readonly IFiltroEtiquetasService _service;
    public MunicipiosFiltroController(IFiltroEtiquetasService service) => _service = service;

    
    [HttpGet("filtrar")]
    public async Task<IActionResult> Filtrar([FromQuery] string? etiquetas)
    {
        List<int>? ids = string.IsNullOrWhiteSpace(etiquetas)
            ? null
            : etiquetas.Split(',').Select(int.Parse).ToList();

        var resultado = await _service.FiltrarMunicipiosAsync(ids);
        return Ok(resultado);
    }
}
