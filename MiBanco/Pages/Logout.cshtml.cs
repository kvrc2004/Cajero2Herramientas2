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
            // Limpiar toda la sesión
            HttpContext.Session.Clear();
            
            TempData["Info"] = "Has cerrado sesión correctamente. ¡Hasta pronto!";
            
            return RedirectToPage("/Index");
        }
    }
}