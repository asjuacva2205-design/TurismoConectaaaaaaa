using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Viaje;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services;

public class ViajeService : IViajeService
{
    private readonly AppDbContext _context;
    public ViajeService(AppDbContext context) => _context = context;

    public async Task<List<PuebloViajeDto>> ObtenerPueblosAsync(CancellationToken ct = default)
    {
        var municipios = await _context.Municipios.AsNoTracking()
            .Where(m => m.Activo && m.Latitud != null && m.Longitud != null)
            .OrderBy(m => m.Nombre)
            .Select(m => new
            {
                m.IdMunicipio,
                m.Nombre,
                Lat = m.Latitud!.Value,
                Lng = m.Longitud!.Value,
                Etiquetas = m.MunicipioEtiqueta.Select(me => me.IdEtiquetaNavigation.Nombre).ToList()
            })
            .ToListAsync(ct);

        var sitios = await _context.SitiosTuristicos.AsNoTracking()
            .Where(s => s.Activo)
            .OrderByDescending(s => s.Destacado)
            .Select(s => new { s.IdMunicipio, s.Nombre })
            .ToListAsync(ct);

        var categoriasNegocio = await _context.Negocios.AsNoTracking()
            .Where(n => n.Estado == "Aprobado")
            .Select(n => new { n.IdMunicipio, Categoria = n.IdCategoriaNavigation.Nombre })
            .Distinct()
            .ToListAsync(ct);

        return municipios.Select(m =>
        {
            var partes = new List<string>();

            var etiquetas = m.Etiquetas.Take(3).Select(e => e.ToLowerInvariant()).ToList();
            if (etiquetas.Count > 0) partes.Add(UnirConY(etiquetas));

            var sitiosM = sitios.Where(s => s.IdMunicipio == m.IdMunicipio).Select(s => s.Nombre).Take(2).ToList();
            if (sitiosM.Count > 0) partes.Add("sitios para visitar como " + UnirConY(sitiosM));

            var cats = categoriasNegocio.Where(c => c.IdMunicipio == m.IdMunicipio)
                                        .Select(c => c.Categoria.ToLowerInvariant()).Take(3).ToList();
            if (cats.Count > 0) partes.Add("negocios locales de " + UnirConY(cats));

            return new PuebloViajeDto
            {
                IdMunicipio = m.IdMunicipio,
                Nombre = m.Nombre,
                Latitud = (double)m.Lat,
                Longitud = (double)m.Lng,
                Ofrece = string.Join("; ", partes)
            };
        }).ToList();
    }

    private static string UnirConY(IList<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        _ => string.Join(", ", items.Take(items.Count - 1)) + " y " + items[^1]
    };
}
