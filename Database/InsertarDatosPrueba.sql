-- =============================================
-- DATOS DE PRUEBA ADICIONALES - MiBanco
-- Base de Datos: MiPlataDB
-- Descripcion: Script para insertar clientes de prueba con cuentas y movimientos
-- Fecha: 18 de Noviembre de 2025
-- =============================================

USE MiPlataDB;
GO

PRINT 'Iniciando insercion de datos de prueba...';
GO

-- =============================================
-- CLIENTE 1: Maria Garcia - Cliente Premium
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'maria.garcia')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1001234567', 'Maria Garcia Lopez', '3101234567', 'maria.garcia', 'maria123', 0, 0, DATEADD(MONTH, -18, GETDATE()));
    
    DECLARE @ClienteId1 INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros con buen saldo
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2001234567', 5800000, @ClienteId1, DATEADD(MONTH, -18, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta1A INT = SCOPE_IDENTITY();
    
    -- Cuenta Corriente
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, MontoSobregiro)
    VALUES ('3001234567', 1200000, @ClienteId1, DATEADD(MONTH, -15, GETDATE()), 'CuentaCorriente', 500000);
    
    DECLARE @Cuenta1C INT = SCOPE_IDENTITY();
    
    -- Tarjeta de Credito
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
    VALUES ('4501234567', -850000, @ClienteId1, DATEADD(MONTH, -12, GETDATE()), 'TarjetaCredito', 3000000);
    
    DECLARE @Cuenta1T INT = SCOPE_IDENTITY();
    
    -- Movimientos variados
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta1A, 'Consignacion', 2000000, 'Deposito inicial', 0, 2000000, DATEADD(MONTH, -18, GETDATE())),
        (@Cuenta1A, 'Consignacion', 1500000, 'Nomina', 2000000, 3500000, DATEADD(DAY, -45, GETDATE())),
        (@Cuenta1A, 'Retiro', 500000, 'Retiro cajero', 3500000, 3000000, DATEADD(DAY, -30, GETDATE())),
        (@Cuenta1A, 'Consignacion', 3000000, 'Bonificacion anual', 3000000, 6000000, DATEADD(DAY, -15, GETDATE())),
        (@Cuenta1A, 'Retiro', 200000, 'Pago servicios', 6000000, 5800000, DATEADD(DAY, -3, GETDATE())),
        
        (@Cuenta1C, 'Consignacion', 1000000, 'Apertura cuenta', 0, 1000000, DATEADD(MONTH, -15, GETDATE())),
        (@Cuenta1C, 'Consignacion', 800000, 'Transferencia', 1000000, 1800000, DATEADD(DAY, -60, GETDATE())),
        (@Cuenta1C, 'Retiro', 600000, 'Pago proveedores', 1800000, 1200000, DATEADD(DAY, -20, GETDATE())),
        
        (@Cuenta1T, 'Compra en cuotas', 450000, 'Electrodom??sticos - 6 cuotas', 0, -450000, DATEADD(DAY, -90, GETDATE())),
        (@Cuenta1T, 'Compra', 280000, 'Supermercado', -450000, -730000, DATEADD(DAY, -40, GETDATE())),
        (@Cuenta1T, 'Compra', 120000, 'Restaurante', -730000, -850000, DATEADD(DAY, -10, GETDATE()));
    
    PRINT 'Cliente Maria Garcia creado exitosamente';
END
GO

-- =============================================
-- CLIENTE 2: Carlos Rodriguez - Empresario
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'carlos.rodriguez')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1002345678', 'Carlos Rodriguez Mendez', '3202345678', 'carlos.rodriguez', 'carlos123', 0, 0, DATEADD(MONTH, -24, GETDATE()));
    
    DECLARE @ClienteId2 INT = SCOPE_IDENTITY();
    
    -- Cuenta Corriente comercial
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, MontoSobregiro)
    VALUES ('3002345678', 8500000, @ClienteId2, DATEADD(MONTH, -24, GETDATE()), 'CuentaCorriente', 2000000);
    
    DECLARE @Cuenta2C INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros personal
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2002345678', 4200000, @ClienteId2, DATEADD(MONTH, -20, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta2A INT = SCOPE_IDENTITY();
    
    -- Tarjeta de Credito empresarial
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
    VALUES ('4502345678', -1850000, @ClienteId2, DATEADD(MONTH, -22, GETDATE()), 'TarjetaCredito', 5000000);
    
    DECLARE @Cuenta2T INT = SCOPE_IDENTITY();
    
    -- Movimientos comerciales
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta2C, 'Consignacion', 5000000, 'Capital inicial empresa', 0, 5000000, DATEADD(MONTH, -24, GETDATE())),
        (@Cuenta2C, 'Consignacion', 3200000, 'Venta productos', 5000000, 8200000, DATEADD(DAY, -50, GETDATE())),
        (@Cuenta2C, 'Retiro', 1800000, 'Pago Nomina', 8200000, 6400000, DATEADD(DAY, -35, GETDATE())),
        (@Cuenta2C, 'Consignacion', 4500000, 'Cobro clientes', 6400000, 10900000, DATEADD(DAY, -18, GETDATE())),
        (@Cuenta2C, 'Retiro', 2400000, 'Compra inventario', 10900000, 8500000, DATEADD(DAY, -5, GETDATE())),
        
        (@Cuenta2A, 'Consignacion', 4000000, 'Ahorro personal', 0, 4000000, DATEADD(MONTH, -20, GETDATE())),
        (@Cuenta2A, 'Consignacion', 300000, 'Intereses', 4000000, 4300000, DATEADD(DAY, -80, GETDATE())),
        (@Cuenta2A, 'Retiro', 100000, 'Gastos personales', 4300000, 4200000, DATEADD(DAY, -25, GETDATE())),
        
        (@Cuenta2T, 'Compra en cuotas', 1200000, 'Equipo de oficina - 12 cuotas', 0, -1200000, DATEADD(DAY, -100, GETDATE())),
        (@Cuenta2T, 'Compra', 650000, 'Viaje de negocios', -1200000, -1850000, DATEADD(DAY, -45, GETDATE()));
    
    PRINT 'Cliente Carlos Rodriguez creado exitosamente';
END
GO

-- =============================================
-- CLIENTE 3: Ana Martinez - Profesional Joven
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'ana.martinez')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1003456789', 'Ana Martinez Silva', '3103456789', 'ana.martinez', 'ana123', 0, 0, DATEADD(MONTH, -8, GETDATE()));
    
    DECLARE @ClienteId3 INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2003456789', 1850000, @ClienteId3, DATEADD(MONTH, -8, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta3A INT = SCOPE_IDENTITY();
    
    -- Tarjeta de Credito
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
    VALUES ('4503456789', -420000, @ClienteId3, DATEADD(MONTH, -6, GETDATE()), 'TarjetaCredito', 1500000);
    
    DECLARE @Cuenta3T INT = SCOPE_IDENTITY();
    
    -- Movimientos de profesional joven
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta3A, 'Consignacion', 800000, 'Primera Nomina', 0, 800000, DATEADD(MONTH, -8, GETDATE())),
        (@Cuenta3A, 'Consignacion', 850000, 'Nomina', 800000, 1650000, DATEADD(MONTH, -7, GETDATE())),
        (@Cuenta3A, 'Retiro', 300000, 'Arriendo', 1650000, 1350000, DATEADD(DAY, -55, GETDATE())),
        (@Cuenta3A, 'Consignacion', 900000, 'Nomina', 1350000, 2250000, DATEADD(DAY, -30, GETDATE())),
        (@Cuenta3A, 'Retiro', 400000, 'Gastos varios', 2250000, 1850000, DATEADD(DAY, -12, GETDATE())),
        
        (@Cuenta3T, 'Compra en cuotas', 280000, 'Ropa y accesorios', 0, -280000, DATEADD(DAY, -70, GETDATE())),
        (@Cuenta3T, 'Compra', 140000, 'Cena romantica', -280000, -420000, DATEADD(DAY, -25, GETDATE()));
    
    PRINT 'Cliente Ana Martinez creado exitosamente';
END
GO

-- =============================================
-- CLIENTE 4: Luis Hernandez - Pensionado
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'luis.hernandez')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1004567890', 'Luis Hernandez Gomez', '3004567890', 'luis.hernandez', 'luis123', 0, 0, DATEADD(YEAR, -5, GETDATE()));
    
    DECLARE @ClienteId4 INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros principal
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2004567890', 12500000, @ClienteId4, DATEADD(YEAR, -5, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta4A INT = SCOPE_IDENTITY();
    
    -- Cuenta Corriente para gastos
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, MontoSobregiro)
    VALUES ('3004567890', 650000, @ClienteId4, DATEADD(YEAR, -4, GETDATE()), 'CuentaCorriente', 300000);
    
    DECLARE @Cuenta4C INT = SCOPE_IDENTITY();
    
    -- Movimientos de pensionado
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta4A, 'Consignacion', 10000000, 'Cesantias', 0, 10000000, DATEADD(YEAR, -5, GETDATE())),
        (@Cuenta4A, 'Consignacion', 1200000, 'Pension', 10000000, 11200000, DATEADD(MONTH, -6, GETDATE())),
        (@Cuenta4A, 'Consignacion', 1200000, 'Pension', 11200000, 12400000, DATEADD(MONTH, -5, GETDATE())),
        (@Cuenta4A, 'Consignacion', 1200000, 'Pension', 12400000, 13600000, DATEADD(MONTH, -4, GETDATE())),
        (@Cuenta4A, 'Retiro', 1100000, 'Gastos mensuales', 13600000, 12500000, DATEADD(DAY, -15, GETDATE())),
        
        (@Cuenta4C, 'Consignacion', 500000, 'Transferencia para gastos', 0, 500000, DATEADD(MONTH, -3, GETDATE())),
        (@Cuenta4C, 'Retiro', 150000, 'Medicamentos', 500000, 350000, DATEADD(DAY, -40, GETDATE())),
        (@Cuenta4C, 'Consignacion', 500000, 'Transferencia mensual', 350000, 850000, DATEADD(DAY, -20, GETDATE())),
        (@Cuenta4C, 'Retiro', 200000, 'Supermercado', 850000, 650000, DATEADD(DAY, -8, GETDATE()));
    
    PRINT 'Cliente Luis Hernandez creado exitosamente';
END
GO

-- =============================================
-- CLIENTE 5: Sofia Lopez - Estudiante
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'sofia.lopez')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1005678901', 'Sofia Lopez Ramirez', '3205678901', 'sofia.lopez', 'sofia123', 0, 0, DATEADD(MONTH, -6, GETDATE()));
    
    DECLARE @ClienteId5 INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros b??sica
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2005678901', 380000, @ClienteId5, DATEADD(MONTH, -6, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta5A INT = SCOPE_IDENTITY();
    
    -- Tarjeta de Credito estudiante
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
    VALUES ('4505678901', -220000, @ClienteId5, DATEADD(MONTH, -4, GETDATE()), 'TarjetaCredito', 800000);
    
    DECLARE @Cuenta5T INT = SCOPE_IDENTITY();
    
    -- Movimientos de estudiante
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta5A, 'Consignacion', 500000, 'Mesada padres', 0, 500000, DATEADD(MONTH, -6, GETDATE())),
        (@Cuenta5A, 'Retiro', 200000, 'Matricula', 500000, 300000, DATEADD(MONTH, -5, GETDATE())),
        (@Cuenta5A, 'Consignacion', 500000, 'Mesada', 300000, 800000, DATEADD(DAY, -60, GETDATE())),
        (@Cuenta5A, 'Retiro', 350000, 'Libros y utiles', 800000, 450000, DATEADD(DAY, -45, GETDATE())),
        (@Cuenta5A, 'Retiro', 70000, 'Transporte', 450000, 380000, DATEADD(DAY, -10, GETDATE())),
        
        (@Cuenta5T, 'Compra en cuotas', 150000, 'Laptop accesorios', 0, -150000, DATEADD(DAY, -50, GETDATE())),
        (@Cuenta5T, 'Compra', 70000, 'Salida con amigos', -150000, -220000, DATEADD(DAY, -20, GETDATE()));
    
    PRINT 'Cliente Sofia Lopez creado exitosamente';
END
GO

-- =============================================
-- CLIENTE 6: Roberto Sanchez - Freelancer
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'roberto.sanchez')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1006789012', 'Roberto Sanchez Torres', '3106789012', 'roberto.sanchez', 'roberto123', 0, 0, DATEADD(MONTH, -14, GETDATE()));
    
    DECLARE @ClienteId6 INT = SCOPE_IDENTITY();
    
    -- Cuenta Corriente principal
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, MontoSobregiro)
    VALUES ('3006789012', 3200000, @ClienteId6, DATEADD(MONTH, -14, GETDATE()), 'CuentaCorriente', 1000000);
    
    DECLARE @Cuenta6C INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2006789012', 5600000, @ClienteId6, DATEADD(MONTH, -12, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta6A INT = SCOPE_IDENTITY();
    
    -- Tarjeta de Credito
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
    VALUES ('4506789012', -980000, @ClienteId6, DATEADD(MONTH, -10, GETDATE()), 'TarjetaCredito', 2500000);
    
    DECLARE @Cuenta6T INT = SCOPE_IDENTITY();
    
    -- Movimientos irregulares de freelancer
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta6C, 'Consignacion', 2500000, 'Proyecto web empresa A', 0, 2500000, DATEADD(MONTH, -14, GETDATE())),
        (@Cuenta6C, 'Consignacion', 1800000, 'Diseno aplicacion movil', 2500000, 4300000, DATEADD(DAY, -90, GETDATE())),
        (@Cuenta6C, 'Retiro', 1500000, 'Pago impuestos', 4300000, 2800000, DATEADD(DAY, -70, GETDATE())),
        (@Cuenta6C, 'Consignacion', 3200000, 'Proyecto consultoria', 2800000, 6000000, DATEADD(DAY, -40, GETDATE())),
        (@Cuenta6C, 'Retiro', 2800000, 'Inversion equipo', 6000000, 3200000, DATEADD(DAY, -15, GETDATE())),
        
        (@Cuenta6A, 'Consignacion', 5000000, 'Ahorro proyectos', 0, 5000000, DATEADD(MONTH, -12, GETDATE())),
        (@Cuenta6A, 'Consignacion', 800000, 'Ahorro mensual', 5000000, 5800000, DATEADD(DAY, -60, GETDATE())),
        (@Cuenta6A, 'Retiro', 200000, 'Emergencia medica', 5800000, 5600000, DATEADD(DAY, -28, GETDATE())),
        
        (@Cuenta6T, 'Compra en cuotas', 650000, 'Monitor y perifericos - 9 cuotas', 0, -650000, DATEADD(DAY, -85, GETDATE())),
        (@Cuenta6T, 'Compra', 330000, 'Software licencias', -650000, -980000, DATEADD(DAY, -35, GETDATE()));
    
    PRINT 'Cliente Roberto Sanchez creado exitosamente';
END
GO

-- =============================================
-- CLIENTE 7: Patricia Morales - medica
-- =============================================
IF NOT EXISTS (SELECT 1 FROM Clientes WHERE Usuario = 'patricia.morales')
BEGIN
    INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
    VALUES ('1007890123', 'Patricia Morales Vega', '3207890123', 'patricia.morales', 'patricia123', 0, 0, DATEADD(YEAR, -3, GETDATE()));
    
    DECLARE @ClienteId7 INT = SCOPE_IDENTITY();
    
    -- Cuenta de Ahorros principal
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
    VALUES ('2007890123', 18500000, @ClienteId7, DATEADD(YEAR, -3, GETDATE()), 'CuentaAhorros', GETDATE());
    
    DECLARE @Cuenta7A INT = SCOPE_IDENTITY();
    
    -- Cuenta Corriente profesional
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, MontoSobregiro)
    VALUES ('3007890123', 6800000, @ClienteId7, DATEADD(YEAR, -2, GETDATE()), 'CuentaCorriente', 1500000);
    
    DECLARE @Cuenta7C INT = SCOPE_IDENTITY();
    
    -- Tarjeta de Credito platinum
    INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
    VALUES ('4507890123', -2100000, @ClienteId7, DATEADD(YEAR, -2, GETDATE()), 'TarjetaCredito', 8000000);
    
    DECLARE @Cuenta7T INT = SCOPE_IDENTITY();
    
    -- Movimientos de profesional medica
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@Cuenta7A, 'Consignacion', 8000000, 'Ahorro inicial', 0, 8000000, DATEADD(YEAR, -3, GETDATE())),
        (@Cuenta7A, 'Consignacion', 4500000, 'Ingresos consultas', 8000000, 12500000, DATEADD(MONTH, -8, GETDATE())),
        (@Cuenta7A, 'Consignacion', 5000000, 'Honorarios cirugias', 12500000, 17500000, DATEADD(MONTH, -4, GETDATE())),
        (@Cuenta7A, 'Consignacion', 2800000, 'Consultas mes', 17500000, 20300000, DATEADD(DAY, -45, GETDATE())),
        (@Cuenta7A, 'Retiro', 1800000, 'Inversion consultorio', 20300000, 18500000, DATEADD(DAY, -20, GETDATE())),
        
        (@Cuenta7C, 'Consignacion', 5000000, 'Transferencia operativa', 0, 5000000, DATEADD(MONTH, -10, GETDATE())),
        (@Cuenta7C, 'Retiro', 2200000, 'Pago empleados', 5000000, 2800000, DATEADD(DAY, -55, GETDATE())),
        (@Cuenta7C, 'Consignacion', 6500000, 'Cobro clinica', 2800000, 9300000, DATEADD(DAY, -30, GETDATE())),
        (@Cuenta7C, 'Retiro', 2500000, 'Equipamiento medico', 9300000, 6800000, DATEADD(DAY, -12, GETDATE())),
        
        (@Cuenta7T, 'Compra en cuotas', 1200000, 'Congreso internacional - 12 cuotas', 0, -1200000, DATEADD(DAY, -120, GETDATE())),
        (@Cuenta7T, 'Compra', 650000, 'Material bibliografico', -1200000, -1850000, DATEADD(DAY, -65, GETDATE())),
        (@Cuenta7T, 'Compra', 250000, 'Cena familiar', -1850000, -2100000, DATEADD(DAY, -18, GETDATE()));
    
    PRINT 'Cliente Patricia Morales creado exitosamente';
END
GO

-- =============================================
-- RESUMEN DE CLIENTES INSERTADOS
-- =============================================
PRINT '';
PRINT '========================================';
PRINT 'RESUMEN DE DATOS INSERTADOS';
PRINT '========================================';

SELECT 
    COUNT(*) AS TotalClientesNuevos
FROM Clientes
WHERE Usuario IN ('maria.garcia', 'carlos.rodriguez', 'ana.martinez', 
                   'luis.hernandez', 'sofia.lopez', 'roberto.sanchez', 'patricia.morales');

SELECT 
    COUNT(*) AS TotalCuentasNuevas
FROM Cuentas
WHERE ClienteId IN (
    SELECT Id FROM Clientes 
    WHERE Usuario IN ('maria.garcia', 'carlos.rodriguez', 'ana.martinez', 
                      'luis.hernandez', 'sofia.lopez', 'roberto.sanchez', 'patricia.morales')
);

SELECT 
    COUNT(*) AS TotalMovimientosNuevos
FROM Movimientos
WHERE CuentaId IN (
    SELECT c.Id FROM Cuentas c
    INNER JOIN Clientes cl ON c.ClienteId = cl.Id
    WHERE cl.Usuario IN ('maria.garcia', 'carlos.rodriguez', 'ana.martinez', 
                         'luis.hernandez', 'sofia.lopez', 'roberto.sanchez', 'patricia.morales')
);

PRINT '';
PRINT 'Datos de prueba insertados exitosamente';
PRINT '========================================';
GO

-- =============================================
-- CONSULTA RESUMEN DE TODOS LOS CLIENTES
-- =============================================
SELECT 
    cl.Id,
    cl.Nombre,
    cl.Usuario,
    cl.FechaRegistro,
    COUNT(DISTINCT c.Id) AS TotalCuentas,
    SUM(c.Saldo) AS SaldoTotal,
    COUNT(DISTINCT m.Id) AS TotalMovimientos
FROM Clientes cl
LEFT JOIN Cuentas c ON cl.Id = c.ClienteId
LEFT JOIN Movimientos m ON c.Id = m.CuentaId
GROUP BY cl.Id, cl.Nombre, cl.Usuario, cl.FechaRegistro
ORDER BY cl.FechaRegistro DESC;
GO
