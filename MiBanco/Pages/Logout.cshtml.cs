using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página para cerrar sesión del usuario
    /// </summary>
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Verificar si había una sesión activa
            bool habiaSession = HttpContext.Session.GetInt32("ClienteId").HasValue;
            string nombreCliente = HttpContext.Session.GetString("NombreCliente") ?? "Usuario";
            
            // Limpiar toda la sesión
            HttpContext.Session.Clear();
            
            if (habiaSession)
            {
                TempData["Info"] = $"¡Hasta pronto, {nombreCliente}! Has cerrado sesión correctamente.";
            }
            else
            {
                TempData["Info"] = "Sesión cerrada.";
            }
            
            return RedirectToPage("/Index");
        }
        
        public IActionResult OnPost()
        {
            // También manejar POST requests si alguien envía un formulario
            return OnGet();
        }
    }
}