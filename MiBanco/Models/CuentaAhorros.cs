namespace MiBanco.Models
{
    /// <summary>
    /// Clase que representa una Cuenta de Ahorros
    /// Hereda de Cuenta e implementa las reglas específicas:
    /// - Genera interés del 1.5% mensual al momento del retiro
    /// - Diseñada para guardar dinero de forma segura
    /// </summary>
    public class CuentaAhorros : Cuenta
    {
        // Tasa de interés mensual para cuentas de ahorro (1.5%)
        private const decimal TASA_INTERES_MENSUAL = 0.015m;

        public DateTime UltimaFechaCalculoInteres { get; set; } = DateTime.Now;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public CuentaAhorros() : base() { }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public CuentaAhorros(string numeroCuenta, int clienteId) : base(numeroCuenta, clienteId)
        {
            UltimaFechaCalculoInteres = DateTime.Now;
        }

        /// <summary>
        /// Implementación específica del método Consignar para Cuenta de Ahorros
        /// </summary>
        public override bool Consignar(decimal monto, string descripcion = "Consignación")
        {
            if (monto <= 0)
                return false;

            decimal saldoAnterior = Saldo;
            Saldo += monto;

            RegistrarMovimiento("Consignación", monto, descripcion, saldoAnterior);
            return true;
        }

        /// <summary>
        /// Implementación específica del método Retirar para Cuenta de Ahorros
        /// Calcula e aplica intereses del 1.5% mensual antes del retiro
        /// </summary>
        public override bool Retirar(decimal monto, string descripcion = "Retiro")
        {
            if (monto <= 0)
                return false;

            // Calcular y aplicar intereses antes del retiro
            CalcularYAplicarIntereses();

            // Verificar si hay suficientes fondos después de aplicar intereses
            if (monto > Saldo)
                return false;

            decimal saldoAnterior = Saldo;
            Saldo -= monto;

            RegistrarMovimiento("Retiro", monto, descripcion, saldoAnterior);
            return true;
        }

        /// <summary>
        /// Método para calcular y aplicar intereses mensuales
        /// Se ejecuta automáticamente en cada retiro
        /// </summary>
        public void CalcularYAplicarIntereses()
        {
            DateTime fechaActual = DateTime.Now;
            
            // Calcular cuántos meses han pasado desde el último cálculo de intereses
            int mesesTranscurridos = CalcularMesesTranscurridos(UltimaFechaCalculoInteres, fechaActual);

            if (mesesTranscurridos > 0 && Saldo > 0)
            {
                decimal saldoAnterior = Saldo;
                decimal interesesGenerados = 0;

                // Aplicar interés compuesto mensual
                for (int i = 0; i < mesesTranscurridos; i++)
                {
                    decimal interesMensual = Saldo * TASA_INTERES_MENSUAL;
                    Saldo += interesMensual;
                    interesesGenerados += interesMensual;
                }

                // Registrar el movimiento de intereses
                if (interesesGenerados > 0)
                {
                    RegistrarMovimiento("Intereses Ahorros", interesesGenerados, 
                        $"Intereses del {TASA_INTERES_MENSUAL:P2} por {mesesTranscurridos} mes(es)", saldoAnterior);
                }

                UltimaFechaCalculoInteres = fechaActual;
            }
        }

        /// <summary>
        /// Método auxiliar para calcular meses transcurridos entre dos fechas
        /// </summary>
        private int CalcularMesesTranscurridos(DateTime fechaInicio, DateTime fechaFin)
        {
            int años = fechaFin.Year - fechaInicio.Year;
            int meses = fechaFin.Month - fechaInicio.Month;
            return años * 12 + meses;
        }

        /// <summary>
        /// Override del método para obtener información específica
        /// </summary>
        public override string ObtenerInformacionEspecifica()
        {
            return $"Saldo: ${Saldo:N2} | Interés: {TASA_INTERES_MENSUAL:P2} mensual | Último cálculo: {UltimaFechaCalculoInteres:dd/MM/yyyy}";
        }

        /// <summary>
        /// Override del método para obtener el tipo de cuenta
        /// </summary>
        public override string ObtenerTipoCuenta()
        {
            return "Cuenta de Ahorros";
        }

        /// <summary>
        /// Método para obtener los intereses proyectados para el próximo mes
        /// </summary>
        public decimal ObtenerInteresesProyectados()
        {
            return Saldo * TASA_INTERES_MENSUAL;
        }
    }
}