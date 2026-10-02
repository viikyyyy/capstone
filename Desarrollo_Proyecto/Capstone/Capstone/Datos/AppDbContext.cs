using Capstone.Modelos;
using Microsoft.EntityFrameworkCore;

namespace Capstone.Datos;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Ficha> Fichas => Set<Ficha>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<Backup> Backups => Set<Backup>();
    public DbSet<CodigoRecuperacion> CodigosRecuperacion => Set<CodigoRecuperacion>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("usuario");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Rut).HasColumnName("rut").HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.DvRut).HasColumnName("dv_rut").HasMaxLength(1).IsUnicode(false).IsRequired();
            entity.Property(e => e.Pnombre).HasColumnName("pnombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.Snombre).HasColumnName("snombre").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Apellidop).HasColumnName("apellidop").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.Apellidom).HasColumnName("apellidom").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(100).IsUnicode(false).IsRequired();
            entity.Property(e => e.ContrasenaHash).HasColumnName("contrasena_hash").HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.Rol).HasColumnName("rol").HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.HasIndex(e => new { e.Rut, e.DvRut }).IsUnique();
            entity.HasIndex(e => e.Correo).IsUnique();
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.ToTable("paciente");
            entity.HasKey(e => e.IdPaciente);
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Rut).HasColumnName("rut").HasMaxLength(10).IsUnicode(false).IsRequired();
            entity.Property(e => e.DvRut).HasColumnName("dv_rut").HasMaxLength(1).IsUnicode(false).IsRequired();
            entity.Property(e => e.Pnombre).HasColumnName("pnombre").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.Snombre).HasColumnName("snombre").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Apellidop).HasColumnName("apellidop").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.Apellidom).HasColumnName("apellidom").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.FechaNacimiento).HasColumnName("fecha_nacimiento").HasColumnType("date");
            entity.Property(e => e.Prevision).HasColumnName("prevision").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Fono).HasColumnName("fono").HasMaxLength(20).IsUnicode(false);
            entity.HasIndex(e => new { e.Rut, e.DvRut }).IsUnique();
            entity.HasOne(e => e.Usuario).WithMany(e => e.Pacientes).HasForeignKey(e => e.IdUsuario).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Ficha>(entity =>
        {
            entity.ToTable("ficha");
            entity.HasKey(e => e.IdFicha);
            entity.Property(e => e.IdFicha).HasColumnName("id_ficha");
            entity.Property(e => e.IdPaciente).HasColumnName("id_paciente");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.FechaAtencion).HasColumnName("fecha_atencion");
            entity.Property(e => e.MotivoConsulta).HasColumnName("motivo_consulta").HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.Diagnostico).HasColumnName("diagnostico").HasColumnType("varchar(max)");
            entity.Property(e => e.Tratamiento).HasColumnName("tratamiento").HasColumnType("varchar(max)");
            entity.Property(e => e.Observaciones).HasColumnName("observaciones").HasColumnType("varchar(max)");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.FechaModificacion).HasColumnName("fecha_modificacion");
            entity.HasOne(e => e.Paciente).WithMany(e => e.Fichas).HasForeignKey(e => e.IdPaciente).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Usuario).WithMany(e => e.Fichas).HasForeignKey(e => e.IdUsuario).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Archivo>(entity =>
        {
            entity.ToTable("archivo");
            entity.HasKey(e => e.IdArchivo);
            entity.Property(e => e.IdArchivo).HasColumnName("id_archivo");
            entity.Property(e => e.IdFicha).HasColumnName("id_ficha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.TipoArchivo).HasColumnName("tipo_archivo").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.NombreArchivo).HasColumnName("nombre_archivo").HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.UrlStorage).HasColumnName("url_storage").HasMaxLength(500).IsUnicode(false).IsRequired();
            entity.Property(e => e.HashIntegridad).HasColumnName("hash_integridad").HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.FechaCarga).HasColumnName("fecha_carga");
            entity.HasOne(e => e.Ficha).WithMany(e => e.Archivos).HasForeignKey(e => e.IdFicha).OnDelete(DeleteBehavior.NoAction);
            entity.HasOne(e => e.Usuario).WithMany(e => e.Archivos).HasForeignKey(e => e.IdUsuario).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.ToTable("auditoria");
            entity.HasKey(e => e.IdAuditoria);
            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Accion).HasColumnName("accion").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.EntidadAfectada).HasColumnName("entidad_afectada").HasMaxLength(50).IsUnicode(false).IsRequired();
            entity.Property(e => e.IdEntidadAfectada).HasColumnName("id_entidad_afectada");
            entity.Property(e => e.Detalle).HasColumnName("detalle").HasColumnType("varchar(max)");
            entity.Property(e => e.IpOrigen).HasColumnName("ip_origen").HasMaxLength(45).IsUnicode(false).IsRequired();
            entity.Property(e => e.FechaHora).HasColumnName("fecha_hora");
            entity.HasOne(e => e.Usuario).WithMany(e => e.Auditorias).HasForeignKey(e => e.IdUsuario).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Backup>(entity =>
        {
            entity.ToTable("backup");
            entity.HasKey(e => e.IdBackup);
            entity.Property(e => e.IdBackup).HasColumnName("id_backup");
            entity.Property(e => e.TipoBackup).HasColumnName("tipo_backup").HasMaxLength(30).IsUnicode(false).IsRequired();
            entity.Property(e => e.Estado).HasColumnName("estado").HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.TamanoBytes).HasColumnName("tamano_bytes");
            entity.Property(e => e.Destino).HasColumnName("destino").HasMaxLength(255).IsUnicode(false).IsRequired();
            entity.Property(e => e.MensajeError).HasColumnName("mensaje_error").HasColumnType("varchar(max)");
        });

        modelBuilder.Entity<CodigoRecuperacion>(entity =>
        {
            entity.ToTable("codigo_recuperacion");
            entity.HasKey(e => e.IdCodigo);
            entity.Property(e => e.IdCodigo).HasColumnName("id_codigo");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasMaxLength(20).IsUnicode(false).IsRequired();
            entity.Property(e => e.Expiracion).HasColumnName("expiracion");
            entity.Property(e => e.Usado).HasColumnName("usado");
            entity.HasOne(e => e.Usuario).WithMany().HasForeignKey(e => e.IdUsuario).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
