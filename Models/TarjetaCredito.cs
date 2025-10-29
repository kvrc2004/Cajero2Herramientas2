namespace MiBanco.Models
{
    /// <summary>
    /// Clase que representa una Tarjeta de Crédito
    /// Hereda de Cuenta e implementa las reglas específicas:
    /// - Funciona con línea de crédito (límite)
    /// - Intereses según número de cuotas:
    ///   * ≤2 cuotas: 0% interés
    ///   * ≤6 cuotas: 1.9% mensual
    ///   * ≥7 cuotas: 2.3% mensual
    /// </summary>
    public class TarjetaCredito : Cuenta
    {
        // Tasas de interés según cuotas
        private const decimal TASA_SIN_INTERES = 0.00m;      // ≤2 cuotas
        private const decimal TASA_INTERES_MEDIO = 0.019m;   // ≤6 cuotas (1.9%)
        private const decimal TASA_INTERES_ALTO = 0.023m;    // ≥7 cuotas (2.3%)

        public decimal LimiteCredito { get; set; } = 0;
        public decimal CreditoDisponible => LimiteCredito - Math.Abs(Saldo);

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public TarjetaCredito() : base() { }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public TarjetaCredito(string numeroCuenta, int clienteId, decimal limiteCredito) : base(numeroCuenta, clienteId)
        {
            LimiteCredito = limiteCredito;
            Saldo = 0; // Las tarjetas de crédito inician en 0 (sin deuda)
        }

        /// <summary>
        /// Implementación específica del método Consignar para Tarjeta de Crédito
        /// Consignar = Pagar deuda
        /// </summary>
        public override bool Consignar(decimal monto, string descripcion = "Pago tarjeta crédito")
        {
            if (monto <= 0)
                return false;

            decimal saldoAnterior = Saldo;
            
            // En tarjeta de crédito, consignar es pagar la deuda
            // Saldo negativo = deuda, Saldo positivo = saldo a favor
            Saldo += monto;

            RegistrarMovimiento("Pago", monto, descripcion, saldoAnterior);
            return true;
        }

        /// <summary>
        /// Implementación específica del método Retirar para Tarjeta de Crédito
        /// Retirar = Usar crédito (hacer compra/avance)
        /// </summary>
        public override bool Retirar(decimal monto, string descripcion = "Compra tarjeta crédito")
        {
            if (monto <= 0)
                return false;

            // Verificar si hay crédito disponible
            if (monto > CreditoDisponible)
                return false;

            decimal saldoAnterior = Saldo;
            Saldo -= monto;

            RegistrarMovimiento("Compra", monto, descripcion, saldoAnterior);
            return true;
        }

        /// <summary>
        /// Método específico para realizar compras en cuotas
        /// Calcula automáticamente los intereses según el número de cuotas
        /// </summary>
        public bool RealizarCompraEnCuotas(decimal monto, int numeroCuotas, string descripcion = "Compra en cuotas")
        {
            if (monto <= 0 || numeroCuotas <= 0)
                return false;

            if (monto > CreditoDisponible)
                return false;

            // Calcular tasa de interés según número de cuotas
            decimal tasaInteres = ObtenerTasaInteresPorCuotas(numeroCuotas);
            
            // Calcular el valor total con intereses
            decimal montoTotal = monto;
            if (tasaInteres > 0)
            {
                // Fórmula de interés compuesto: Monto * (1 + tasa)^cuotas
                montoTotal = monto * (decimal)Math.Pow((double)(1 + tasaInteres), numeroCuotas);
            }

            // Calcular pago mensual
            decimal pagoMensual = montoTotal / numeroCuotas;

            decimal saldoAnterior = Saldo;
            Saldo -= montoTotal; // Se debita el monto total con intereses

            string detalleCompra = $"{descripcion} - {numeroCuotas} cuotas - Pago mensual: ${pagoMensual:N2}";
            if (tasaInteres > 0)
            {
                detalleCompra += $" - Interés: {tasaInteres:P2} mensual";
            }

            RegistrarMovimiento("Compra en cuotas", montoTotal, detalleCompra, saldoAnterior);
            
            return true;
        }

        /// <summary>
        /// Método para obtener la tasa de interés según el número de cuotas
        /// </summary>
        private decimal ObtenerTasaInteresPorCuotas(int cuotas)
        {
            if (cuotas <= 2)
                return TASA_SIN_INTERES;
            else if (cuotas <= 6)
                return TASA_INTERES_MEDIO;
            else
                return TASA_INTERES_ALTO;
        }

        /// <summary>
        /// Método para calcular el pago mensual de una compra en cuotas
        /// </summary>
        public decimal CalcularPagoMensual(decimal monto, int cuotas)
        {
            if (cuotas <= 0) return 0;

            decimal tasaInteres = ObtenerTasaInteresPorCuotas(cuotas);
            decimal montoTotal = monto;

            if (tasaInteres > 0)
            {
                montoTotal = monto * (decimal)Math.Pow((double)(1 + tasaInteres), cuotas);
            }

            return montoTotal / cuotas;
        }

        /// <summary>
        /// Override del método para obtener información específica
        /// </summary>
        public override string ObtenerInformacionEspecifica()
        {
            decimal deuda = Saldo < 0 ? Math.Abs(Saldo) : 0;
            return $"Límite: ${LimiteCredito:N2} | Disponible: ${CreditoDisponible:N2} | Deuda: ${deuda:N2}";
        }

        /// <summary>
        /// Override del método para obtener el tipo de cuenta
        /// </summary>
        public override string ObtenerTipoCuenta()
        {
            return "Tarjeta de Crédito";
        }

        /// <summary>
        /// Método para verificar si la tarjeta tiene deuda
        /// </summary>
        public bool TieneDeuda()
        {
            return Saldo < 0;
        }

        /// <summary>
        /// Método para obtener el monto de la deuda actual
        /// </summary>
        public decimal ObtenerDeuda()
        {
            return Saldo < 0 ? Math.Abs(Saldo) : 0;
        }

        /// <summary>
        /// Override del método transferir - no aplica para tarjetas de crédito
        /// </summary>
        public override bool Transferir(Cuenta cuentaDestino, decimal monto, string descripcion = "Transferencia")
        {
            // Las tarjetas de crédito no pueden hacer transferencias directas
            // Se debe realizar un avance en efectivo (retiro) y luego consignar en la cuenta destino
            return false;
        }

        /// <summary>
        /// Método para realizar avance en efectivo que puede ser transferido
        /// </summary>
        public bool AvanceEfectivo(decimal monto, Cuenta cuentaDestino, string descripcion = "Avance en efectivo")
        {
            if (Retirar(monto, $"Avance efectivo - {descripcion}"))
            {
                return cuentaDestino.Consignar(monto, $"Avance recibido - {descripcion}");
            }
            return false;
        }
    }
}