using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Negocios;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Services.Mappers;
using TurismoConecta.api.Constants;

namespace TurismoConecta.api.Services
{
    public class NegocioService : INegocioService
    {
        private readonly AppDbContext _context;
        private readonly INotificacionService _notificaciones;

        public NegocioService(AppDbContext context, INotificacionService notificaciones)
        {
            _context = context;
            _notificaciones = notificaciones;
        }

        public async Task<NegocioDto> CrearAsync(int idUsuario, NegocioCrearDto dto)
        {
            var negocio = new Negocio
            {
                IdUsuario = idUsuario,
                Nombre = dto.Nombre,
                Descripcion = dto.Descripcion,
                IdCategoria = dto.IdCategoria,
                IdMunicipio = dto.IdMunicipio,
                Telefono = dto.Telefono,
                Horario = dto.Horario,
                Direccion = dto.Direccion,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                Estado = EstadosNegocio.Pendiente, // HU-22
                FechaRegistro = DateTime.Now
            };

            // HU-22: galería de fotos junto con el registro, en la misma transacción
            negocio.GaleriaNegocios = dto.ImagenesGaleria
                .Where(url => !string.IsNullOrWhiteSpace(url))
                .Select(url => new GaleriaNegocio { ImagenUrl = url })
                .ToList();

            _context.Negocios.Add(negocio);
            await _context.SaveChangesAsync();

            // Avisar a los administradores del municipio que hay algo por revisar
            await NotificarRevisoresAsync(
                negocio.IdMunicipio,
                $"Nuevo establecimiento por revisar: «{negocio.Nombre}».");

            return await ObtenerFichaAsync(negocio.IdNegocio) ?? throw new InvalidOperationException("Error al crear el negocio.");
        }

        public async Task<(bool exito, string? error)> EditarAsync(int idNegocio, int idUsuario, NegocioEditarDto dto)
        {
            var negocio = await _context.Negocios.FindAsync(idNegocio);
            if (negocio is null || negocio.IdUsuario != idUsuario)
                return (false, "Negocio no encontrado o no te pertenece.");

            // HU-23 exacto: "un cambio sustancial (nombre, categoría) requiere nueva verificación"
            // — solo esos 2 campos disparan la regla; horario/teléfono/dirección se editan libremente.
            bool cambioSustancial = negocio.Nombre != dto.Nombre || negocio.IdCategoria != dto.IdCategoria;

            negocio.Nombre = dto.Nombre;
            negocio.Descripcion = dto.Descripcion;
            negocio.IdCategoria = dto.IdCategoria;
            negocio.Telefono = dto.Telefono;
            negocio.Horario = dto.Horario;
            negocio.Direccion = dto.Direccion;
            negocio.Latitud = dto.Latitud;
            negocio.Longitud = dto.Longitud;

            if (cambioSustancial) negocio.Estado = EstadosNegocio.Pendiente;

            await _context.SaveChangesAsync();

            // HU-23: un cambio de nombre o categoría vuelve a revisión → avisar al municipio
            if (cambioSustancial)
            {
                await NotificarRevisoresAsync(
                    negocio.IdMunicipio,
                    $"«{negocio.Nombre}» cambió su nombre o categoría y necesita una nueva revisión.");
            }

            return (true, null);
        }

        public async Task<(bool exito, string? error)> CambiarEstadoAsync(int idNegocio, int idAdminMunicipio, string nuevoEstado)
        {
            if (nuevoEstado != EstadosNegocio.Aprobado && nuevoEstado != EstadosNegocio.Rechazado)
                return (false, "Estado inválido. Use 'Aprobado' o 'Rechazado'.");

            var negocio = await _context.Negocios.FindAsync(idNegocio);
            if (negocio is null) return (false, "Negocio no encontrado.");

            var admin = await _context.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefaultAsync(u => u.IdUsuario == idAdminMunicipio);
            if (admin is null)
                return (false, "Usuario administrador no encontrado.");

            bool esSuperAdmin = admin.IdRolNavigation?.Nombre == Roles.AdminGeneral;
            if (!esSuperAdmin && admin.MunicipioAsignadoId != negocio.IdMunicipio)
                return (false, "No tienes permiso sobre este municipio.");

            negocio.Estado = nuevoEstado;
            if (nuevoEstado == EstadosNegocio.Aprobado) negocio.FechaAprobacion = DateTime.Now;

            await _context.SaveChangesAsync();

            // HU-31: avisar al dueño del negocio (en vivo + historial)
            if (negocio.IdUsuario.HasValue)
            {
                var fueAprobado = nuevoEstado == EstadosNegocio.Aprobado;

                await _notificaciones.EnviarAsync(
                    negocio.IdUsuario.Value,
                    fueAprobado ? CodigosNotificacion.NegocioAprobado : CodigosNotificacion.NegocioRechazado,
                    fueAprobado
                        ? $"¡Tu establecimiento «{negocio.Nombre}» fue aprobado! Ya es visible para los turistas."
                        : $"Tu establecimiento «{negocio.Nombre}» no fue aprobado. Revisa sus datos y vuelve a enviarlo.");
            }

            return (true, null);
        }

        /// <summary>
        /// Envía la notificación NEGOCIO_PENDIENTE a todos los AdminMunicipio
        /// activos asignados a ese municipio.
        /// </summary>
        private async Task NotificarRevisoresAsync(int idMunicipio, string mensaje)
        {
            var idsRevisores = await _context.Usuarios
                .Where(u => u.Activo
                         && u.MunicipioAsignadoId == idMunicipio
                         && u.IdRolNavigation.Nombre == Roles.AdminMunicipio)
                .Select(u => u.IdUsuario)
                .ToListAsync();

            foreach (var idRevisor in idsRevisores)
            {
                await _notificaciones.EnviarAsync(idRevisor, CodigosNotificacion.NegocioPendiente, mensaje);
            }
        }

        public async Task<List<NegocioDto>> ListarPorMunicipioAsync(int idMunicipio, int? idCategoria)
        {
            var query = _context.Negocios
                .Include(n => n.GaleriaNegocios)
                .Include(n => n.IdCategoriaNavigation)
                .Include(n => n.IdMunicipioNavigation)
                .Where(n => n.IdMunicipio == idMunicipio && n.Estado == EstadosNegocio.Aprobado);

            if (idCategoria.HasValue)
                query = query.Where(n => n.IdCategoria == idCategoria); // HU-25: filtro por categoría

            var negocios = await query.ToListAsync();
            return negocios.Select(n => NegocioMapper.ToDto(n)).ToList();
        }

        // ◆ CAMBIO: trae el detalle completo y ordena del MÁS NUEVO al más antiguo
        public async Task<List<NegocioPendienteDto>> ListarPendientesAsync(int idAdminMunicipal)
        {
            var admin = await _context.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefaultAsync(u => u.IdUsuario == idAdminMunicipal);
            if (admin is null) return new List<NegocioPendienteDto>();

            IQueryable<Negocio> query = _context.Negocios
                .Include(n => n.IdUsuarioNavigation)
                .Include(n => n.IdMunicipioNavigation)
                .Include(n => n.IdCategoriaNavigation)
                .Include(n => n.GaleriaNegocios)                                    // ◆ fotos
                .Include(n => n.NegocioEtiqueta).ThenInclude(ne => ne.IdEtiquetaNavigation) // ◆ etiquetas
                .Where(n => n.Estado == EstadosNegocio.Pendiente);

            bool esSuperAdmin = admin.IdRolNavigation?.Nombre == Roles.AdminGeneral;
            if (!esSuperAdmin)
            {
                if (admin.MunicipioAsignadoId is null) return new List<NegocioPendienteDto>();
                query = query.Where(n => n.IdMunicipio == admin.MunicipioAsignadoId);
            }

            var pendientes = await query
                .AsSplitQuery()                              // ◆ evita una consulta gigante al juntar 2 colecciones
                .OrderByDescending(n => n.FechaRegistro)     // ◆ A1: más nuevo arriba
                .ThenByDescending(n => n.IdNegocio)
                .ToListAsync();

            // ◆ Cuántos negocios APROBADOS tiene cada propietario (una sola consulta para todos)
            var idsPropietarios = pendientes
                .Where(n => n.IdUsuario.HasValue)
                .Select(n => n.IdUsuario!.Value)
                .Distinct()
                .ToList();

            var aprobadosPorPropietario = await _context.Negocios
                .Where(n => n.IdUsuario.HasValue
                         && idsPropietarios.Contains(n.IdUsuario.Value)
                         && n.Estado == EstadosNegocio.Aprobado)
                .GroupBy(n => n.IdUsuario!.Value)
                .Select(g => new { IdUsuario = g.Key, Total = g.Count() })
                .ToDictionaryAsync(x => x.IdUsuario, x => x.Total);

            return pendientes
                .Select(n => NegocioMapper.ToPendienteDto(
                    n,
                    n.IdUsuario.HasValue && aprobadosPorPropietario.TryGetValue(n.IdUsuario.Value, out var total) ? total : 0))
                .ToList();
        }

        // KPIs del dashboard: conteo por estado, 
        // (AdminGeneral ve todo; AdminMunicipio solo su municipio asignado)
        public async Task<NegocioResumenDto> ObtenerResumenAsync(int idAdminMunicipal)
        {
            var admin = await _context.Usuarios.Include(u => u.IdRolNavigation).FirstOrDefaultAsync(u => u.IdUsuario == idAdminMunicipal);
            if (admin is null) return new NegocioResumenDto();

            IQueryable<Negocio> query = _context.Negocios;

            bool esSuperAdmin = admin.IdRolNavigation?.Nombre == Roles.AdminGeneral;
            if (!esSuperAdmin)
            {
                if (admin.MunicipioAsignadoId is null) return new NegocioResumenDto();
                query = query.Where(n => n.IdMunicipio == admin.MunicipioAsignadoId);
            }

            var conteos = await query
                .GroupBy(n => n.Estado)
                .Select(g => new { Estado = g.Key, Total = g.Count() })
                .ToListAsync();

            int Contar(string estado) => conteos.Where(c => c.Estado == estado).Sum(c => c.Total);

            return new NegocioResumenDto
            {
                Aprobados = Contar(EstadosNegocio.Aprobado),
                Pendientes = Contar(EstadosNegocio.Pendiente),
                Rechazados = Contar(EstadosNegocio.Rechazado)
            };
        }

        public async Task<NegocioDto?> ObtenerFichaAsync(int idNegocio)
        {
            var n = await _context.Negocios
                .Include(x => x.GaleriaNegocios)
                .Include(x => x.IdCategoriaNavigation)
                .Include(x => x.IdMunicipioNavigation)
                .FirstOrDefaultAsync(x => x.IdNegocio == idNegocio);
            if (n is null) return null;

            var promedio = await _context.Reseñas
                .Where(r => r.IdNegocio == idNegocio && r.Moderada)
                .AverageAsync(r => (double?)r.Calificacion);

            return NegocioMapper.ToDto(n, promedio);
        }

        public async Task<List<NegocioDto>> ListarMisNegociosAsync(int idUsuario)
        {
            var negocios = await _context.Negocios
                .Include(n => n.GaleriaNegocios)
                .Include(n => n.IdCategoriaNavigation)
                .Include(n => n.IdMunicipioNavigation)
                .Where(n => n.IdUsuario == idUsuario)
                .OrderByDescending(n => n.FechaRegistro)
                .ToListAsync();

            return negocios.Select(n => NegocioMapper.ToDto(n)).ToList();
        }
    }
}
