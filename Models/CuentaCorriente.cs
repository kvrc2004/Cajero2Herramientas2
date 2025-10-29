namespace MiBanco.Models
{
    /// <summary>
    /// Clase que representa una Cuenta Corriente
    /// Hereda de Cuenta e implementa las reglas específicas:
    /// - Permite sobregiro del 20% sobre el saldo actual
    /// - No genera intereses
    /// - Diseñada para transacciones frecuentes
    /// </summary>
    public class CuentaCorriente : Cuenta
    {
        // Porcentaje de sobregiro permitido (20%)
        private const decimal PORCENTAJE_SOBREGIRO = 0.20m;

        public decimal MontoSobregiro { get; set; } = 0;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public CuentaCorriente() : base() { }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public CuentaCorriente(string numeroCuenta, int clienteId) : base(numeroCuenta, clienteId) { }

        /// <summary>
        /// Implementación específica del método Consignar para Cuenta Corriente
        /// </summary>
        public override bool Consignar(decimal monto, string descripcion = "Consignación")
        {
            if (monto <= 0)
                return false;

            decimal saldoAnterior = Saldo;
            
            // Si hay sobregiro, primero se cubre el sobregiro
            if (MontoSobregiro > 0)
            {
                if (monto >= MontoSobregiro)
                {
                    // El monto cubre completamente el sobregiro
                    decimal exceso = monto - MontoSobregiro;
                    MontoSobregiro = 0;
                    Saldo += exceso;
                    
                    RegistrarMovimiento("Consignación", monto, 
                        $"{descripcion} - Cancelación sobregiro", saldoAnterior);
                }
                else
                {
                    // El monto cubre parcialmente el sobregiro
                    MontoSobregiro -= monto;
                    
                    RegistrarMovimiento("Consignación", monto, 
                        $"{descripcion} - Abono a sobregiro", saldoAnterior);
                }
            }
            else
            {
                // No hay sobregiro, consignación normal
                Saldo += monto;
                RegistrarMovimiento("Consignación", monto, descripcion, saldoAnterior);
            }

            return true;
        }

        /// <summary>
        /// Implementación específica del método Retirar para Cuenta Corriente
        /// Permite retirar hasta el saldo + 20% de sobregiro
        /// </summary>
        public override bool Retirar(decimal monto, string descripcion = "Retiro")
        {
            if (monto <= 0)
                return false;

            // Calcular el límite máximo de retiro (saldo + sobregiro permitido)
            decimal sobregiroMaximo = Saldo * PORCENTAJE_SOBREGIRO;
            decimal limiteRetiro = Saldo + sobregiroMaximo - MontoSobregiro;

            if (monto > limiteRetiro)
                return false;

            decimal saldoAnterior = Saldo;

            if (monto <= Saldo)
            {
                // Retiro normal sin usar sobregiro
                Saldo -= monto;
                RegistrarMovimiento("Retiro", monto, descripcion, saldoAnterior);
            }
            else
            {
                // Retiro que requiere usar sobregiro
                decimal montoSobregiro = monto - Saldo;
                MontoSobregiro += montoSobregiro;
                Saldo = 0;
                
                RegistrarMovimiento("Retiro", monto, 
                    $"{descripcion} - Usando sobregiro: ${montoSobregiro:N2}", saldoAnterior);
            }

            return true;
        }

        /// <summary>
        /// Override del método para obtener información específica
        /// </summary>
        public override string ObtenerInformacionEspecifica()
        {
            decimal sobregiroDisponible = (Saldo * PORCENTAJE_SOBREGIRO) - MontoSobregiro;
            return $"Saldo: ${Saldo:N2} | Sobregiro usado: ${MontoSobregiro:N2} | Sobregiro disponible: ${sobregiroDisponible:N2}";
        }

        /// <summary>
        /// Override del método para obtener el tipo de cuenta
        /// </summary>
        public override string ObtenerTipoCuenta()
        {
            return "Cuenta Corriente";
        }

        /// <summary>
        /// Método para obtener el sobregiro máximo permitido
        /// </summary>
        public decimal ObtenerSobregiroMaximo()
        {
            return Saldo * PORCENTAJE_SOBREGIRO;
        }

        /// <summary>
        /// Método para obtener el sobregiro disponible
        /// </summary>
        public decimal ObtenerSobregiroDisponible()
        {
            return Math.Max(0, ObtenerSobregiroMaximo() - MontoSobregiro);
        }

        /// <summary>
        /// Método para verificar si la cuenta está en sobregiro
        /// </summary>
        public bool EstaEnSobregiro()
        {
            return MontoSobregiro > 0;
        }

        /// <summary>
        /// Override del método transferir para considerar sobregiro
        /// </summary>
        public override bool Transferir(Cuenta cuentaDestino, decimal monto, string descripcion = "Transferencia")
        {
            if (monto <= 0)
                return false;

            // Verificar si se puede realizar la transferencia considerando sobregiro
            decimal limiteTransferencia = Saldo + ObtenerSobregiroDisponible();
            
            if (monto > limiteTransferencia)
                return false;

            if (Retirar(monto, $"Transferencia enviada - {descripcion}"))
            {
                cuentaDestino.Consignar(monto, $"Transferencia recibida - {descripcion}");
                return true;
            }
            
            return false;
        }
    }
}