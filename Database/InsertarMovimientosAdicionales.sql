-- Script de datos de prueba simplificado
USE MiPlataDB;
GO

-- Insertar movimientos para clientes existentes
DECLARE @CuentaId INT;

-- Cliente 1: Juan Perez - Agregar mas movimientos
SELECT @CuentaId = Id FROM Cuentas WHERE NumeroCuenta = '1000000001';
IF @CuentaId IS NOT NULL
BEGIN
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@CuentaId, 'Consignacion', 500000, 'Nomina mensual', 1900000, 2400000, DATEADD(DAY, -25, GETDATE())),
        (@CuentaId, 'Retiro', 200000, 'Pago servicios', 2400000, 2200000, DATEADD(DAY, -20, GETDATE())),
        (@CuentaId, 'Consignacion', 300000, 'Bono', 2200000, 2500000, DATEADD(DAY, -10, GETDATE()));
    PRINT 'Movimientos agregados para Juan Perez';
END

-- Cliente 2: Kevin Romero
SELECT @CuentaId = Id FROM Cuentas WHERE NumeroCuenta = '2000000002';
IF @CuentaId IS NOT NULL
BEGIN
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@CuentaId, 'Consignacion', 100000, 'Transferencia', 200000, 300000, DATEADD(DAY, -15, GETDATE())),
        (@CuentaId, 'Retiro', 50000, 'Retiro cajero', 300000, 250000, DATEADD(DAY, -8, GETDATE()));
    PRINT 'Movimientos agregados para Kevin Romero';
END

-- Cliente 3: Ferney Romero
SELECT @CuentaId = Id FROM Cuentas WHERE NumeroCuenta = '3000000003';
IF @CuentaId IS NOT NULL
BEGIN
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES 
        (@CuentaId, 'Consignacion', 800000, 'Ingreso mensual', 2999829.29, 3799829.29, DATEADD(DAY, -12, GETDATE())),
        (@CuentaId, 'Retiro', 300000, 'Compras', 3799829.29, 3499829.29, DATEADD(DAY, -5, GETDATE()));
    PRINT 'Movimientos agregados para Ferney Romero';
END

PRINT 'Datos adicionales insertados exitosamente';
GO
