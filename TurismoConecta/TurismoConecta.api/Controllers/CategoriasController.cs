using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Negocios;
using TurismoConecta.api.Models;

namespace TurismoConecta.api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasController(AppDbContext context) => _context = context;

        // GET api/categorias/negocios → para Crear/Editar establecimiento
        [HttpGet("negocios")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarParaNegocios(CancellationToken ct) =>
            Ok(await Proyectar(_context.Categoria.Where(c => c.AplicaNegocio)).ToListAsync(ct));

        // GET api/categorias/sitios → para sitios turísticos y las cards de Inicio
        [HttpGet("sitios")]
        [AllowAnonymous]
        public async Task<IActionResult> ListarParaSitios(CancellationToken ct) =>
            Ok(await Proyectar(_context.Categoria.Where(c => c.AplicaSitio)).ToListAsync(ct));

        // Ordena por nombre y convierte cada Categoria en CategoriaDto
        private static IQueryable<CategoriaDto> Proyectar(IQueryable<Categoria> query) =>
            query.OrderBy(c => c.Nombre)
                 .Select(c => new CategoriaDto
                 {
                     IdCategoria = c.IdCategoria,
                     Nombre = c.Nombre,
                     Icono = c.Icono
                 });
    }
}
