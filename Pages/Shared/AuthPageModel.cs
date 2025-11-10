using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiBanco.Extensions;
using MiBanco.Models;
using MiBanco.Services;

namespace MiBanco.Pages.Shared
{
    /// <summary>
    /// Clase base para páginas que requieren autenticación
    /// Proporciona funcionalidades comunes para el manejo de sesión y cliente logueado
    /// </summary>
    public abstract class AuthPageModel : PageModel
    {
        protected readonly BancoService _bancoService;
        private Cliente? _clienteLogueado;

        protected AuthPageModel(BancoService bancoService)
        {
            _bancoService = bancoService;
        }

        /// <summary>
        /// Obtiene el cliente logueado desde la sesión
        /// </summary>
        public Cliente? ClienteLogueado
        {
            get
            {
                if (_clienteLogueado == null)
                {
                    var clienteId = HttpContext.Session.GetInt32("ClienteId");
                    if (clienteId.HasValue)
                    {
                        _clienteLogueado = _bancoService.ObtenerCliente(clienteId.Value).GetAwaiter().GetResult();
                    }
                }
                return _clienteLogueado;
            }
        }

        /// <summary>
        /// Verifica si hay un usuario autenticado
        /// </summary>
        protected bool EstaAutenticado => ClienteLogueado != null;

        /// <summary>
        /// Redirige al login si no está autenticado
        /// </summary>
        protected IActionResult? VerificarAutenticacion()
        {
            if (!EstaAutenticado)
            {
                return RedirectToPage("/Login");
            }
            return null;
        }

        /// <summary>
        /// Establece la sesión del cliente
        /// </summary>
        protected void EstablecerSesionCliente(Cliente cliente)
        {
            HttpContext.Session.SetInt32("ClienteId", cliente.Id);
            HttpContext.Session.SetString("NombreCliente", cliente.Nombre);
            _clienteLogueado = cliente;
        }

        /// <summary>
        /// Cierra la sesión del cliente
        /// </summary>
        protected void CerrarSesion()
        {
            HttpContext.Session.Clear();
            _clienteLogueado = null;
        }

        /// <summary>
        /// Obtiene una cuenta específica del cliente logueado
        /// </summary>
        protected Cuenta? ObtenerCuentaCliente(int cuentaId)
        {
            return ClienteLogueado?.Cuentas.FirstOrDefault(c => c.Id == cuentaId);
        }

        /// <summary>
        /// Muestra un mensaje de error en TempData
        /// </summary>
        protected void MostrarError(string mensaje)
        {
            TempData["Error"] = mensaje;
        }

        /// <summary>
        /// Muestra un mensaje de éxito en TempData
        /// </summary>
        protected void MostrarExito(string mensaje)
        {
            TempData["Exito"] = mensaje;
        }

        /// <summary>
        /// Muestra un mensaje de información en TempData
        /// </summary>
        protected void MostrarInfo(string mensaje)
        {
            TempData["Info"] = mensaje;
        }
    }
}