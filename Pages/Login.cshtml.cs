using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiBanco.Services;
using MiBanco.ViewModels;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página de inicio de sesión del sistema bancario
    /// Implementa el sistema de autenticación con límite de intentos
    /// </summary>
    public class LoginModel : PageModel
    {
        private readonly BancoService _bancoService;

        [BindProperty]
        public LoginViewModel LoginViewModel { get; set; } = new LoginViewModel();

        public LoginModel(BancoService bancoService)
        {
            _bancoService = bancoService;
        }

        public IActionResult OnGet()
        {
            // Si ya está autenticado, redirigir a transacciones
            if (HttpContext.Session.GetInt32("ClienteId").HasValue)
            {
                return RedirectToPage("/Transacciones");
            }

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Intentar autenticar al cliente
                var cliente = await _bancoService.AutenticarCliente(LoginViewModel.Usuario, LoginViewModel.Clave);

                if (cliente != null)
                {
                    // Login exitoso - establecer sesión
                    HttpContext.Session.SetInt32("ClienteId", cliente.Id);
                    HttpContext.Session.SetString("NombreCliente", cliente.Nombre);
                    
                    TempData["Exito"] = $"¡Bienvenido, {cliente.Nombre}!";
                    return RedirectToPage("/Transacciones");
                }
                else
                {
                    // Login fallido - verificar si la cuenta está bloqueada
                    var todosClientes = await _bancoService.ObtenerTodosLosClientes();
                    var clienteExistente = todosClientes
                        .FirstOrDefault(c => c.Usuario.Equals(LoginViewModel.Usuario, StringComparison.OrdinalIgnoreCase));

                    if (clienteExistente != null)
                    {
                        if (clienteExistente.CuentaBloqueada)
                        {
                            LoginViewModel.MensajeError = "Tu cuenta ha sido bloqueada por exceso de intentos fallidos. Contacta al servicio al cliente.";
                            LoginViewModel.IntentosRestantes = 0;
                        }
                        else
                        {
                            LoginViewModel.IntentosRestantes = 3 - clienteExistente.IntentosLogin;
                            
                            if (LoginViewModel.IntentosRestantes > 0)
                            {
                                LoginViewModel.MensajeError = $"Usuario o clave incorrectos. Te quedan {LoginViewModel.IntentosRestantes} intento(s).";
                            }
                            else
                            {
                                LoginViewModel.MensajeError = "Cuenta bloqueada por exceso de intentos fallidos.";
                            }
                        }
                    }
                    else
                    {
                        LoginViewModel.MensajeError = "Usuario no encontrado. Verifica tus credenciales.";
                        LoginViewModel.IntentosRestantes = 3;
                    }
                }
            }
            catch (Exception ex)
            {
                LoginViewModel.MensajeError = "Ocurrió un error durante el inicio de sesión. Intenta nuevamente.";
                // En producción, aquí se debería loggear el error
            }

            return Page();
        }
    }
}