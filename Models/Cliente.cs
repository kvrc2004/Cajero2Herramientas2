using System.ComponentModel.DataAnnotations;

namespace MiBanco.Models
{
    /// <summary>
    /// Clase que representa un cliente del banco "Mi Plata"
    /// Contiene la información personal y las credenciales de acceso
    /// </summary>
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "La identificación es obligatoria")]
        [StringLength(20, ErrorMessage = "La identificación no puede exceder 20 caracteres")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio")]
        [StringLength(15, ErrorMessage = "El celular no puede exceder 15 caracteres")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "El celular debe tener entre 10 y 15 dígitos")]
        public string Celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener al menos 6 caracteres")]
        public string Clave { get; set; } = string.Empty;

        // Control de seguridad para bloqueo de cuenta
        public int IntentosLogin { get; set; } = 0;
        public bool CuentaBloqueada { get; set; } = false;

        // Fecha de registro
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        // Lista de cuentas bancarias del cliente
        public List<Cuenta> Cuentas { get; set; } = new List<Cuenta>();

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public Cliente() { }

        /// <summary>
        /// Constructor con parámetros
        /// </summary>
        public Cliente(string identificacion, string nombre, string celular, string usuario, string clave)
        {
            Identificacion = identificacion;
            Nombre = nombre;
            Celular = celular;
            Usuario = usuario;
            Clave = clave;
        }

        /// <summary>
        /// Método para verificar las credenciales del cliente
        /// </summary>
        public bool VerificarCredenciales(string usuario, string clave)
        {
            return Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase) && Clave == clave;
        }

        /// <summary>
        /// Método para incrementar intentos de login fallidos
        /// </summary>
        public void IncrementarIntentosLogin()
        {
            IntentosLogin++;
            if (IntentosLogin >= 3)
            {
                CuentaBloqueada = true;
            }
        }

        /// <summary>
        /// Método para reiniciar intentos de login (cuando el login es exitoso)
        /// </summary>
        public void ReiniciarIntentosLogin()
        {
            IntentosLogin = 0;
            CuentaBloqueada = false;
        }

        /// <summary>
        /// Obtener cuenta por tipo
        /// </summary>
        public Cuenta? ObtenerCuentaPorTipo(string tipo)
        {
            return Cuentas.FirstOrDefault(c => c.GetType().Name.ToLower().Contains(tipo.ToLower()));
        }
    }
}