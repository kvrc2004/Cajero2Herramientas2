using System.ComponentModel.DataAnnotations;

namespace MiBanco.ViewModels
{
    /// <summary>
    /// ViewModel para el formulario de login
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave")]
        public string Clave { get; set; } = string.Empty;

        public string? MensajeError { get; set; }
        public int IntentosRestantes { get; set; } = 3;
    }

    /// <summary>
    /// ViewModel para el formulario de registro
    /// </summary>
    public class RegistroViewModel
    {
        [Required(ErrorMessage = "La identificación es obligatoria")]
        [StringLength(20, ErrorMessage = "La identificación no puede exceder 20 caracteres")]
        [Display(Name = "Identificación")]
        public string Identificacion { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio")]
        [StringLength(15, ErrorMessage = "El celular no puede exceder 15 caracteres")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "El celular debe tener entre 10 y 15 dígitos")]
        [Display(Name = "Celular")]
        public string Celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La clave debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave")]
        public string Clave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la clave")]
        [DataType(DataType.Password)]
        [Compare("Clave", ErrorMessage = "Las claves no coinciden")]
        [Display(Name = "Confirmar clave")]
        public string ConfirmarClave { get; set; } = string.Empty;

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }
    }

    /// <summary>
    /// ViewModel para operaciones bancarias
    /// </summary>
    public class OperacionViewModel
    {
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto")]
        public decimal Monto { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string Descripcion { get; set; } = string.Empty;

        [Display(Name = "Cuenta")]
        public int CuentaId { get; set; }

        [Display(Name = "Número de cuenta destino")]
        public string? CuentaDestino { get; set; }

        [Display(Name = "Número de cuotas")]
        [Range(1, 60, ErrorMessage = "El número de cuotas debe estar entre 1 y 60")]
        public int NumeroCuotas { get; set; } = 1;

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }
    }

    /// <summary>
    /// ViewModel para transferencias
    /// </summary>
    public class TransferenciaViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar una cuenta origen")]
        [Display(Name = "Cuenta origen")]
        public int CuentaOrigenId { get; set; }

        [Required(ErrorMessage = "El número de cuenta destino es obligatorio")]
        [Display(Name = "Número de cuenta destino")]
        public string CuentaDestino { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, 999999999, ErrorMessage = "El monto debe ser mayor a 0")]
        [Display(Name = "Monto a transferir")]
        public decimal Monto { get; set; }

        [Display(Name = "Descripción")]
        [StringLength(200, ErrorMessage = "La descripción no puede exceder 200 caracteres")]
        public string Descripcion { get; set; } = "Transferencia";

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }
        public string? InformacionCuentaDestino { get; set; }
    }

    /// <summary>
    /// ViewModel para editar perfil
    /// </summary>
    public class PerfilViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El celular es obligatorio")]
        [StringLength(15, ErrorMessage = "El celular no puede exceder 15 caracteres")]
        [RegularExpression(@"^\d{10,15}$", ErrorMessage = "El celular debe tener entre 10 y 15 dígitos")]
        [Display(Name = "Celular")]
        public string Celular { get; set; } = string.Empty;

        [Required(ErrorMessage = "El usuario es obligatorio")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Display(Name = "Identificación")]
        public string Identificacion { get; set; } = string.Empty; // Solo lectura

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }
    }

    /// <summary>
    /// ViewModel para cambio de clave
    /// </summary>
    public class CambioClaveViewModel
    {
        [Required(ErrorMessage = "El usuario es obligatorio")]
        [Display(Name = "Usuario")]
        public string Usuario { get; set; } = string.Empty;

        [Required(ErrorMessage = "La clave actual es obligatoria")]
        [DataType(DataType.Password)]
        [Display(Name = "Clave actual")]
        public string ClaveActual { get; set; } = string.Empty;

        [Required(ErrorMessage = "La nueva clave es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La nueva clave debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Nueva clave")]
        public string NuevaClave { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe confirmar la nueva clave")]
        [DataType(DataType.Password)]
        [Compare("NuevaClave", ErrorMessage = "Las claves no coinciden")]
        [Display(Name = "Confirmar nueva clave")]
        public string ConfirmarNuevaClave { get; set; } = string.Empty;

        public string? MensajeError { get; set; }
        public string? MensajeExito { get; set; }
    }
}