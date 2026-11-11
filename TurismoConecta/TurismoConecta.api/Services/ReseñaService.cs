using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Constants;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Reseñas;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;

namespace TurismoConecta.api.Services
{
    public class ReseñaService : IReseñaService
    {
        private const int MaxModeracion = 50;
        private static readonly string[] PalabrasProhibidas = { "estafa", "viagra", "compra-ya", "http://", "https://" };

        // Una sola proyección para todos los listados (EF la traduce a SQL)
        private static readonly Expression<Func<Reseña, ReseñaDto>> ADto = r => new ReseñaDto
        {
            IdReseña = r.IdReseña,
            IdUsuario = r.IdUsuario,
            NombreUsuario = r.IdUsuarioNavigation.Nombre,
            IdMunicipio = r.IdMunicipio,
            IdNegocio = r.IdNegocio,
            NombreLugar = r.IdNegocioNavigation != null
                ? r.IdNegocioNavigation.Nombre
                : r.IdMunicipioNavigation != null ? r.IdMunicipioNavigation.Nombre : "",
            Calificacion = r.Calificacion,
            Comentario = r.Comentario,
            Respuesta = r.Respuesta,
            FechaRespuesta = r.FechaRespuesta,
            Moderada = r.Moderada,
            FechaCreacion = r.FechaCreacion
        };

        private readonly AppDbContext _context;
        private readonly INotificacionService _notificaciones;

        public ReseñaService(AppDbContext context, INotificacionService notificaciones)
        {
            _context = context;
            _notificaciones = notificaciones;
        }

        public async Task<(ResultadoReseña resultado, string? error, ReseñaDto? creada)> CrearAsync(int idUsuario, CrearReseñaDto dto)
        {
            // Exactamente un lugar: o un municipio o un negocio
            if ((dto.IdMunicipio is null) == (dto.IdNegocio is null))
                return (ResultadoReseña.Invalida, "Indica un municipio o un negocio (solo uno).", null);

            Negocio? negocio = null;
            if (dto.IdNegocio is int idNegocio)
            {
                negocio = await _context.Negocios.AsNoTracking().FirstOrDefaultAsync(n => n.IdNegocio == idNegocio);
                if (negocio is null || negocio.Estado != EstadosNegocio.Aprobado)
                    return (ResultadoReseña.NoEncontrada, "El establecimiento no existe o aún no está publicado.", null);

                if (negocio.IdUsuario == idUsuario)
                    return (ResultadoReseña.Invalida, "No puedes reseñar tu propio establecimiento.", null);
            }
            else if (!await _context.Municipios.AnyAsync(m => m.IdMunicipio == dto.IdMunicipio && m.Activo))
            {
                return (ResultadoReseña.NoEncontrada, "El municipio no existe.", null);
            }

            bool yaExiste = await _context.Reseñas.AnyAsync(r =>
                r.IdUsuario == idUsuario && r.IdMunicipio == dto.IdMunicipio && r.IdNegocio == dto.IdNegocio);
            if (yaExiste) return (ResultadoReseña.Invalida, "Ya publicaste una reseña para este lugar.", null); // HU-28: una activa por lugar

            var comentario = string.IsNullOrWhiteSpace(dto.Comentario) ? null : dto.Comentario.Trim();
            bool esSpam = PalabrasProhibidas.Any(p =>
                (comentario ?? "").Contains(p, StringComparison.OrdinalIgnoreCase));

            var reseña = new Reseña
            {
                IdUsuario = idUsuario,
                IdMunicipio = dto.IdMunicipio,
                IdNegocio = dto.IdNegocio,
                Calificacion = (byte)dto.Calificacion,
                Comentario = comentario,
                FechaCreacion = DateTime.Now,
                Moderada = !esSpam // HU-30: si detecta palabra prohibida, queda oculta hasta revisión
            };

            _context.Reseñas.Add(reseña);
            await _context.SaveChangesAsync();

            // Avisar al dueño solo si la reseña quedó visible
            if (negocio?.IdUsuario is int idDuenio && reseña.Moderada)
            {
                await _notificaciones.EnviarAsync(
                    idDuenio,
                    CodigosNotificacion.ReseñaNueva,
                    $"Tu establecimiento «{negocio.Nombre}» recibió una reseña de {dto.Calificacion} estrella(s).");
            }

            var creada = await _context.Reseñas.AsNoTracking()
                .Where(r => r.IdReseña == reseña.IdReseña)
                .Select(ADto)
                .FirstAsync();

            return (ResultadoReseña.Ok, null, creada);
        }

        public async Task<(ResultadoReseña resultado, string? error)> ResponderAsync(int idReseña, int idDuenioNegocio, string respuesta)
        {
            respuesta = respuesta.Trim();
            if (respuesta.Length == 0) return (ResultadoReseña.Invalida, "Escribe una respuesta.");

            var reseña = await _context.Reseñas
                .Include(r => r.IdNegocioNavigation)
                .FirstOrDefaultAsync(r => r.IdReseña == idReseña);

            if (reseña is null || reseña.IdNegocioNavigation is null)
                return (ResultadoReseña.NoEncontrada, "Reseña no encontrada o no corresponde a un negocio.");

            if (reseña.IdNegocioNavigation.IdUsuario != idDuenioNegocio)
                return (ResultadoReseña.SinPermiso, "No tienes permiso para responder esta reseña.");

            // HU-29: "una única respuesta pública por reseña" — si ya respondió, se bloquea
            if (!string.IsNullOrEmpty(reseña.Respuesta))
                return (ResultadoReseña.Invalida, "Esta reseña ya tiene una respuesta.");

            reseña.Respuesta = respuesta;
            reseña.FechaRespuesta = DateTime.Now;
            await _context.SaveChangesAsync();

            await _notificaciones.EnviarAsync(
                reseña.IdUsuario,
                CodigosNotificacion.ReseñaRespondida,
                $"«{reseña.IdNegocioNavigation.Nombre}» respondió a tu reseña.");

            return (ResultadoReseña.Ok, null);
        }

        public async Task<List<ReseñaDto>> ListarAsync(int? idMunicipio, int? idNegocio)
        {
            var query = _context.Reseñas.AsNoTracking().Where(r => r.Moderada);
            if (idMunicipio.HasValue) query = query.Where(r => r.IdMunicipio == idMunicipio);
            if (idNegocio.HasValue) query = query.Where(r => r.IdNegocio == idNegocio);

            return await query
                .OrderByDescending(r => r.FechaCreacion)
                .Select(ADto)
                .ToListAsync();
        }

        public async Task<ReseñaDto?> ObtenerMiaAsync(int idUsuario, int? idMunicipio, int? idNegocio)
        {
            return await _context.Reseñas.AsNoTracking()
                .Where(r => r.IdUsuario == idUsuario && r.IdMunicipio == idMunicipio && r.IdNegocio == idNegocio)
                .Select(ADto)
                .FirstOrDefaultAsync();
        }

        public async Task<List<ReseñaDto>> ListarDeMisNegociosAsync(int idDuenio)
        {
            return await _context.Reseñas.AsNoTracking()
                .Where(r => r.Moderada
                         && r.IdNegocioNavigation != null
                         && r.IdNegocioNavigation.IdUsuario == idDuenio)
                .OrderBy(r => r.Respuesta != null)          // sin responder primero
                .ThenByDescending(r => r.FechaCreacion)
                .Select(ADto)
                .ToListAsync();
        }

        public async Task<List<ReseñaDto>> ListarParaModeracionAsync(int idAdmin, bool esAdminGeneral)
        {
            var query = _context.Reseñas.AsNoTracking();

            if (!esAdminGeneral)
            {
                var idMunicipio = await _context.Usuarios
                    .Where(u => u.IdUsuario == idAdmin)
                    .Select(u => u.MunicipioAsignadoId)
                    .FirstOrDefaultAsync();
                if (idMunicipio is null) return new();

                // Del municipio directamente, o de un negocio que está en ese municipio
                query = query.Where(r => r.IdMunicipio == idMunicipio
                    || (r.IdNegocioNavigation != null && r.IdNegocioNavigation.IdMunicipio == idMunicipio));
            }

            return await query
                .OrderBy(r => r.Moderada)                   // retenidas (false) primero
                .ThenByDescending(r => r.FechaCreacion)
                .Take(MaxModeracion)
                .Select(ADto)
                .ToListAsync();
        }

        public async Task<(ResultadoReseña resultado, string? error)> AprobarAsync(int idReseña, int idAdmin, bool esAdminGeneral)
        {
            var (reseña, resultado, error) = await BuscarConPermisoAsync(idReseña, idAdmin, esAdminGeneral);
            if (reseña is null) return (resultado, error);

            reseña.Moderada = true;
            await _context.SaveChangesAsync();
            return (ResultadoReseña.Ok, null);
        }

        public async Task<(ResultadoReseña resultado, string? error)> EliminarAsync(int idReseña, int idAdmin, bool esAdminGeneral)
        {
            var (reseña, resultado, error) = await BuscarConPermisoAsync(idReseña, idAdmin, esAdminGeneral);
            if (reseña is null) return (resultado, error);

            _context.Reseñas.Remove(reseña);
            await _context.SaveChangesAsync();
            return (ResultadoReseña.Ok, null);
        }

        // El AdminGeneral modera todo; el AdminMunicipio solo lo de su municipio
        private async Task<(Reseña? reseña, ResultadoReseña resultado, string? error)> BuscarConPermisoAsync(
            int idReseña, int idAdmin, bool esAdminGeneral)
        {
            var reseña = await _context.Reseñas
                .Include(r => r.IdNegocioNavigation)
                .FirstOrDefaultAsync(r => r.IdReseña == idReseña);
            if (reseña is null) return (null, ResultadoReseña.NoEncontrada, "Reseña no encontrada.");

            if (esAdminGeneral) return (reseña, ResultadoReseña.Ok, null);

            // La reseña pertenece a un municipio directo, o a un municipio a través de su negocio
            var idMunicipioDeLaResena = reseña.IdMunicipio ?? reseña.IdNegocioNavigation?.IdMunicipio;

            var admin = await _context.Usuarios.FindAsync(idAdmin);
            if (admin is null || admin.MunicipioAsignadoId != idMunicipioDeLaResena)
                return (null, ResultadoReseña.SinPermiso, "No tienes permiso sobre esta reseña.");

            return (reseña, ResultadoReseña.Ok, null);
        }
    }
}
