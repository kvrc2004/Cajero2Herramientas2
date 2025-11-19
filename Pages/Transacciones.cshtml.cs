using Microsoft.AspNetCore.Mvc;
using MiBanco.Pages.Shared;
using MiBanco.Services;
using MiBanco.ViewModels;
using MiBanco.Data;
using System.Text.Json;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página principal de transacciones bancarias
    /// Permite realizar todas las operaciones del cajero automático
    /// </summary>
    [IgnoreAntiforgeryToken]
    public class TransaccionesModel : AuthPageModel
    {
        private readonly MiBancoDbContext _context;

        public TransaccionesModel(BancoService bancoService, MiBancoDbContext context) : base(bancoService)
        {
            _context = context;
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
        public async Task<IActionResult> OnPostConsignar([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                bool resultado = await _bancoService.Consignar(operacion.CuentaId, operacion.Monto, operacion.Descripcion);
                
                if (resultado)
                {
                    var cuenta = await _bancoService.ObtenerCuenta(operacion.CuentaId);
                    return new JsonResult(new { 
                        success = true, 
                        message = $"Consignación exitosa. Nuevo saldo: ${cuenta?.Saldo:N2}",
                        nuevoSaldo = cuenta?.Saldo ?? 0
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
        public async Task<IActionResult> OnPostRetirar([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                bool resultado = await _bancoService.Retirar(operacion.CuentaId, operacion.Monto, operacion.Descripcion);
                
                if (resultado)
                {
                    var cuenta = await _bancoService.ObtenerCuenta(operacion.CuentaId);
                    return new JsonResult(new { 
                        success = true, 
                        message = $"Retiro exitoso. Nuevo saldo: ${cuenta?.Saldo:N2}",
                        nuevoSaldo = cuenta?.Saldo ?? 0
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
        public async Task<IActionResult> OnPostTransferir([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Sesión no válida" });
                }

                // Obtener cuenta destino desde la descripción temporalmente
                // En el frontend, vamos a enviar la información de manera diferente
                int cuentaOrigenId = operacion.CuentaId;
                string cuentaDestino = operacion.CuentaDestino ?? "";
                decimal monto = operacion.Monto;
                string descripcion = operacion.Descripcion ?? "Transferencia";

                // Validaciones básicas
                if (cuentaOrigenId <= 0 || string.IsNullOrEmpty(cuentaDestino) || monto <= 0)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos para la transferencia" });
                }

                bool resultado = await _bancoService.RealizarTransferencia(
                    cuentaOrigenId, 
                    cuentaDestino, 
                    monto, 
                    descripcion);

                if (resultado)
                {
                    var cuentaOrigen = await _bancoService.ObtenerCuenta(cuentaOrigenId);
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
                return new JsonResult(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Operación AJAX para realizar compras en cuotas con tarjeta de crédito
        /// </summary>
        public async Task<IActionResult> OnPostComprarEnCuotas([FromBody] OperacionViewModel operacion)
        {
            try
            {
                if (!ModelState.IsValid || ClienteLogueado == null)
                {
                    return new JsonResult(new { success = false, message = "Datos inválidos" });
                }

                var cuenta = await _bancoService.ObtenerCuenta(operacion.CuentaId);
                if (cuenta is not Models.TarjetaCredito tarjeta)
                {
                    return new JsonResult(new { success = false, message = "La cuenta seleccionada no es una tarjeta de crédito" });
                }

                bool resultado = await _bancoService.ComprarEnCuotas(operacion.CuentaId, operacion.Monto, operacion.NumeroCuotas, operacion.Descripcion);
                
                if (resultado)
                {
                    // Recalcular después de guardar en la BD
                    tarjeta = await _bancoService.ObtenerCuenta(operacion.CuentaId) as Models.TarjetaCredito;
                    decimal pagoMensual = tarjeta!.CalcularPagoMensual(operacion.Monto, operacion.NumeroCuotas);
                    
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
        public async Task<IActionResult> OnGetBuscarCuenta(string numeroCuenta)
        {
            try
            {
                if (string.IsNullOrEmpty(numeroCuenta))
                {
                    return new JsonResult(new { success = false, message = "Número de cuenta requerido" });
                }

                var cuenta = await _bancoService.ObtenerCuentaPorNumero(numeroCuenta);
                if (cuenta == null)
                {
                    return new JsonResult(new { success = false, message = "Cuenta no encontrada" });
                }

                var cliente = await _bancoService.ObtenerCliente(cuenta.ClienteId);
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