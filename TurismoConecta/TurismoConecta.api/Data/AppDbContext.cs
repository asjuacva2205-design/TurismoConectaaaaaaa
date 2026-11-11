using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TurismoConecta.api.Models;

namespace TurismoConecta.api.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Categoria> Categoria { get; set; }

    public virtual DbSet<Departamento> Departamentos { get; set; }

    public virtual DbSet<Etiqueta> Etiqueta { get; set; }

    public virtual DbSet<Favorito> Favoritos { get; set; }

    public virtual DbSet<FechaRelevante> FechaRelevantes { get; set; }

    public virtual DbSet<GaleriaNegocio> GaleriaNegocios { get; set; }

    public virtual DbSet<Itinerario> Itinerarios { get; set; }
    public virtual DbSet<SitioTuristico> SitiosTuristicos { get; set; }

    public virtual DbSet<ItinerarioDetalle> ItinerarioDetalles { get; set; }

    public virtual DbSet<Municipio> Municipios { get; set; }

    public virtual DbSet<MunicipioEtiqueta> MunicipioEtiqueta { get; set; }

    public virtual DbSet<MunicipioFechaRelevante> MunicipioFechaRelevantes { get; set; }

    public virtual DbSet<Negocio> Negocios { get; set; }

    public virtual DbSet<NegocioEtiqueta> NegocioEtiqueta { get; set; }

    public virtual DbSet<Notificacion> Notificacions { get; set; }

    public virtual DbSet<Reseña> Reseñas { get; set; }

    public virtual DbSet<Rol> Rols { get; set; }

    public virtual DbSet<TiposNotificacion> TiposNotificacions { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Modern_Spanish_CI_AI");

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__Categori__8A3D240C145FC97D");

            entity.HasIndex(e => e.Nombre, "UQ__Categori__72AFBCC62B055AE0").IsUnique();

            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
            entity.Property(e => e.Icono)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("icono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");
            entity.Property(e => e.AplicaNegocio).HasColumnName("aplicaNegocio");
            entity.Property(e => e.AplicaSitio).HasColumnName("aplicaSitio");
        });

        modelBuilder.Entity<Departamento>(entity =>
        {
            entity.HasKey(e => e.IdDepartamento).HasName("PK__Departam__C225F98D7F570F44");

            entity.ToTable("Departamento");

            entity.Property(e => e.IdDepartamento).HasColumnName("idDepartamento");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(500)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");
            entity.Property(e => e.Pais)
                .HasMaxLength(100)
                .HasDefaultValue("Colombia")
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("pais");
        });

        modelBuilder.Entity<Etiqueta>(entity =>
        {
            entity.HasKey(e => e.IdEtiqueta);

            entity.Property(e => e.IdEtiqueta).HasColumnName("idEtiqueta");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
            entity.Property(e => e.Icono)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("icono");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")       
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Favorito>(entity =>
        {
            entity.HasKey(e => e.IdFavorito);

            entity.ToTable("Favorito");

            entity.Property(e => e.IdFavorito).HasColumnName("idFavorito");
            entity.Property(e => e.FechaGuardado)
                .HasDefaultValueSql("(sysdatetime())", "DF_Favorito_fechaGuardado")
                .HasColumnName("fechaGuardado");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdMunicipio)
                .HasConstraintName("FK_Favorito_Municipio");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdNegocio)
                .HasConstraintName("FK_Favorito_Negocio");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Favoritos)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorito_Usuario");
        });

        modelBuilder.Entity<FechaRelevante>(entity =>
        {
            entity.HasKey(e => e.IdFechaRelevante);

            entity.ToTable("FechaRelevante");

            entity.Property(e => e.IdFechaRelevante).HasColumnName("idFechaRelevante");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Descripcion).HasColumnName("descripcion");
            entity.Property(e => e.EsRecurrente)
                .HasDefaultValue(true)
                .HasColumnName("esRecurrente");
            entity.Property(e => e.FechaFin).HasColumnName("fechaFin");
            entity.Property(e => e.FechaInicio).HasColumnName("fechaInicio");
            entity.Property(e => e.Historia).HasColumnName("historia");
            entity.Property(e => e.MesCelebracion).HasColumnName("mesCelebracion");
            entity.Property(e => e.NombreFestividad)
                .HasMaxLength(150)
                .HasColumnName("nombreFestividad");
            entity.Property(e => e.Recomendaciones).HasColumnName("recomendaciones");
            entity.Property(e => e.TipoFestividad)
                .HasMaxLength(50)
                .HasColumnName("tipoFestividad");
        });

        modelBuilder.Entity<GaleriaNegocio>(entity =>
        {
            entity.HasKey(e => e.IdGaleria).HasName("PK__GaleriaN__F48A5B5461E947ED");

            entity.ToTable("GaleriaNegocio");

            entity.Property(e => e.IdGaleria).HasColumnName("idGaleria");
            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("imagenUrl");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany(p => p.GaleriaNegocios)
                .HasForeignKey(d => d.IdNegocio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GaleriaNegocio_Negocio");
        });

        modelBuilder.Entity<SitioTuristico>(entity =>
        {
            entity.ToTable("SitiosTuristicos");
            entity.HasKey(e => e.IdSitioTuristico);

            // ── Columnas existentes ──
            entity.Property(e => e.IdSitioTuristico).HasColumnName("idSitiosTuristicos");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Descripcion).HasColumnName("descripcion").HasMaxLength(1000);
            entity.Property(e => e.ImagenUrl).HasColumnName("imagenUrl").HasMaxLength(500);
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.IdReseña).HasColumnName("idReseña");

            // ── Columnas nuevas ──
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.Altitud).HasColumnName("altitud");
            entity.Property(e => e.Destacado).HasColumnName("destacado").HasDefaultValue(false);
            entity.Property(e => e.Activo).HasColumnName("activo").HasDefaultValue(true);

            // ── Relaciones ──
            entity.HasOne(e => e.IdMunicipioNavigation)
                .WithMany()
                .HasForeignKey(e => e.IdMunicipio)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SitiosTuristicos_Municipio");

            entity.HasOne(e => e.IdReseñaNavigation)
                .WithMany()
                .HasForeignKey(e => e.IdReseña)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.IdCategoriaNavigation)
                .WithMany(c => c.SitiosTuristicos)
                .HasForeignKey(e => e.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SitiosTuristicos_Categoria");
        });

        modelBuilder.Entity<Itinerario>(entity =>
        {
            entity.HasKey(e => e.IdItinerario).HasName("PK__Itinerar__B201E2D6E29D1263");

            entity.ToTable("Itinerario");

            entity.Property(e => e.IdItinerario).HasColumnName("idItinerario");
            entity.Property(e => e.CodigoCompartir)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("codigoCompartir");
            entity.Property(e => e.Compartido).HasColumnName("compartido");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaFin).HasColumnName("fechaFin");
            entity.Property(e => e.FechaInicio).HasColumnName("fechaInicio");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");
            entity.Property(e => e.Observaciones)
                .HasMaxLength(150)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("observaciones");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Itinerarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Itinerarios_Usuarios");
        });

        modelBuilder.Entity<ItinerarioDetalle>(entity =>
        {
            entity.HasKey(e => e.IdItinerarioDetalle).HasName("PK__Itinerar__CCC310BCE5DA3F0B");

            entity.ToTable("ItinerarioDetalle");

            entity.Property(e => e.IdItinerarioDetalle).HasColumnName("idItinerarioDetalle");
            entity.Property(e => e.DiaNumero)
                .HasDefaultValue(1)
                .HasColumnName("diaNumero");
            entity.Property(e => e.DistanciaKm)
                .HasColumnType("decimal(6, 2)")
                .HasColumnName("distanciaKm");
            entity.Property(e => e.FechaVisita).HasColumnName("fechaVisita");
            entity.Property(e => e.IdItinerario).HasColumnName("idItinerario");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.Orden)
                .HasDefaultValue(1)
                .HasColumnName("orden");
            entity.Property(e => e.TiempoEstimadoMin).HasColumnName("tiempoEstimadoMin");

            entity.HasOne(d => d.IdItinerarioNavigation).WithMany(p => p.ItinerarioDetalles)
                .HasForeignKey(d => d.IdItinerario)
                .HasConstraintName("FK_ItinerarioDetalle_Itinerarios");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.ItinerarioDetalles)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ItinerarioDetalle_Municipios");
        });

        modelBuilder.Entity<Municipio>(entity =>
        {
            entity.HasKey(e => e.IdMunicipio).HasName("PK__Municipi__FD10E40022C53579");

            entity.ToTable("Municipio");

            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Clima)
                .HasMaxLength(200)
                .UseCollation("Modern_Spanish_CI_AI")   
                .HasColumnName("clima");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(1000)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.Historia)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("historia");
            entity.Property(e => e.IdDepartamento).HasColumnName("idDepartamento");
            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("imagenUrl");
            entity.Property(e => e.Latitud)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitud");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");

            entity.HasOne(d => d.IdDepartamentoNavigation).WithMany(p => p.Municipios)
                .HasForeignKey(d => d.IdDepartamento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Municipios_Departamentos");
        });

        modelBuilder.Entity<MunicipioEtiqueta>(entity =>
        {
            entity.HasKey(e => e.IdMunicipioEtiqueta);

            entity.Property(e => e.IdMunicipioEtiqueta).HasColumnName("idMunicipioEtiqueta");
            entity.Property(e => e.IdEtiqueta).HasColumnName("idEtiqueta");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");

            entity.HasOne(d => d.IdEtiquetaNavigation).WithMany(p => p.MunicipioEtiqueta)
                .HasForeignKey(d => d.IdEtiqueta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MunicipioEtiqueta_Etiqueta");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.MunicipioEtiqueta)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MunicipioEtiqueta_Municipio");
        });

        modelBuilder.Entity<MunicipioFechaRelevante>(entity =>
        {
            entity.HasKey(e => e.IdMunicipioFechaRelevante);

            entity.ToTable("MunicipioFechaRelevante");

            entity.Property(e => e.IdMunicipioFechaRelevante).HasColumnName("idMunicipioFechaRelevante");
            entity.Property(e => e.DescripcionLocal).HasColumnName("descripcionLocal");
            entity.Property(e => e.EsFestividadPrincipal).HasColumnName("esFestividadPrincipal");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.FotoUrl).HasColumnName("fotoUrl");
            entity.Property(e => e.IdFechaRelevante).HasColumnName("idFechaRelevante");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.Orden)
                .HasDefaultValue(1)
                .HasColumnName("orden");

            entity.HasOne(d => d.IdFechaRelevanteNavigation).WithMany(p => p.MunicipioFechaRelevantes)
                .HasForeignKey(d => d.IdFechaRelevante)
                .HasConstraintName("FK_MunicipioFechaRelevante_FechaRelevante");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.MunicipioFechaRelevantes)
                .HasForeignKey(d => d.IdMunicipio)
                .HasConstraintName("FK_MunicipioFechaRelevante_Municipio");
        });

        modelBuilder.Entity<Negocio>(entity =>
        {
            entity.HasKey(e => e.IdNegocio);

            entity.ToTable("Negocio");

            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(1000)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
            entity.Property(e => e.Direccion)
                .HasMaxLength(250)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("direccion");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasDefaultValue("Pendiente", "DF_Negocio_estado")
                .HasColumnName("estado");
            entity.Property(e => e.FechaAprobacion).HasColumnName("fechaAprobacion");
            entity.Property(e => e.FechaRegistro).HasColumnName("fechaRegistro");
            entity.Property(e => e.Horario)
                .HasMaxLength(200)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("horario");
            entity.Property(e => e.IdCategoria).HasColumnName("idCategoria");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.ImagenPrincipalUrl)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("imagenPrincipalUrl");
            entity.Property(e => e.Latitud)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("latitud");
            entity.Property(e => e.Longitud)
                .HasColumnType("decimal(9, 6)")
                .HasColumnName("longitud");
            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Negocios)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Negocio_Categoria");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Negocios)
                .HasForeignKey(d => d.IdMunicipio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Negocio_Municipio");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Negocios)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK_Negocio_Usuario");
        });

        modelBuilder.Entity<NegocioEtiqueta>(entity =>
        {
            entity.HasKey(e => e.IdNegocioEtiqueta);

            entity.Property(e => e.IdNegocioEtiqueta).HasColumnName("idNegocioEtiqueta");
            entity.Property(e => e.IdEtiqueta).HasColumnName("idEtiqueta");
            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");

            entity.HasOne(d => d.IdEtiquetaNavigation).WithMany(p => p.NegocioEtiqueta)
                .HasForeignKey(d => d.IdEtiqueta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NegocioEtiqueta_Etiqueta");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany(p => p.NegocioEtiqueta)
                .HasForeignKey(d => d.IdNegocio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_NegocioEtiqueta_Negocio");
        });

        modelBuilder.Entity<Notificacion>(entity =>
        {
            entity.HasKey(e => e.IdNotificacion).HasName("PK__Notifica__AFE1D7E4960044C0");

            entity.ToTable("Notificacion");

            entity.Property(e => e.IdNotificacion).HasColumnName("idNotificacion");
            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fechaCreacion");
            entity.Property(e => e.IdTipoNotificacion).HasColumnName("idTipoNotificacion");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Leida).HasColumnName("leida");
            entity.Property(e => e.Mensaje)
                .HasMaxLength(500)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("mensaje");

            entity.HasOne(d => d.IdTipoNotificacionNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.IdTipoNotificacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificaciones_Tipos");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Notificacions)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notificaciones_Usuarios");
        });

        modelBuilder.Entity<Reseña>(entity =>
        {
            entity.HasKey(e => e.IdReseña);

            entity.ToTable("Reseña");

            entity.Property(e => e.IdReseña).HasColumnName("idReseña");
            entity.Property(e => e.Calificacion).HasColumnName("calificacion");
            entity.Property(e => e.Comentario)
                .HasMaxLength(1000)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("comentario");
            entity.Property(e => e.FechaCreacion).HasColumnName("fechaCreacion");
            entity.Property(e => e.FechaRespuesta).HasColumnName("fechaRespuesta");
            entity.Property(e => e.IdMunicipio).HasColumnName("idMunicipio");
            entity.Property(e => e.IdNegocio).HasColumnName("idNegocio");
            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Moderada).HasColumnName("moderada");
            entity.Property(e => e.Respuesta)
                .HasMaxLength(1000)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("respuesta");

            entity.HasOne(d => d.IdMunicipioNavigation).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.IdMunicipio)
                .HasConstraintName("FK_Reseña_Municipio");

            entity.HasOne(d => d.IdNegocioNavigation).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.IdNegocio)
                .HasConstraintName("FK_Reseña_Negocio");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Reseñas)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Reseña_Usuario");
        });

        modelBuilder.Entity<Rol>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Rol__3C872F76177276D9");

            entity.ToTable("Rol");

            entity.HasIndex(e => e.Nombre, "UQ__Rol__72AFBCC66E08C31C").IsUnique();

            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<TiposNotificacion>(entity =>
        {
            entity.HasKey(e => e.IdTipoNotificacion).HasName("PK__TiposNot__6F70CA63E933ECF1");

            entity.ToTable("TiposNotificacion");

            entity.HasIndex(e => e.Codigo, "UQ__TiposNot__40F9A206DA8B2BA4").IsUnique();

            entity.Property(e => e.IdTipoNotificacion).HasColumnName("idTipoNotificacion");
            entity.Property(e => e.Codigo)
                .HasMaxLength(50)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("codigo");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("descripcion");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuario__645723A6F4DEE701");

            entity.ToTable("Usuario");

            entity.HasIndex(e => e.Email, "UQ__Usuario__AB6E6164279433C1").IsUnique();

            entity.Property(e => e.IdUsuario).HasColumnName("idUsuario");
            entity.Property(e => e.Activo)
                .HasDefaultValue(true)
                .HasColumnName("activo");
            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("apellido");
            entity.Property(e => e.Email)
                .HasMaxLength(200)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("email");
            entity.Property(e => e.EmailConfirmado).HasColumnName("emailConfirmado");
            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("(sysdatetime())")
                .HasColumnName("fechaRegistro");
            entity.Property(e => e.FotoUrl)
                .IsUnicode(false)
                .UseCollation("Modern_Spanish_CI_AI");
            entity.Property(e => e.IdRol).HasColumnName("idRol");
            entity.Property(e => e.MunicipioAsignadoId).HasColumnName("municipioAsignadoId");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(300)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("passwordHash");
            entity.Property(e => e.PasswordResetToken)
                .HasMaxLength(256)
                .UseCollation("Modern_Spanish_CI_AI");
            entity.Property(e => e.Telefono)
                .HasMaxLength(30)
                .UseCollation("Modern_Spanish_CI_AI")
                .HasColumnName("telefono");

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");

            entity.HasOne(d => d.MunicipioAsignado).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.MunicipioAsignadoId)
                .HasConstraintName("FK_Usuarios_Municipios");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
