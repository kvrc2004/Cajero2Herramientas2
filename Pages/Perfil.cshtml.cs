using Microsoft.AspNetCore.Mvc;
using MiBanco.Pages.Shared;
using MiBanco.Services;
using MiBanco.ViewModels;
using System.Linq;

namespace MiBanco.Pages
{
    /// <summary>
    /// Página de perfil del usuario
    /// Permite editar información personal y cambiar la clave
    /// </summary>
    public class PerfilModel : AuthPageModel
    {
        [BindProperty]
        public PerfilViewModel PerfilViewModel { get; set; } = new PerfilViewModel();

        [BindProperty]
        public CambioClaveViewModel CambioClaveViewModel { get; set; } = new CambioClaveViewModel();

        public PerfilModel(BancoService bancoService) : base(bancoService)
        {
        }

        public IActionResult OnGet()
        {
            Console.WriteLine("DEBUG PERFIL - Método OnGet iniciado");
            
            var verificacion = VerificarAutenticacion();
            if (verificacion != null) return verificacion;

            Console.WriteLine($"DEBUG PERFIL - Cliente logueado: {ClienteLogueado?.Usuario ?? "NULL"}");

            // Cargar datos actuales del cliente
            PerfilViewModel.Id = ClienteLogueado!.Id;
            PerfilViewModel.Identificacion = ClienteLogueado.Identificacion;
            PerfilViewModel.Nombre = ClienteLogueado.Nombre;
            PerfilViewModel.Celular = ClienteLogueado.Celular;
            PerfilViewModel.Usuario = ClienteLogueado.Usuario;

            // Precargar usuario para cambio de clave
            CambioClaveViewModel.Usuario = ClienteLogueado.Usuario;

            Console.WriteLine("DEBUG PERFIL - Datos cargados correctamente");
            return Page();
        }

        /// <summary>
        /// Maneja la actualización del perfil del usuario
        /// </summary>
        public IActionResult OnPostActualizarPerfil()
        {
            var verificacion = VerificarAutenticacion();
            if (verificacion != null) return verificacion;

            // Precargar usuario para cambio de clave (en caso de error y retorno)
            CambioClaveViewModel.Usuario = ClienteLogueado!.Usuario;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Crear cliente con datos actualizados
                var clienteActualizado = new Models.Cliente
                {
                    Id = ClienteLogueado.Id,
                    Identificacion = ClienteLogueado.Identificacion, // No se puede cambiar
                    Nombre = PerfilViewModel.Nombre.Trim(),
                    Celular = PerfilViewModel.Celular.Trim(),
                    Usuario = PerfilViewModel.Usuario.Trim(),
                    Clave = "" // No cambiar clave aquí
                };

                bool actualizado = _bancoService.ActualizarCliente(clienteActualizado);

                if (actualizado)
                {
                    // Actualizar sesión si cambió el nombre
                    if (ClienteLogueado.Nombre != clienteActualizado.Nombre)
                    {
                        HttpContext.Session.SetString("NombreCliente", clienteActualizado.Nombre);
                    }

                    PerfilViewModel.MensajeExito = "Información actualizada correctamente.";
                    
                    // Actualizar los datos en memoria
                    ClienteLogueado.Nombre = clienteActualizado.Nombre;
                    ClienteLogueado.Celular = clienteActualizado.Celular;
                    ClienteLogueado.Usuario = clienteActualizado.Usuario;
                }
                else
                {
                    PerfilViewModel.MensajeError = "No se pudo actualizar la información. El usuario podría estar en uso.";
                }
            }
            catch (Exception ex)
            {
                PerfilViewModel.MensajeError = "Ocurrió un error al actualizar la información. Intenta nuevamente.";
            }

            return Page();
        }

        /// <summary>
        /// Maneja el cambio de clave del usuario
        /// </summary>
        public IActionResult OnPostCambiarClave()
        {
            Console.WriteLine("DEBUG CAMBIO CLAVE - Método OnPostCambiarClave iniciado");
            
            var verificacion = VerificarAutenticacion();
            if (verificacion != null) 
            {
                Console.WriteLine("DEBUG CAMBIO CLAVE - Verificación de autenticación falló");
                return verificacion;
            }
            
            Console.WriteLine("DEBUG CAMBIO CLAVE - Usuario autenticado correctamente");

            // Cargar datos actuales del perfil (en caso de error y retorno)
            PerfilViewModel.Id = ClienteLogueado!.Id;
            PerfilViewModel.Identificacion = ClienteLogueado.Identificacion;
            PerfilViewModel.Nombre = ClienteLogueado.Nombre;
            PerfilViewModel.Celular = ClienteLogueado.Celular;
            PerfilViewModel.Usuario = ClienteLogueado.Usuario;

            Console.WriteLine($"DEBUG CAMBIO CLAVE - Datos del formulario:");
            Console.WriteLine($"  Usuario formulario: {CambioClaveViewModel?.Usuario ?? "NULL"}");
            Console.WriteLine($"  Clave actual formulario: {(string.IsNullOrEmpty(CambioClaveViewModel?.ClaveActual) ? "VACÍA" : "CON VALOR")}");
            Console.WriteLine($"  Nueva clave formulario: {(string.IsNullOrEmpty(CambioClaveViewModel?.NuevaClave) ? "VACÍA" : "CON VALOR")}");
            Console.WriteLine($"  Usuario logueado: {ClienteLogueado.Usuario}");
            Console.WriteLine($"  Clave actual usuario: {(string.IsNullOrEmpty(ClienteLogueado.Clave) ? "VACÍA" : "CON VALOR")}");

            // Limpiar errores de validación que no corresponden al CambioClaveViewModel
            var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("CambioClaveViewModel.")).ToList();
            foreach (var key in keysToRemove)
            {
                ModelState.Remove(key);
            }
            
            Console.WriteLine($"DEBUG CAMBIO CLAVE - ModelState después de limpiar: {ModelState.IsValid}");

            // Validar que el modelo sea válido
            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG CAMBIO CLAVE - ModelState no es válido:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  Campo: {error.Key}, Errores: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return Page();
            }
            
            Console.WriteLine("DEBUG CAMBIO CLAVE - ModelState es válido");

            try
            {
                Console.WriteLine("DEBUG CAMBIO CLAVE - Iniciando validaciones...");
                
                // Verificar que el usuario y clave actual sean correctos
                if (!ClienteLogueado.Usuario.Equals(CambioClaveViewModel?.Usuario, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("DEBUG CAMBIO CLAVE - Usuario no coincide");
                    CambioClaveViewModel!.MensajeError = "El usuario ingresado no coincide con tu usuario actual.";
                    return Page();
                }

                if (ClienteLogueado.Clave != CambioClaveViewModel?.ClaveActual)
                {
                    Console.WriteLine("DEBUG CAMBIO CLAVE - Clave actual incorrecta");
                    CambioClaveViewModel!.MensajeError = "La clave actual no es correcta.";
                    return Page();
                }

                Console.WriteLine("DEBUG CAMBIO CLAVE - Validaciones pasadas, actualizando cliente...");

                // Actualizar la clave
                var clienteActualizado = new Models.Cliente
                {
                    Id = ClienteLogueado.Id,
                    Identificacion = ClienteLogueado.Identificacion,
                    Nombre = ClienteLogueado.Nombre,
                    Celular = ClienteLogueado.Celular,
                    Usuario = ClienteLogueado.Usuario,
                    Clave = CambioClaveViewModel!.NuevaClave
                };

                Console.WriteLine($"DEBUG CAMBIO CLAVE - Llamando ActualizarCliente con nueva clave: {(string.IsNullOrEmpty(clienteActualizado.Clave) ? "VACÍA" : "CON VALOR")}");

                bool actualizado = _bancoService.ActualizarCliente(clienteActualizado);
                
                Console.WriteLine($"DEBUG CAMBIO CLAVE - Resultado ActualizarCliente: {actualizado}");

                if (actualizado)
                {
                    Console.WriteLine("DEBUG CAMBIO CLAVE - Verificando actualización...");
                    // IMPORTANTE: Verificar que se actualizó correctamente antes de limpiar
                    var clienteActualizadoDesdeServicio = _bancoService.ObtenerCliente(ClienteLogueado.Id);
                    if (clienteActualizadoDesdeServicio != null && clienteActualizadoDesdeServicio.Clave == CambioClaveViewModel!.NuevaClave)
                    {
                        Console.WriteLine("DEBUG CAMBIO CLAVE - Verificación exitosa, clave actualizada correctamente");
                        CambioClaveViewModel.MensajeExito = "Clave cambiada correctamente. La próxima vez que inicies sesión, usa tu nueva clave. ✓ Verificado.";
                        
                        // Limpiar formulario después del éxito
                        CambioClaveViewModel.ClaveActual = "";
                        CambioClaveViewModel.NuevaClave = "";
                        CambioClaveViewModel.ConfirmarNuevaClave = "";
                    }
                    else
                    {
                        Console.WriteLine("DEBUG CAMBIO CLAVE - Error: Verificación falló, clave no se actualizó");
                        CambioClaveViewModel!.MensajeError = "Error: La clave no se actualizó correctamente en el sistema.";
                    }
                }
                else
                {
                    Console.WriteLine("DEBUG CAMBIO CLAVE - ActualizarCliente retornó false");
                    CambioClaveViewModel!.MensajeError = "No se pudo cambiar la clave. Intenta nuevamente.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DEBUG CAMBIO CLAVE - Excepción: {ex.Message}");
                CambioClaveViewModel!.MensajeError = "Ocurrió un error al cambiar la clave. Intenta nuevamente.";
            }

            Console.WriteLine("DEBUG CAMBIO CLAVE - Método terminado, retornando página");
            return Page();
        }
    }
}