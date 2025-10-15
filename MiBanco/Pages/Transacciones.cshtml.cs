using Microsoft.AspNetCore.Mvc;
using MiBanco.Pages.Shared;
using MiBanco.Services;
using MiBanco.ViewModels;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página principal de transacciones bancarias
    /// Permite realizar todas las operaciones del cajero automático
    /// </summary>
    public class TransaccionesModel : AuthPageModel
    {
        public TransaccionesModel(BancoService bancoService) : base(bancoService)
        {
        }

        public IActionResult OnGet()
        {
            var verificacion = VerificarAutenticacion();
            if (verificacion != null) return verificacion;

            return Page();
        }

        #region Operaciones AJAX

        /// <summary>
        /// Operación AJAX para consignar dinero
        /// </summary>
        public IActionResult OnPostConsignar([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                var cuenta = ObtenerCuentaCliente(operacion.CuentaId);
                if (cuenta == null)
                {
                    return new JsonResult(new { success = false, message = "Cuenta no encontrada" });
                }

                bool resultado = cuenta.Consignar(operacion.Monto, operacion.Descripcion);
                
                if (resultado)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = $"Consignación exitosa. Nuevo saldo: ${cuenta.Saldo:N2}",
                        nuevoSaldo = cuenta.Saldo
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Error al procesar la consignación" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Operación AJAX para retirar dinero
        /// </summary>
        public IActionResult OnPostRetirar([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                var cuenta = ObtenerCuentaCliente(operacion.CuentaId);
                if (cuenta == null)
                {
                    return new JsonResult(new { success = false, message = "Cuenta no encontrada" });
                }

                bool resultado = cuenta.Retirar(operacion.Monto, operacion.Descripcion);
                
                if (resultado)
                {
                    return new JsonResult(new { 
                        success = true, 
                        message = $"Retiro exitoso. Nuevo saldo: ${cuenta.Saldo:N2}",
                        nuevoSaldo = cuenta.Saldo
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Fondos insuficientes para el retiro" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Operación AJAX para transferir dinero
        /// </summary>
        public IActionResult OnPostTransferir([FromBody] TransferenciaViewModel transferencia)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                bool resultado = _bancoService.RealizarTransferencia(
                    transferencia.CuentaOrigenId, 
                    transferencia.CuentaDestino, 
                    transferencia.Monto, 
                    transferencia.Descripcion);

                if (resultado)
                {
                    var cuentaOrigen = ObtenerCuentaCliente(transferencia.CuentaOrigenId);
                    return new JsonResult(new { 
                        success = true, 
                        message = $"Transferencia exitosa. Nuevo saldo: ${cuentaOrigen?.Saldo:N2}",
                        nuevoSaldo = cuentaOrigen?.Saldo ?? 0
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Error al procesar la transferencia. Verifique los datos." });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Operación AJAX para realizar compras en cuotas con tarjeta de crédito
        /// </summary>
        public IActionResult OnPostComprarEnCuotas([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                var cuenta = ObtenerCuentaCliente(operacion.CuentaId);
                if (cuenta is not Models.TarjetaCredito tarjeta)
                {
                    return new JsonResult(new { success = false, message = "La cuenta seleccionada no es una tarjeta de crédito" });
                }

                bool resultado = tarjeta.RealizarCompraEnCuotas(operacion.Monto, operacion.NumeroCuotas, operacion.Descripcion);
                
                if (resultado)
                {
                    decimal pagoMensual = tarjeta.CalcularPagoMensual(operacion.Monto, operacion.NumeroCuotas);
                    return new JsonResult(new { 
                        success = true, 
                        message = $"Compra exitosa en {operacion.NumeroCuotas} cuotas. Pago mensual: ${pagoMensual:N2}",
                        pagoMensual = pagoMensual,
                        creditoDisponible = tarjeta.CreditoDisponible
                    });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "Crédito insuficiente para la compra" });
                }
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Operación AJAX para buscar cuenta destino en transferencias
        /// </summary>
        public IActionResult OnGetBuscarCuenta(string numeroCuenta)
        {
            try
            {
                if (string.IsNullOrEmpty(numeroCuenta))
                {
                    return new JsonResult(new { success = false, message = "Número de cuenta requerido" });
                }

                var cuenta = _bancoService.ObtenerCuentaPorNumero(numeroCuenta);
                if (cuenta == null)
                {
                    return new JsonResult(new { success = false, message = "Cuenta no encontrada" });
                }

                var cliente = _bancoService.ObtenerCliente(cuenta.ClienteId);
                return new JsonResult(new { 
                    success = true, 
                    tipoCuenta = cuenta.ObtenerTipoCuenta(),
                    nombreTitular = cliente?.Nombre ?? "Titular no encontrado",
                    esPropia = cuenta.ClienteId == ClienteLogueado?.Id
                });
            }
            catch (Exception ex)
            {
                return new JsonResult(new { success = false, message = "Error al buscar la cuenta" });
            }
        }

        #endregion
    }
}