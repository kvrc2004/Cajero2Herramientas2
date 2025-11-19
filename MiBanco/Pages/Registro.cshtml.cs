using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MiBanco.Models;
using MiBanco.Services;
using MiBanco.ViewModels;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página de registro de nuevos clientes del banco
    /// Permite crear una cuenta nueva con validaciones completas
    /// </summary>
    public class RegistroModel : PageModel
    {
        private readonly BancoService _bancoService;

        [BindProperty]
        public RegistroViewModel RegistroViewModel { get; set; } = new RegistroViewModel();

        public RegistroModel(BancoService bancoService)
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

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Crear el nuevo cliente
                var nuevoCliente = new Cliente
                {
                    Identificacion = RegistroViewModel.Identificacion.Trim(),
                    Nombre = RegistroViewModel.Nombre.Trim(),
                    Celular = RegistroViewModel.Celular.Trim(),
                    Usuario = RegistroViewModel.Usuario.Trim(),
                    Clave = RegistroViewModel.Clave
                };

                // Intentar registrar el cliente
                bool registroExitoso = _bancoService.RegistrarCliente(nuevoCliente);

                if (registroExitoso)
                {
                    RegistroViewModel.MensajeExito = $"¡Registro exitoso! Tu cuenta ha sido creada correctamente, {nuevoCliente.Nombre}. " +
                                                   "Ya puedes iniciar sesión y comenzar a usar nuestros servicios.";
                    
                    // Limpiar el formulario
                    RegistroViewModel = new RegistroViewModel { MensajeExito = RegistroViewModel.MensajeExito };

                    // Opcionalmente, redirigir automáticamente después de unos segundos
                    TempData["Info"] = "Cuenta creada exitosamente. Ahora puedes iniciar sesión.";
                }
                else
                {
                    RegistroViewModel.MensajeError = "No se pudo completar el registro. " +
                                                   "Verifica que el usuario o la identificación no estén ya registrados.";
                }
            }
            catch (Exception ex)
            {
                RegistroViewModel.MensajeError = "Ocurrió un error durante el registro. Por favor, intenta nuevamente.";
                // En producción, aquí se debería loggear el error
            }

            return Page();
        }
    }
}