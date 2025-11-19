using Microsoft.AspNetCore.Mvc.RazorPages;
using MiBanco.Services;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página principal del sistema bancario Mi Plata
    /// Muestra información de la empresa y servicios disponibles
    /// </summary>
    public class IndexModel : PageModel
    {
        private readonly BancoService _bancoService;

        public IndexModel(BancoService bancoService)
        {
            _bancoService = bancoService;
        }

        /// <summary>
        /// Indica si hay un usuario autenticado
        /// </summary>
        public bool EstaAutenticado => HttpContext.Session.GetInt32("ClienteId").HasValue;

        /// <summary>
        /// Nombre del cliente logueado (si está autenticado)
        /// </summary>
        public string NombreCliente => HttpContext.Session.GetString("NombreCliente") ?? "";

        public void OnGet()
        {
            // La página de inicio no requiere lógica especial
            // Solo muestra la información corporativa y los servicios
        }
    }
}
