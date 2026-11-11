using TurismoConecta.api.DTOs.Negocios;
using TurismoConecta.api.Models;

namespace TurismoConecta.api.Services.Mappers
{
    public static class NegocioMapper
    {
        public static NegocioDto ToDto(Negocio n, double? promedio = null) => new()
        {
            IdNegocio = n.IdNegocio,
            Nombre = n.Nombre,
            Descripcion = n.Descripcion,
            Estado = n.Estado,
            IdCategoria = n.IdCategoria,
            IdMunicipio = n.IdMunicipio,
            NombreCategoria = n.IdCategoriaNavigation?.Nombre ?? "Sin categoría",
            NombreMunicipio = n.IdMunicipioNavigation?.Nombre ?? "Sin municipio",
            Telefono = n.Telefono,
            Horario = n.Horario,
            Direccion = n.Direccion,
            Latitud = n.Latitud,
            Longitud = n.Longitud,
            PromedioCalificacion = promedio,
            Galeria = n.GaleriaNegocios.Select(g => g.ImagenUrl!).Where(u => u != null).ToList()
        };

        // ◆ CAMBIO: recibe cuántos negocios aprobados tiene el propietario
        //   (se calcula en el servicio con UNA sola consulta para toda la lista)
        public static NegocioPendienteDto ToPendienteDto(Negocio n, int negociosAprobadosPropietario = 0) => new()
        {
            IdNegocio = n.IdNegocio,
            Nombre = n.Nombre,
            IdCategoria = n.IdCategoria,
            FechaRegistro = n.FechaRegistro,
            NombrePropietario = n.IdUsuarioNavigation?.Nombre ?? "",
            IdMunicipio = n.IdMunicipio,
            NombreMunicipio = n.IdMunicipioNavigation?.Nombre ?? "Sin municipio",
            NombreCategoria = n.IdCategoriaNavigation?.Nombre ?? "Sin categoría",

            // ◆ NUEVO
            Descripcion = n.Descripcion,
            Direccion = n.Direccion,
            Telefono = n.Telefono,
            Email = n.Email,
            Horario = n.Horario,
            Latitud = n.Latitud,
            Longitud = n.Longitud,

            // Foto principal (si existe) + galería, sin vacías ni repetidas
            Fotos = new[] { n.ImagenPrincipalUrl }
                .Concat(n.GaleriaNegocios.Select(g => g.ImagenUrl))
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => url!)
                .Distinct()
                .ToList(),

            Etiquetas = n.NegocioEtiqueta
                .Select(ne => ne.IdEtiquetaNavigation?.Nombre)
                .Where(nombre => !string.IsNullOrWhiteSpace(nombre))
                .Select(nombre => nombre!)
                .ToList(),

            EmailPropietario = n.IdUsuarioNavigation?.Email ?? "",
            NegociosAprobadosPropietario = negociosAprobadosPropietario
        };
    }
}
