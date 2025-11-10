using Microsoft.EntityFrameworkCore;
using MiBanco.Models;

namespace MiBanco.Data
{
    /// <summary>
    /// Contexto de base de datos para Entity Framework Core
    /// Conecta las clases del modelo con las tablas de SQL Server
    /// </summary>
    public class MiBancoDbContext : DbContext
    {
        public MiBancoDbContext(DbContextOptions<MiBancoDbContext> options) : base(options)
        {
        }

        // DbSets - Representan las tablas en la base de datos
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Cuenta> Cuentas { get; set; }
        public DbSet<CuentaAhorros> CuentasAhorros { get; set; }
        public DbSet<CuentaCorriente> CuentasCorrientes { get; set; }
        public DbSet<TarjetaCredito> TarjetasCredito { get; set; }
        public DbSet<Movimiento> Movimientos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de herencia TPH (Table Per Hierarchy)
            modelBuilder.Entity<Cuenta>()
                .HasDiscriminator<string>("TipoCuenta")
                .HasValue<CuentaAhorros>("CuentaAhorros")
                .HasValue<CuentaCorriente>("CuentaCorriente")
                .HasValue<TarjetaCredito>("TarjetaCredito");

            // Configuración de Cliente
            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("Clientes");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Identificacion)
                    .HasMaxLength(20)
                    .IsRequired();
                
                entity.Property(e => e.Nombre)
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(e => e.Celular)
                    .HasMaxLength(15)
                    .IsRequired();
                
                entity.Property(e => e.Usuario)
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property(e => e.Clave)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.HasIndex(e => e.Usuario).IsUnique();
                entity.HasIndex(e => e.Identificacion).IsUnique();

                // Relación con Cuentas
                entity.HasMany(e => e.Cuentas)
                    .WithOne()
                    .HasForeignKey(c => c.ClienteId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración de Cuenta
            modelBuilder.Entity<Cuenta>(entity =>
            {
                entity.ToTable("Cuentas");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.NumeroCuenta)
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property(e => e.Saldo)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();

                entity.HasIndex(e => e.NumeroCuenta).IsUnique();

                // Relación con Movimientos
                entity.HasMany(e => e.Movimientos)
                    .WithOne()
                    .HasForeignKey(m => m.CuentaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configuración específica de CuentaAhorros
            modelBuilder.Entity<CuentaAhorros>(entity =>
            {
                entity.Property(e => e.UltimaFechaCalculoInteres)
                    .IsRequired();
            });

            // Configuración específica de CuentaCorriente
            modelBuilder.Entity<CuentaCorriente>(entity =>
            {
                entity.Property(e => e.MontoSobregiro)
                    .HasColumnType("decimal(18,2)")
                    .HasDefaultValue(0);
            });

            // Configuración específica de TarjetaCredito
            modelBuilder.Entity<TarjetaCredito>(entity =>
            {
                entity.Property(e => e.LimiteCredito)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });

            // Configuración de Movimiento
            modelBuilder.Entity<Movimiento>(entity =>
            {
                entity.ToTable("Movimientos");
                entity.HasKey(e => e.Id);
                
                entity.Property(e => e.Tipo)
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property(e => e.Monto)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.Descripcion)
                    .HasMaxLength(200);
                
                entity.Property(e => e.SaldoAnterior)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
                
                entity.Property(e => e.SaldoNuevo)
                    .HasColumnType("decimal(18,2)")
                    .IsRequired();
            });
        }
    }
}
