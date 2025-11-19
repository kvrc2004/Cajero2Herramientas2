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
        public async Task<IActionResult> OnPostActualizarPerfil()
        {
            Console.WriteLine("DEBUG PERFIL - Método OnPostActualizarPerfil iniciado");
            
            var verificacion = VerificarAutenticacion();
            if (verificacion != null)
            {
                Console.WriteLine("DEBUG PERFIL - Verificación de autenticación falló");
                return verificacion;
            }

            Console.WriteLine($"DEBUG PERFIL - ClienteLogueado: {ClienteLogueado?.Usuario}");
            Console.WriteLine($"DEBUG PERFIL - Datos del formulario: Nombre={PerfilViewModel.Nombre}, Celular={PerfilViewModel.Celular}, Usuario={PerfilViewModel.Usuario}");

            // Precargar usuario para cambio de clave (en caso de error y retorno)
            CambioClaveViewModel.Usuario = ClienteLogueado!.Usuario;

            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG PERFIL - ModelState no es válido");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  Campo: {error.Key}, Errores: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                
                // Limpiar errores que no corresponden al PerfilViewModel
                var keysToRemove = ModelState.Keys.Where(k => !k.StartsWith("PerfilViewModel.")).ToList();
                foreach (var key in keysToRemove)
                {
                    ModelState.Remove(key);
                }
                
                // Verificar nuevamente después de limpiar
                if (!ModelState.IsValid)
                {
                    Console.WriteLine("DEBUG PERFIL - ModelState sigue inválido después de limpiar");
                    return Page();
                }
                
                Console.WriteLine("DEBUG PERFIL - ModelState válido después de limpiar");
            }

            try
            {
                Console.WriteLine("DEBUG PERFIL - Creando cliente actualizado");
                
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

                Console.WriteLine($"DEBUG PERFIL - Llamando ActualizarCliente. ID={clienteActualizado.Id}, Nombre={clienteActualizado.Nombre}");
                bool actualizado = await _bancoService.ActualizarCliente(clienteActualizado);
                Console.WriteLine($"DEBUG PERFIL - Resultado ActualizarCliente: {actualizado}");

                if (actualizado)
                {
                    // Recargar el cliente desde la base de datos para asegurar que tenemos los datos actualizados
                    var clienteActualizadoDB = await _bancoService.ObtenerCliente(ClienteLogueado.Id);
                    if (clienteActualizadoDB != null)
                    {
                        // Actualizar sesión si cambió el nombre
                        if (clienteActualizadoDB.Nombre != ClienteLogueado.Nombre)
                        {
                            HttpContext.Session.SetString("NombreCliente", clienteActualizadoDB.Nombre);
                        }

                        PerfilViewModel.MensajeExito = "Información actualizada correctamente.";
                        
                        // Actualizar los datos en memoria con los datos de la base de datos
                        ClienteLogueado.Nombre = clienteActualizadoDB.Nombre;
                        ClienteLogueado.Celular = clienteActualizadoDB.Celular;
                        ClienteLogueado.Usuario = clienteActualizadoDB.Usuario;
                    }
                    else
                    {
                        PerfilViewModel.MensajeError = "Error al verificar la actualización.";
                    }
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
        public async Task<IActionResult> OnPostCambiarClave()
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
                
                // Obtener el cliente actualizado desde la base de datos
                var clienteActualDB = await _bancoService.ObtenerCliente(ClienteLogueado.Id);
                if (clienteActualDB == null)
                {
                    CambioClaveViewModel!.MensajeError = "Error al obtener los datos del cliente.";
                    return Page();
                }
                
                // Verificar que el usuario y clave actual sean correctos
                if (!clienteActualDB.Usuario.Equals(CambioClaveViewModel?.Usuario, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("DEBUG CAMBIO CLAVE - Usuario no coincide");
                    CambioClaveViewModel!.MensajeError = "El usuario ingresado no coincide con tu usuario actual.";
                    return Page();
                }

                // Validar que la clave actual sea correcta
                if (clienteActualDB.Clave != CambioClaveViewModel?.ClaveActual)
                {
                    Console.WriteLine($"DEBUG CAMBIO CLAVE - Clave actual incorrecta. Esperada: '{clienteActualDB.Clave}', Recibida: '{CambioClaveViewModel?.ClaveActual}'");
                    CambioClaveViewModel!.MensajeError = "⚠️ La clave actual no es correcta. Por favor, verifica e intenta nuevamente.";
                    
                    // Limpiar los campos de clave
                    CambioClaveViewModel.ClaveActual = "";
                    CambioClaveViewModel.NuevaClave = "";
                    CambioClaveViewModel.ConfirmarNuevaClave = "";
                    
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

                bool actualizado = await _bancoService.ActualizarCliente(clienteActualizado);
                
                Console.WriteLine($"DEBUG CAMBIO CLAVE - Resultado ActualizarCliente: {actualizado}");

                if (actualizado)
                {
                    Console.WriteLine("DEBUG CAMBIO CLAVE - Verificando actualización...");
                    // IMPORTANTE: Verificar que se actualizó correctamente antes de limpiar
                    var clienteActualizadoDesdeServicio = await _bancoService.ObtenerCliente(ClienteLogueado.Id);
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