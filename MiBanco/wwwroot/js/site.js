// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

/**
 * Función para confirmar el cierre de sesión
 */
function confirmarCerrarSesion() {
    return confirm('¿Estás seguro de que deseas cerrar sesión?');
}

/**
 * Función para mostrar notificaciones toast
 */
function mostrarToast(mensaje, tipo = 'info') {
    // Crear elemento toast si no existe
    let toastContainer = document.getElementById('toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toast-container';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '1055';
        document.body.appendChild(toastContainer);
    }

    // Crear toast
    const toastId = 'toast-' + Date.now();
    const toastHtml = `
        <div id="${toastId}" class="toast" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="toast-header">
                <i class="fas fa-${tipo === 'success' ? 'check-circle text-success' : tipo === 'error' ? 'exclamation-triangle text-danger' : 'info-circle text-primary'} me-2"></i>
                <strong class="me-auto">Mi Plata</strong>
                <small>ahora</small>
                <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
            </div>
            <div class="toast-body">
                ${mensaje}
            </div>
        </div>
    `;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    // Mostrar toast
    const toastElement = document.getElementById(toastId);
    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    // Eliminar toast después de que se oculte
    toastElement.addEventListener('hidden.bs.toast', () => {
        toastElement.remove();
    });
}

// Inicialización cuando el DOM está listo
document.addEventListener('DOMContentLoaded', function() {
    // Inicializar tooltips de Bootstrap si existen
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Manejar alertas automáticas que se ocultan después de un tiempo
    const alertas = document.querySelectorAll('.alert:not(.alert-permanent)');
    alertas.forEach(function(alerta) {
        setTimeout(function() {
            const bsAlert = new bootstrap.Alert(alerta);
            bsAlert.close();
        }, 5000); // Ocultar después de 5 segundos
    });
});
