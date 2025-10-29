using System.ComponentModel.DataAnnotations;

namespace MiBanco.Models
{
    /// <summary>
    /// Clase abstracta que representa una cuenta bancaria genérica
    /// Implementa polimorfismo - será heredada por los diferentes tipos de cuenta
    /// </summary>
    public abstract class Cuenta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string NumeroCuenta { get; set; } = string.Empty;

        [Required]
        public decimal Saldo { get; set; } = 0;

        [Required]
        public int ClienteId { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Lista de movimientos (historial de transacciones)
        public List<Movimiento> Movimientos { get; set; } = new List<Movimiento>();

        /// <summary>
        /// Constructor protegido para ser usado por las clases hijas
        /// </summary>
        protected Cuenta() { }

        protected Cuenta(string numeroCuenta, int clienteId)
        {
            NumeroCuenta = numeroCuenta;
            ClienteId = clienteId;
        }

        /// <summary>
        /// Método abstracto para consignar dinero - será implementado por cada tipo de cuenta
        /// </summary>
        public abstract bool Consignar(decimal monto, string descripcion = "Consignación");

        /// <summary>
        /// Método abstracto para retirar dinero - será implementado por cada tipo de cuenta
        /// </summary>
        public abstract bool Retirar(decimal monto, string descripcion = "Retiro");

        /// <summary>
        /// Método virtual para transferir dinero - puede ser sobrescrito por las clases hijas
        /// </summary>
        public virtual bool Transferir(Cuenta cuentaDestino, decimal monto, string descripcion = "Transferencia")
        {
            if (monto <= 0)
                return false;

            if (Retirar(monto, $"Transferencia enviada - {descripcion}"))
            {
                cuentaDestino.Consignar(monto, $"Transferencia recibida - {descripcion}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Método protegido para registrar movimientos
        /// </summary>
        protected void RegistrarMovimiento(string tipo, decimal monto, string descripcion, decimal saldoAnterior)
        {
            var movimiento = new Movimiento
            {
                Fecha = DateTime.Now,
                Tipo = tipo,
                Monto = monto,
                Descripcion = descripcion,
                SaldoAnterior = saldoAnterior,
                SaldoNuevo = Saldo,
                CuentaId = Id
            };
            Movimientos.Add(movimiento);
        }

        /// <summary>
        /// Método virtual para obtener el tipo de cuenta como string
        /// </summary>
        public virtual string ObtenerTipoCuenta()
        {
            return GetType().Name;
        }

        /// <summary>
        /// Método virtual para obtener información específica de la cuenta
        /// </summary>
        public virtual string ObtenerInformacionEspecifica()
        {
            return $"Saldo: ${Saldo:N2}";
        }
    }
}