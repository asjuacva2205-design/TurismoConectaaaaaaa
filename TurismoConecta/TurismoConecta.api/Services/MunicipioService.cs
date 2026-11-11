using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Data;
using TurismoConecta.api.DTOs.Common;
using TurismoConecta.api.DTOs.Municipios;
using TurismoConecta.api.Models;
using TurismoConecta.api.Services.Interfaces;
using TurismoConecta.api.Services.Mappers;
using TurismoConecta.api.Constants;

namespace TurismoConecta.api.Services
{
    public class MunicipioService : IMunicipioService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MunicipioService> _logger;

        public MunicipioService(AppDbContext context, ILogger<MunicipioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<int> CrearAsync(MunicipioCrearDto dto, CancellationToken ct = default)
        {
            // Si no se especificó departamento o viene en 0, asignamos 1 (Boyacá por defecto)
            int idDepartamento = dto.IdDepartamento > 0 ? dto.IdDepartamento : 1;

            // Regla de negocio: no permitir dos municipios con el mismo nombre en el mismo departamento
            var existe = await _context.Municipios
                .AnyAsync(m => m.Nombre.ToLower() == dto.Nombre.Trim().ToLower()
                            && m.IdDepartamento == idDepartamento, ct);
            if (existe)
                throw new InvalidOperationException("Ya existe un municipio con ese nombre en el departamento.");

            var municipio = new Municipio
            {
                IdDepartamento = idDepartamento,
                Nombre = dto.Nombre.Trim(),
                Descripcion = dto.Descripcion,
                Historia = dto.Historia,
                Clima = dto.Clima,
                ImagenUrl = dto.ImagenUrl,
                Latitud = dto.Latitud,
                Longitud = dto.Longitud,
                FechaCreacion = DateTime.UtcNow,
                Activo = true
            };

            _context.Municipios.Add(municipio);
            await _context.SaveChangesAsync(ct);

            // Si se enviaron fechas relevantes al crear, las sincronizamos
            if (dto.FechasRelevantes != null && dto.FechasRelevantes.Any())
            {
                await SincronizarFechasRelevantesAsync(municipio.IdMunicipio, dto.FechasRelevantes, ct);
            }

            // Si se enviaron etiquetas al crear, las sincronizamos
            if (dto.Etiquetas != null && dto.Etiquetas.Any())
            {
                await SincronizarEtiquetasAsync(municipio.IdMunicipio, dto.Etiquetas, ct);
            }

            return municipio.IdMunicipio;
        }
        public async Task<ResultadoPaginado<MunicipioListadoDto>> ListarAsync(int pagina, int tamano, CancellationToken ct = default)
        {
            // Clamp: nunca dejamos que el cliente pida página 0/negativa o un tamaño absurdo (protección básica contra abuso del endpoint)
            pagina = Math.Max(1, pagina);
            tamano = Math.Clamp(tamano, 1, 50);

            var query = _context.Municipios.Where(m => m.Activo);
            var total = await query.CountAsync(ct);

            var items = await query
                .OrderBy(m => m.Nombre)
                .Skip((pagina - 1) * tamano)
                .Take(tamano)
                .Include(m => m.MunicipioEtiqueta).ThenInclude(me => me.IdEtiquetaNavigation)
                .ToListAsync(ct);

            return new ResultadoPaginado<MunicipioListadoDto>
            {
                Items = items.Select(MunicipioMapper.ToListadoDto).ToList(),
                Total = total,
                Pagina = pagina,
                Tamano = tamano
            };
        }

        public async Task<List<MunicipioMapaDto>> ListarParaMapaAsync(CancellationToken ct = default)
        {
            return await _context.Municipios
                .AsNoTracking()
                .Where(m => m.Activo)
                .OrderBy(m => m.Nombre)
                .Select(m => new MunicipioMapaDto
                {
                    IdMunicipio = m.IdMunicipio,
                    Nombre = m.Nombre,
                    Latitud = (double?)m.Latitud,
                    Longitud = (double?)m.Longitud
                })
                .ToListAsync(ct);
        }

        public async Task<List<MunicipioListadoDto>> BuscarAsync(string? texto, int? idEtiqueta, CancellationToken ct = default)
        {
            var query = _context.Municipios.Where(m => m.Activo).AsQueryable();

            // Filtramos por etiqueta en la BD (puede usar índices)
            if (idEtiqueta.HasValue)
                query = query.Where(m => m.MunicipioEtiqueta.Any(me => me.IdEtiqueta == idEtiqueta));

            var municipios = await query
                .Include(m => m.MunicipioEtiqueta).ThenInclude(me => me.IdEtiquetaNavigation)
                .ToListAsync(ct);

            // Filtramos por texto en memoria con normalización de tildes.
            // Así "gameza" encuentra "Gámeza", "tunja" encuentra "Tunja", etc.
            if (!string.IsNullOrWhiteSpace(texto))
            {
                var textoNorm = Normalizar(texto.Trim());
                municipios = municipios
                    .Where(m => Normalizar(m.Nombre).Contains(textoNorm))
                    .ToList();
            }

            return municipios.Select(MunicipioMapper.ToListadoDto).ToList();
        }

        /// <summary>
        /// Elimina tildes y convierte a minúsculas para comparación insensible a acentos.
        /// Ejemplo: "Gámeza" → "gameza"
        /// </summary>
        private static string Normalizar(string s)
        {
            return new string(
                s.Normalize(System.Text.NormalizationForm.FormD)
                 .Where(c => System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                             != System.Globalization.UnicodeCategory.NonSpacingMark)
                 .ToArray()
            ).ToLower();
        }

        public async Task<MunicipioFichaDto?> ObtenerFichaAsync(int id, CancellationToken ct = default)
        {
            var m = await _context.Municipios
                .Include(x => x.MunicipioEtiqueta).ThenInclude(me => me.IdEtiquetaNavigation)
                .Include(x => x.MunicipioFechaRelevantes).ThenInclude(mfr => mfr.IdFechaRelevanteNavigation) // nuevo
                .FirstOrDefaultAsync(x => x.IdMunicipio == id && x.Activo, ct);

            return m is null ? null : MunicipioMapper.ToFichaDto(m);
        }

        public async Task<(bool exito, string? error)> EditarAsync(int id, int idAdminSolicitante, MunicipioEditarDto dto, CancellationToken ct = default)
        {
            var municipio = await _context.Municipios.FindAsync(new object?[] { id }, ct);
            if (municipio is null) return (false, "Municipio no encontrado.");

            // Traemos el usuario con su rol cargado
            var admin = await _context.Usuarios
                .Include(u => u.IdRolNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == idAdminSolicitante, ct);

            if (admin is null) return (false, "Usuario administrador no encontrado.");

            // Verificamos permisos: AdminGeneral o AdminPrincipal puede editar cualquiera; AdminMunicipio solo el asignado
            bool esAdminGeneral = admin.IdRolNavigation?.Nombre == Roles.AdminGeneral; 
            bool esAdminDelMunicipio = admin.MunicipioAsignadoId == id;

            if (!esAdminGeneral && !esAdminDelMunicipio)
            {
                _logger.LogWarning("Usuario {IdUsuario} intentó editar el municipio {IdMunicipio} sin permisos", idAdminSolicitante, id);
                return (false, "No tienes permiso para editar este municipio.");
            }



            municipio.Nombre = dto.Nombre;
            municipio.Descripcion = dto.Descripcion;
            municipio.Clima = dto.Clima;
            municipio.ImagenUrl = dto.ImagenUrl;
            municipio.Historia = dto.Historia;

            // Sincronizamos las festividades en las tablas relacionales
            await SincronizarFechasRelevantesAsync(id, dto.FechasRelevantes, ct);
            await SincronizarEtiquetasAsync(id, dto.Etiquetas, ct);


            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Municipio {IdMunicipio} actualizado por el usuario {IdUsuario}", id, idAdminSolicitante);
            return (true, null);
        }


        private async Task SincronizarFechasRelevantesAsync(int idMunicipio, List<FechaRelevanteDto>? festividades, CancellationToken ct)
        {
            // Filtramos festividades válidas (que tengan nombre)
            var listaFestividades = festividades?
                .Where(f => !string.IsNullOrWhiteSpace(f.NombreFestividad))
                .ToList() ?? new List<FechaRelevanteDto>();

            // Consultamos las relaciones actuales del municipio en la BD
            var relacionesActuales = await _context.MunicipioFechaRelevantes
                .Include(mfr => mfr.IdFechaRelevanteNavigation)
                .Where(mfr => mfr.IdMunicipio == idMunicipio)
                .ToListAsync(ct);

            // 1. Desvincular las festividades que el usuario haya eliminado de la lista
            var nombresDeseados = listaFestividades
                .Select(f => f.NombreFestividad.Trim().ToLower())
                .ToList();

            var paraEliminar = relacionesActuales
                .Where(r => !nombresDeseados.Contains(r.IdFechaRelevanteNavigation.NombreFestividad.ToLower()))
                .ToList();

            if (paraEliminar.Any())
            {
                _context.MunicipioFechaRelevantes.RemoveRange(paraEliminar);
            }

            // 2. Crear o actualizar cada festividad recibida
            foreach (var fDto in listaFestividades)
            {
                var nombreLimpio = fDto.NombreFestividad.Trim();

                // Buscamos si la festividad ya existe en el catálogo general
                var festividad = await _context.FechaRelevantes
                    .FirstOrDefaultAsync(f => f.NombreFestividad.ToLower() == nombreLimpio.ToLower(), ct);

                if (festividad == null)
                {
                    // Si no existe, la creamos con todos sus datos completos
                    festividad = new FechaRelevante
                    {
                        NombreFestividad = nombreLimpio,
                        FechaInicio = fDto.FechaInicio,
                        FechaFin = fDto.FechaFin,
                        TipoFestividad = string.IsNullOrWhiteSpace(fDto.TipoFestividad) ? "Cultural" : fDto.TipoFestividad,
                        MesCelebracion = fDto.FechaInicio.Month, // Se guarda el mes automáticamente
                        Descripcion = fDto.Descripcion,
                        EsRecurrente = fDto.EsRecurrente,
                        Activo = true
                    };
                    _context.FechaRelevantes.Add(festividad);
                    await _context.SaveChangesAsync(ct);
                }
                else
                {
                    // Si ya existe, actualizamos sus datos
                    festividad.FechaInicio = fDto.FechaInicio;
                    festividad.FechaFin = fDto.FechaFin;
                    festividad.TipoFestividad = fDto.TipoFestividad ?? festividad.TipoFestividad;
                    festividad.MesCelebracion = fDto.FechaInicio.Month;
                    festividad.Descripcion = fDto.Descripcion ?? festividad.Descripcion;
                    festividad.EsRecurrente = fDto.EsRecurrente;
                }

                // Verificamos si ya está asociada a este municipio
                bool yaVinculada = relacionesActuales.Any(r => r.IdFechaRelevante == festividad.IdFechaRelevante);
                if (!yaVinculada)
                {
                    _context.MunicipioFechaRelevantes.Add(new MunicipioFechaRelevante
                    {
                        IdMunicipio = idMunicipio,
                        IdFechaRelevante = festividad.IdFechaRelevante,
                        FechaCreacion = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync(ct);
        }


        private async Task SincronizarEtiquetasAsync(int idMunicipio, List<string>? nombresEtiquetas, CancellationToken ct)
        {
            var etiquetasDeseadas = nombresEtiquetas?
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .Select(e => e.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList() ?? new List<string>();

            var relacionesActuales = await _context.MunicipioEtiqueta
                .Include(me => me.IdEtiquetaNavigation)
                .Where(me => me.IdMunicipio == idMunicipio)
                .ToListAsync(ct);

            // 1. Eliminar relaciones de etiquetas que fueron desmarcadas
            var paraEliminar = relacionesActuales
                .Where(r => !etiquetasDeseadas.Contains(r.IdEtiquetaNavigation.Nombre, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (paraEliminar.Any())
            {
                _context.MunicipioEtiqueta.RemoveRange(paraEliminar);
            }

            // 2. Asociar o crear las etiquetas deseadas
            foreach (var nombre in etiquetasDeseadas)
            {
                var etiqueta = await _context.Etiqueta
                    .FirstOrDefaultAsync(e => e.Nombre.ToLower() == nombre.ToLower(), ct);

                // Si no existe en el catálogo, se crea automáticamente
                if (etiqueta == null)
                {
                    etiqueta = new Etiqueta
                    {
                        Nombre = nombre,
                        Activo = true
                    };
                    _context.Etiqueta.Add(etiqueta);
                    await _context.SaveChangesAsync(ct);
                }

                // Si no está vinculada a este municipio, creamos el enlace
                bool yaVinculada = relacionesActuales.Any(r => r.IdEtiqueta == etiqueta.IdEtiqueta);
                if (!yaVinculada)
                {
                    _context.MunicipioEtiqueta.Add(new MunicipioEtiqueta
                    {
                        IdMunicipio = idMunicipio,
                        IdEtiqueta = etiqueta.IdEtiqueta
                    });
                }
            }

            await _context.SaveChangesAsync(ct);
        }


    }
}
