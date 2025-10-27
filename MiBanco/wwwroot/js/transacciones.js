/**
 * JavaScript para manejar las operaciones del cajero automático
 * Sistema bancario Mi Plata
 */

$(document).ready(function() {
    
    // ===== CONFIGURACIÓN INICIAL =====
    
    // Configurar CSRF token para todas las peticiones AJAX
    $.ajaxSetup({
        beforeSend: function(xhr, settings) {
            if (!/^(GET|HEAD|OPTIONS|TRACE)$/i.test(settings.type) && !this.crossDomain) {
                xhr.setRequestHeader("RequestVerificationToken", $('input[name=__RequestVerificationToken]').val());
            }
        }
    });
    
    // Función para formatear números como moneda
    function formatCurrency(amount) {
        return new Intl.NumberFormat('es-CO', {
            style: 'currency',
            currency: 'COP',
            minimumFractionDigits: 0,
            maximumFractionDigits: 2
        }).format(amount);
    }
    
    // Función para mostrar mensajes en modales
    function showModalMessage(containerId, message, type) {
        const container = $(containerId);
        container.removeClass('alert-success alert-danger alert-warning alert-info');
        container.addClass(`alert-${type}`);
        container.html(`<i class="fas fa-${type === 'success' ? 'check-circle' : type === 'danger' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>${message}`);
        container.show();
    }
    
    // ===== OPERACIÓN CONSIGNAR =====
    
    $('#formConsignar').on('submit', function(e) {
        e.preventDefault();
        
        const datos = {
            CuentaId: parseInt($('#cuentaConsignar').val()),
            Monto: parseFloat($('#montoConsignar').val()),
            Descripcion: $('#descripcionConsignar').val() || 'Consignación'
        };
        
        if (!datos.CuentaId || !datos.Monto || datos.Monto <= 0) {
            showModalMessage('#resultadoConsignar', 'Por favor completa todos los campos obligatorios.', 'danger');
            return;
        }
        
        $.ajax({
            url: '/Transacciones?handler=Consignar',
            type: 'POST',
            data: JSON.stringify(datos),
            contentType: 'application/json',
            beforeSend: function() {
                $('#formConsignar button[type="submit"]').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Procesando...');
            },
            success: function(response) {
                if (response.success) {
                    showModalMessage('#resultadoConsignar', response.message, 'success');
                    $('#formConsignar')[0].reset();
                    setTimeout(() => {
                        $('#consignarModal').modal('hide');
                        location.reload(); // Recargar para actualizar los saldos
                    }, 2000);
                } else {
                    showModalMessage('#resultadoConsignar', response.message, 'danger');
                }
            },
            error: function() {
                showModalMessage('#resultadoConsignar', 'Error de conexión. Intenta nuevamente.', 'danger');
            },
            complete: function() {
                $('#formConsignar button[type="submit"]').prop('disabled', false).html('<i class="fas fa-check me-2"></i>Consignar');
            }
        });
    });
    
    // ===== OPERACIÓN RETIRAR =====
    
    // Actualizar información disponible al cambiar cuenta
    $('#cuentaRetirar').on('change', function() {
        const option = $(this).find('option:selected');
        const saldo = parseFloat(option.data('saldo')) || 0;
        const tipo = option.data('tipo');
        
        let infoText = '';
        if (tipo === 'CuentaCorriente') {
            infoText = `Saldo disponible + sobregiro. Consulta el monto exacto en "Consultar Saldo"`;
        } else if (tipo === 'TarjetaCredito') {
            infoText = `Crédito disponible para avances en efectivo`;
        } else {
            infoText = `Saldo disponible: ${formatCurrency(saldo)}`;
        }
        
        $('#infoDisponible').text(infoText);
    });
    
    $('#formRetirar').on('submit', function(e) {
        e.preventDefault();
        
        const datos = {
            CuentaId: parseInt($('#cuentaRetirar').val()),
            Monto: parseFloat($('#montoRetirar').val()),
            Descripcion: $('#descripcionRetirar').val() || 'Retiro'
        };
        
        if (!datos.CuentaId || !datos.Monto || datos.Monto <= 0) {
            showModalMessage('#resultadoRetirar', 'Por favor completa todos los campos obligatorios.', 'danger');
            return;
        }
        
        $.ajax({
            url: '/Transacciones?handler=Retirar',
            type: 'POST',
            data: JSON.stringify(datos),
            contentType: 'application/json',
            beforeSend: function() {
                $('#formRetirar button[type="submit"]').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Procesando...');
            },
            success: function(response) {
                if (response.success) {
                    showModalMessage('#resultadoRetirar', response.message, 'success');
                    $('#formRetirar')[0].reset();
                    setTimeout(() => {
                        $('#retirarModal').modal('hide');
                        location.reload();
                    }, 2000);
                } else {
                    showModalMessage('#resultadoRetirar', response.message, 'danger');
                }
            },
            error: function() {
                showModalMessage('#resultadoRetirar', 'Error de conexión. Intenta nuevamente.', 'danger');
            },
            complete: function() {
                $('#formRetirar button[type="submit"]').prop('disabled', false).html('<i class="fas fa-check me-2"></i>Retirar');
            }
        });
    });
    
    // ===== OPERACIÓN TRANSFERIR =====
    
    // Buscar cuenta destino
    $('#btnBuscarCuenta').on('click', function() {
        const numeroCuenta = $('#cuentaDestino').val().trim();
        
        if (!numeroCuenta) {
            showModalMessage('#resultadoTransferir', 'Ingresa el número de cuenta destino.', 'warning');
            return;
        }
        
        $.ajax({
            url: `/Transacciones?handler=BuscarCuenta&numeroCuenta=${encodeURIComponent(numeroCuenta)}`,
            type: 'GET',
            beforeSend: function() {
                $('#btnBuscarCuenta').prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i>');
            },
            success: function(response) {
                if (response.success) {
                    let mensaje = `✓ ${response.tipoCuenta} - ${response.nombreTitular}`;
                    if (response.esPropia) {
                        mensaje += ' (Cuenta propia)';
                    }
                    $('#infoCuentaDestino').html(`<span class="text-success">${mensaje}</span>`);
                    $('#btnConfirmarTransfer').prop('disabled', false);
                } else {
                    $('#infoCuentaDestino').html(`<span class="text-danger">✗ ${response.message}</span>`);
                    $('#btnConfirmarTransfer').prop('disabled', true);
                }
            },
            error: function() {
                $('#infoCuentaDestino').html('<span class="text-danger">✗ Error al buscar la cuenta</span>');
                $('#btnConfirmarTransfer').prop('disabled', true);
            },
            complete: function() {
                $('#btnBuscarCuenta').prop('disabled', false).html('<i class="fas fa-search"></i>');
            }
        });
    });
    
    // Actualizar disponible para transferir
    $('#cuentaOrigen').on('change', function() {
        const saldo = parseFloat($(this).find('option:selected').data('saldo')) || 0;
        $('#infoDisponibleTransfer').text(`Saldo disponible: ${formatCurrency(saldo)}`);
    });
    
    $('#formTransferir').on('submit', function(e) {
        e.preventDefault();
        
        const datos = {
            CuentaOrigenId: parseInt($('#cuentaOrigen').val()),
            CuentaDestino: $('#cuentaDestino').val().trim(),
            Monto: parseFloat($('#montoTransferir').val()),
            Descripcion: $('#descripcionTransferir').val() || 'Transferencia'
        };
        
        if (!datos.CuentaOrigenId || !datos.CuentaDestino || !datos.Monto || datos.Monto <= 0) {
            showModalMessage('#resultadoTransferir', 'Por favor completa todos los campos obligatorios.', 'danger');
            return;
        }
        
        $.ajax({
            url: '/Transacciones?handler=Transferir',
            type: 'POST',
            data: JSON.stringify(datos),
            contentType: 'application/json',
            beforeSend: function() {
                $('#btnConfirmarTransfer').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Procesando...');
            },
            success: function(response) {
                if (response.success) {
                    showModalMessage('#resultadoTransferir', response.message, 'success');
                    $('#formTransferir')[0].reset();
                    $('#infoCuentaDestino').empty();
                    setTimeout(() => {
                        $('#transferirModal').modal('hide');
                        location.reload();
                    }, 2000);
                } else {
                    showModalMessage('#resultadoTransferir', response.message, 'danger');
                }
            },
            error: function() {
                showModalMessage('#resultadoTransferir', 'Error de conexión. Intenta nuevamente.', 'danger');
            },
            complete: function() {
                $('#btnConfirmarTransfer').prop('disabled', false).html('<i class="fas fa-check me-2"></i>Transferir');
            }
        });
    });
    
    // ===== OPERACIÓN COMPRAS EN CUOTAS =====
    
    // Actualizar crédito disponible
    $('#tarjetaCredito').on('change', function() {
        const disponible = parseFloat($(this).find('option:selected').data('disponible')) || 0;
        $('#infoDisponibleCompra').text(`Crédito disponible: ${formatCurrency(disponible)}`);
        actualizarSimulador();
    });
    
    // Actualizar simulador al cambiar monto o cuotas
    $('#montoCompra, #numeroCuotas').on('change input', actualizarSimulador);
    
    function actualizarSimulador() {
        const monto = parseFloat($('#montoCompra').val()) || 0;
        const cuotas = parseInt($('#numeroCuotas').val()) || 0;
        
        if (monto > 0 && cuotas > 0) {
            // Calcular tasa de interés
            let tasaInteres = 0;
            if (cuotas <= 2) {
                tasaInteres = 0;
            } else if (cuotas <= 6) {
                tasaInteres = 0.019; // 1.9%
            } else {
                tasaInteres = 0.023; // 2.3%
            }
            
            // Calcular monto total
            let montoTotal = monto;
            if (tasaInteres > 0) {
                montoTotal = monto * Math.pow(1 + tasaInteres, cuotas);
            }
            
            const pagoMensual = montoTotal / cuotas;
            const interesTotal = montoTotal - monto;
            
            let html = `
                <div class="row">
                    <div class="col-6">
                        <small class="text-muted">Monto compra:</small><br>
                        <strong>${formatCurrency(monto)}</strong>
                    </div>
                    <div class="col-6">
                        <small class="text-muted">Número cuotas:</small><br>
                        <strong>${cuotas}</strong>
                    </div>
                </div>
                <hr class="my-2">
                <div class="row">
                    <div class="col-6">
                        <small class="text-muted">Interés aplicado:</small><br>
                        <strong>${(tasaInteres * 100).toFixed(1)}% mensual</strong>
                    </div>
                    <div class="col-6">
                        <small class="text-muted">Total intereses:</small><br>
                        <strong class="text-warning">${formatCurrency(interesTotal)}</strong>
                    </div>
                </div>
                <hr class="my-2">
                <div class="row">
                    <div class="col-6">
                        <small class="text-muted">Total a pagar:</small><br>
                        <strong class="text-primary">${formatCurrency(montoTotal)}</strong>
                    </div>
                    <div class="col-6">
                        <small class="text-muted">Pago mensual:</small><br>
                        <strong class="text-success">${formatCurrency(pagoMensual)}</strong>
                    </div>
                </div>
            `;
            
            $('#simuladorCuotas').html(html);
        } else {
            $('#simuladorCuotas').html('<p class="text-muted">Ingresa el monto y selecciona las cuotas para ver la simulación.</p>');
        }
    }
    
    $('#formComprarCuotas').on('submit', function(e) {
        e.preventDefault();
        
        const datos = {
            CuentaId: parseInt($('#tarjetaCredito').val()),
            Monto: parseFloat($('#montoCompra').val()),
            NumeroCuotas: parseInt($('#numeroCuotas').val()),
            Descripcion: $('#descripcionCompra').val()
        };
        
        if (!datos.CuentaId || !datos.Monto || datos.Monto <= 0 || !datos.NumeroCuotas || !datos.Descripcion.trim()) {
            showModalMessage('#resultadoCompra', 'Por favor completa todos los campos obligatorios.', 'danger');
            return;
        }
        
        $.ajax({
            url: '/Transacciones?handler=ComprarEnCuotas',
            type: 'POST',
            data: JSON.stringify(datos),
            contentType: 'application/json',
            beforeSend: function() {
                $('#formComprarCuotas button[type="submit"]').prop('disabled', true).html('<i class="fas fa-spinner fa-spin me-2"></i>Procesando...');
            },
            success: function(response) {
                if (response.success) {
                    showModalMessage('#resultadoCompra', response.message, 'success');
                    $('#formComprarCuotas')[0].reset();
                    $('#simuladorCuotas').html('<p class="text-muted">Ingresa el monto y selecciona las cuotas para ver la simulación.</p>');
                    setTimeout(() => {
                        $('#comprasCuotasModal').modal('hide');
                        location.reload();
                    }, 3000);
                } else {
                    showModalMessage('#resultadoCompra', response.message, 'danger');
                }
            },
            error: function() {
                showModalMessage('#resultadoCompra', 'Error de conexión. Intenta nuevamente.', 'danger');
            },
            complete: function() {
                $('#formComprarCuotas button[type="submit"]').prop('disabled', false).html('<i class="fas fa-shopping-cart me-2"></i>Realizar Compra');
            }
        });
    });
    
    // ===== FILTRO DE MOVIMIENTOS =====
    
    $('#filtroMovimientos').on('change', function() {
        const cuentaId = $(this).val();
        
        if (cuentaId) {
            $('.movement-item').hide();
            $(`.movement-item[data-cuenta="${cuentaId}"]`).show();
        } else {
            $('.movement-item').show();
        }
    });
    
    // ===== LIMPIAR FORMULARIOS AL CERRAR MODALES =====
    
    $('.modal').on('hidden.bs.modal', function() {
        $(this).find('form')[0]?.reset();
        $(this).find('.alert').hide();
        $(this).find('.form-text').text('');
        $('#infoCuentaDestino').empty();
        $('#btnConfirmarTransfer').prop('disabled', true);
    });
    
});