using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HotelAPI.Models;

public partial class GestionHotelContext : DbContext
{
    public GestionHotelContext()
    {
    }

    public GestionHotelContext(DbContextOptions<GestionHotelContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Habitacione> Habitaciones { get; set; }

    public virtual DbSet<MetodosPago> MetodosPagos { get; set; }

    public virtual DbSet<Pago> Pagos { get; set; }

    public virtual DbSet<Reserva> Reservas { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=AZAEL;Initial Catalog=GestionHotel;Integrated Security=True;Encrypt=False;Trust Server Certificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Clientes__3213E83FACE564D0");

            entity.HasIndex(e => e.DocumentoIdentidad, "UQ__Clientes__1A03B13F8E4589F8").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Clientes__AB6E61643DEC3EC5").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Apellido)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("apellido");
            entity.Property(e => e.DocumentoIdentidad)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("documento_identidad");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Telefono)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("telefono");
        });

        modelBuilder.Entity<Habitacione>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Habitaci__3213E83FEE584F21");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Numero)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("numero");
            entity.Property(e => e.PrecioPorNoche)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("precio_por_noche");
            entity.Property(e => e.Tipo)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("tipo");
        });

        modelBuilder.Entity<MetodosPago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Metodos___3213E83F1D0DDAA6");

            entity.ToTable("Metodos_Pago");

            entity.HasIndex(e => e.Nombre, "UQ__Metodos___72AFBCC6016E0006").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pagos__3213E83F11D2757B");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DetallesPago)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("detalles_pago");
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
            entity.Property(e => e.MetodoPagoId).HasColumnName("metodo_pago_id");
            entity.Property(e => e.MontoPago)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_pago");
            entity.Property(e => e.ReferenciaTransaccion)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("referencia_transaccion");
            entity.Property(e => e.ReservaId).HasColumnName("reserva_id");

            entity.HasOne(d => d.MetodoPago).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.MetodoPagoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__metodo_pa__45F365D3");

            entity.HasOne(d => d.Reserva).WithMany(p => p.Pagos)
                .HasForeignKey(d => d.ReservaId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Pagos__reserva_i__44FF419A");
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Reservas__3213E83FB84BFCCB");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClienteId).HasColumnName("cliente_id");
            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("estado_pago");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.HabitacionId).HasColumnName("habitacion_id");
            entity.Property(e => e.MontoPagado)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("monto_pagado");
            entity.Property(e => e.SaldoPendiente)
                .HasComputedColumnSql("([total]-[monto_pagado])", true)
                .HasColumnType("decimal(11, 2)")
                .HasColumnName("saldo_pendiente");
            entity.Property(e => e.Total)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total");

            entity.HasOne(d => d.Cliente).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.ClienteId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservas__client__3E52440B");

            entity.HasOne(d => d.Habitacion).WithMany(p => p.Reservas)
                .HasForeignKey(d => d.HabitacionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Reservas__habita__3D5E1FD2");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3213E83F94FABC39");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__AB6E6164A63B5EE4").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ActualizadoEn)
                .HasColumnType("datetime")
                .HasColumnName("actualizado_en");
            entity.Property(e => e.CreadoEn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creado_en");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.Estado)
                .HasDefaultValue(true)
                .HasColumnName("estado");
            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.Rol)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("rol");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
