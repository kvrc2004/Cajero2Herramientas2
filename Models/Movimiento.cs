using System.ComponentModel.DataAnnotations;

namespace MiBanco.Models
{
    /// <summary>
    /// Clase que representa un movimiento bancario (transacción)
    /// Almacena el historial de todas las operaciones realizadas en las cuentas
    /// </summary>
    public class Movimiento
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // "Consignación", "Retiro", "Transferencia"

        [Required]
        public decimal Monto { get; set; }

        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public decimal SaldoAnterior { get; set; }

        [Required]
        public decimal SaldoNuevo { get; set; }

        [Required]
        public int CuentaId { get; set; }

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Movimiento() { }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public Movimiento(string tipo, decimal monto, string descripcion, decimal saldoAnterior, decimal saldoNuevo, int cuentaId)
        {
            Tipo = tipo;
            Monto = monto;
            Descripcion = descripcion;
            SaldoAnterior = saldoAnterior;
            SaldoNuevo = saldoNuevo;
            CuentaId = cuentaId;
        }

        /// <summary>
        /// Método para obtener la descripción formateada del movimiento
        /// </summary>
        public string ObtenerDescripcionCompleta()
        {
            return $"{Fecha:dd/MM/yyyy HH:mm} - {Tipo}: ${Monto:N2} - {Descripcion} - Saldo: ${SaldoNuevo:N2}";
        }

        /// <summary>
        /// Método para determinar si el movimiento es de débito o crédito
        /// </summary>
        public bool EsDebito()
        {
            return Tipo.ToLower().Contains("retiro") || Tipo.ToLower().Contains("transferencia enviada");
        }

        /// <summary>
        /// Método para determinar si el movimiento es de crédito
        /// </summary>
        public bool EsCredito()
        {
            return !EsDebito();
        }
    }
}