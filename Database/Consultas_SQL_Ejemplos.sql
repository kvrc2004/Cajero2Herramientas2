-- =============================================
-- CONSULTAS SQL - EJEMPLOS Y DOCUMENTACIÓN
-- Base de Datos: MiPlataDB
-- Proyecto: Sistema Bancario MiBanco
-- Fecha: 18 de Noviembre de 2025
-- =============================================

USE MiPlataDB;
GO

-- =============================================
-- SECCIÓN 1: CONSULTAS BÁSICAS (SELECT)
-- =============================================

-- Consulta 1: Obtener todos los clientes activos
-- Descripción: Lista todos los clientes cuyas cuentas no están bloqueadas
SELECT 
    Id,
    Identificacion,
    Nombre,
    Celular,
    Usuario,
    FechaRegistro,
    CuentaBloqueada
FROM Clientes
WHERE CuentaBloqueada = 0
ORDER BY FechaRegistro DESC;

-- Consulta 2: Obtener información de un cliente específico por usuario
-- Descripción: Busca un cliente por su nombre de usuario
SELECT 
    c.Id,
    c.Identificacion,
    c.Nombre,
    c.Celular,
    c.Usuario,
    c.FechaRegistro,
    c.CuentaBloqueada,
    c.IntentosLogin
FROM Clientes c
WHERE c.Usuario = 'juan.perez';  -- Reemplazar con el usuario deseado

-- Consulta 3: Obtener todas las cuentas con saldo positivo
-- Descripción: Lista todas las cuentas que tienen saldo mayor a cero
SELECT 
    c.Id,
    c.NumeroCuenta,
    c.TipoCuenta,
    c.Saldo,
    c.ClienteId,
    c.FechaCreacion
FROM Cuentas c
WHERE c.Saldo > 0
ORDER BY c.Saldo DESC;

-- Consulta 4: Obtener cuentas por tipo específico
-- Descripción: Filtra cuentas según su tipo (Ahorros, Corriente, TarjetaCredito)
SELECT 
    c.Id,
    c.NumeroCuenta,
    c.TipoCuenta,
    c.Saldo,
    c.FechaCreacion,
    CASE 
        WHEN c.TipoCuenta = 'CuentaAhorros' THEN 'Cuenta de Ahorros'
        WHEN c.TipoCuenta = 'CuentaCorriente' THEN 'Cuenta Corriente'
        WHEN c.TipoCuenta = 'TarjetaCredito' THEN 'Tarjeta de Crédito'
        ELSE 'Otro'
    END AS TipoDescripcion
FROM Cuentas c
WHERE c.TipoCuenta = 'CuentaAhorros'  -- Cambiar por el tipo deseado
ORDER BY c.FechaCreacion;

-- =============================================
-- SECCIÓN 2: CONSULTAS CON JOIN
-- =============================================

-- Consulta 5: Obtener clientes con sus cuentas
-- Descripción: Lista cada cliente con todas sus cuentas asociadas
SELECT 
    cl.Id AS ClienteId,
    cl.Nombre AS NombreCliente,
    cl.Identificacion,
    cl.Usuario,
    c.Id AS CuentaId,
    c.NumeroCuenta,
    c.TipoCuenta,
    c.Saldo,
    c.FechaCreacion
FROM Clientes cl
INNER JOIN Cuentas c ON cl.Id = c.ClienteId
ORDER BY cl.Nombre, c.FechaCreacion;

-- Consulta 6: Obtener movimientos con información de cuenta y cliente
-- Descripción: Muestra cada movimiento con los datos de la cuenta y el cliente asociado
SELECT 
    m.Id AS MovimientoId,
    m.Tipo AS TipoMovimiento,
    m.Monto,
    m.Descripcion,
    m.SaldoAnterior,
    m.SaldoNuevo,
    m.Fecha,
    c.NumeroCuenta,
    c.TipoCuenta,
    cl.Nombre AS NombreCliente,
    cl.Identificacion
FROM Movimientos m
INNER JOIN Cuentas c ON m.CuentaId = c.Id
INNER JOIN Clientes cl ON c.ClienteId = cl.Id
ORDER BY m.Fecha DESC;

-- Consulta 7: Obtener clientes con el número total de cuentas y saldo total
-- Descripción: Resumen por cliente mostrando cantidad de cuentas y suma de saldos
SELECT 
    cl.Id,
    cl.Nombre,
    cl.Identificacion,
    cl.Usuario,
    COUNT(c.Id) AS TotalCuentas,
    SUM(c.Saldo) AS SaldoTotal,
    AVG(c.Saldo) AS SaldoPromedio,
    MAX(c.Saldo) AS SaldoMaximo,
    MIN(c.Saldo) AS SaldoMinimo
FROM Clientes cl
LEFT JOIN Cuentas c ON cl.Id = c.ClienteId
GROUP BY cl.Id, cl.Nombre, cl.Identificacion, cl.Usuario
ORDER BY SaldoTotal DESC;

-- Consulta 8: Obtener cuentas con su último movimiento
-- Descripción: Muestra cada cuenta junto con su movimiento más reciente
SELECT 
    c.NumeroCuenta,
    c.TipoCuenta,
    c.Saldo AS SaldoActual,
    cl.Nombre AS NombreCliente,
    m.Tipo AS UltimoMovimiento,
    m.Monto AS MontoUltimoMovimiento,
    m.Fecha AS FechaUltimoMovimiento,
    m.Descripcion
FROM Cuentas c
INNER JOIN Clientes cl ON c.ClienteId = cl.Id
OUTER APPLY (
    SELECT TOP 1 
        m2.Tipo, 
        m2.Monto, 
        m2.Fecha,
        m2.Descripcion
    FROM Movimientos m2
    WHERE m2.CuentaId = c.Id
    ORDER BY m2.Fecha DESC
) m
ORDER BY m.Fecha DESC;

-- =============================================
-- SECCIÓN 3: CONSULTAS CON FILTROS COMPLEJOS
-- =============================================

-- Consulta 9: Obtener movimientos por rango de fechas
-- Descripción: Filtra movimientos entre dos fechas específicas
DECLARE @FechaInicio DATETIME = '2025-01-01';
DECLARE @FechaFin DATETIME = '2025-12-31';

SELECT 
    m.Id,
    m.Tipo,
    m.Monto,
    m.Descripcion,
    m.Fecha,
    c.NumeroCuenta,
    cl.Nombre AS NombreCliente
FROM Movimientos m
INNER JOIN Cuentas c ON m.CuentaId = c.Id
INNER JOIN Clientes cl ON c.ClienteId = cl.Id
WHERE m.Fecha >= @FechaInicio AND m.Fecha <= @FechaFin
ORDER BY m.Fecha DESC;

-- Consulta 10: Obtener movimientos por tipo
-- Descripción: Filtra movimientos según su tipo (Depósito, Retiro, etc.)
SELECT 
    m.Tipo,
    COUNT(*) AS CantidadMovimientos,
    SUM(m.Monto) AS MontoTotal,
    AVG(m.Monto) AS MontoPromedio,
    MIN(m.Fecha) AS PrimeraTransaccion,
    MAX(m.Fecha) AS UltimaTransaccion
FROM Movimientos m
WHERE m.Tipo IN ('Depósito', 'Retiro', 'Compra a Crédito')
GROUP BY m.Tipo
ORDER BY MontoTotal DESC;

-- Consulta 11: Obtener clientes con cuentas en negativo
-- Descripción: Lista clientes que tienen al menos una cuenta con saldo negativo
SELECT DISTINCT
    cl.Id,
    cl.Nombre,
    cl.Identificacion,
    cl.Celular,
    c.NumeroCuenta,
    c.TipoCuenta,
    c.Saldo
FROM Clientes cl
INNER JOIN Cuentas c ON cl.Id = c.ClienteId
WHERE c.Saldo < 0
ORDER BY c.Saldo ASC;

-- Consulta 12: Obtener tarjetas de crédito con uso mayor al 70%
-- Descripción: Identifica tarjetas que han utilizado más del 70% de su límite
SELECT 
    c.Id,
    c.NumeroCuenta,
    c.LimiteCredito,
    c.Saldo,
    ABS(c.Saldo) AS SaldoUsado,
    c.LimiteCredito + c.Saldo AS CreditoDisponible,
    CAST((ABS(c.Saldo) * 100.0 / c.LimiteCredito) AS DECIMAL(5,2)) AS PorcentajeUso,
    cl.Nombre AS NombreCliente
FROM Cuentas c
INNER JOIN Clientes cl ON c.ClienteId = cl.Id
WHERE c.TipoCuenta = 'TarjetaCredito'
  AND c.LimiteCredito > 0
  AND (ABS(c.Saldo) * 100.0 / c.LimiteCredito) > 70
ORDER BY PorcentajeUso DESC;

-- =============================================
-- SECCIÓN 4: CONSULTAS DE AGREGACIÓN
-- =============================================

-- Consulta 13: Estadísticas generales del banco
-- Descripción: Resumen general con totales de clientes, cuentas y movimientos
SELECT 
    (SELECT COUNT(*) FROM Clientes) AS TotalClientes,
    (SELECT COUNT(*) FROM Clientes WHERE CuentaBloqueada = 0) AS ClientesActivos,
    (SELECT COUNT(*) FROM Cuentas) AS TotalCuentas,
    (SELECT SUM(Saldo) FROM Cuentas) AS SaldoTotalBanco,
    (SELECT COUNT(*) FROM Movimientos) AS TotalMovimientos,
    (SELECT SUM(Monto) FROM Movimientos WHERE Tipo = 'Depósito') AS TotalDepositos,
    (SELECT SUM(Monto) FROM Movimientos WHERE Tipo = 'Retiro') AS TotalRetiros;

-- Consulta 14: Resumen de cuentas por tipo
-- Descripción: Agrupa cuentas por tipo mostrando cantidad y saldo promedio
SELECT 
    TipoCuenta,
    COUNT(*) AS CantidadCuentas,
    SUM(Saldo) AS SaldoTotal,
    AVG(Saldo) AS SaldoPromedio,
    MAX(Saldo) AS SaldoMaximo,
    MIN(Saldo) AS SaldoMinimo
FROM Cuentas
GROUP BY TipoCuenta
ORDER BY SaldoTotal DESC;

-- Consulta 15: Top 5 clientes con mayor saldo total
-- Descripción: Identifica los 5 clientes con mayor patrimonio en el banco
SELECT TOP 5
    cl.Id,
    cl.Nombre,
    cl.Identificacion,
    COUNT(c.Id) AS NumeroCuentas,
    SUM(c.Saldo) AS SaldoTotal
FROM Clientes cl
INNER JOIN Cuentas c ON cl.Id = c.ClienteId
GROUP BY cl.Id, cl.Nombre, cl.Identificacion
ORDER BY SaldoTotal DESC;

-- Consulta 16: Movimientos por mes del año actual
-- Descripción: Agrupa movimientos por mes mostrando cantidad y monto total
SELECT 
    YEAR(m.Fecha) AS Anio,
    MONTH(m.Fecha) AS Mes,
    DATENAME(MONTH, m.Fecha) AS NombreMes,
    COUNT(*) AS CantidadMovimientos,
    SUM(m.Monto) AS MontoTotal,
    AVG(m.Monto) AS MontoPromedio
FROM Movimientos m
WHERE YEAR(m.Fecha) = YEAR(GETDATE())
GROUP BY YEAR(m.Fecha), MONTH(m.Fecha), DATENAME(MONTH, m.Fecha)
ORDER BY Anio, Mes;

-- =============================================
-- SECCIÓN 5: CONSULTAS DE SUBCONSULTAS
-- =============================================

-- Consulta 17: Clientes con saldo superior al promedio
-- Descripción: Lista clientes cuyo saldo total supera el promedio del banco
SELECT 
    cl.Id,
    cl.Nombre,
    cl.Identificacion,
    SUM(c.Saldo) AS SaldoTotal
FROM Clientes cl
INNER JOIN Cuentas c ON cl.Id = c.ClienteId
GROUP BY cl.Id, cl.Nombre, cl.Identificacion
HAVING SUM(c.Saldo) > (
    SELECT AVG(SaldoCliente)
    FROM (
        SELECT SUM(Saldo) AS SaldoCliente
        FROM Cuentas
        GROUP BY ClienteId
    ) AS Promedios
)
ORDER BY SaldoTotal DESC;

-- Consulta 18: Cuentas sin movimientos
-- Descripción: Identifica cuentas que no tienen ningún movimiento registrado
SELECT 
    c.Id,
    c.NumeroCuenta,
    c.TipoCuenta,
    c.Saldo,
    c.FechaCreacion,
    cl.Nombre AS NombreCliente
FROM Cuentas c
INNER JOIN Clientes cl ON c.ClienteId = cl.Id
WHERE NOT EXISTS (
    SELECT 1
    FROM Movimientos m
    WHERE m.CuentaId = c.Id
)
ORDER BY c.FechaCreacion;

-- =============================================
-- SECCIÓN 6: OPERACIONES DE INSERCIÓN (INSERT)
-- =============================================

-- Inserción 1: Agregar nuevo cliente
-- Descripción: Crea un nuevo cliente en el sistema
INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave, CuentaBloqueada, IntentosLogin, FechaRegistro)
VALUES ('1234567890', 'María García López', '3001234567', 'maria.garcia', 
        'clave123', 0, 0, GETDATE());

-- Inserción 2: Agregar cuenta de ahorros
-- Descripción: Crea una nueva cuenta de ahorros para un cliente
INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, UltimaFechaCalculoInteres)
VALUES ('2000000001', 100000, 1, GETDATE(), 'CuentaAhorros', GETDATE());

-- Inserción 3: Agregar tarjeta de crédito
-- Descripción: Crea una nueva tarjeta de crédito con límite
INSERT INTO Cuentas (NumeroCuenta, Saldo, ClienteId, FechaCreacion, TipoCuenta, LimiteCredito)
VALUES ('4500000001', 0, 1, GETDATE(), 'TarjetaCredito', 2000000);

-- Inserción 4: Registrar movimiento de depósito
-- Descripción: Registra un depósito en una cuenta
DECLARE @CuentaId INT = 1;
DECLARE @SaldoActual DECIMAL(18,2);

SELECT @SaldoActual = Saldo FROM Cuentas WHERE Id = @CuentaId;

INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
VALUES (@CuentaId, 'Depósito', 50000, 'Depósito en efectivo', 
        @SaldoActual, @SaldoActual + 50000, GETDATE());

-- =============================================
-- SECCIÓN 7: OPERACIONES DE ACTUALIZACIÓN (UPDATE)
-- =============================================

-- Actualización 1: Actualizar información del cliente
-- Descripción: Modifica datos personales de un cliente
UPDATE Clientes
SET Nombre = 'Juan Carlos Pérez Gómez',
    Celular = '3109876543'
WHERE Usuario = 'juan.perez';

-- Actualización 2: Actualizar saldo de cuenta después de depósito
-- Descripción: Incrementa el saldo de una cuenta
UPDATE Cuentas
SET Saldo = Saldo + 100000
WHERE NumeroCuenta = '1000000001';

-- Actualización 3: Bloquear cuenta de cliente
-- Descripción: Bloquea la cuenta de un cliente por seguridad
UPDATE Clientes
SET CuentaBloqueada = 1,
    IntentosLogin = 3
WHERE Usuario = 'juan.perez';

-- Actualización 4: Calcular intereses en cuentas de ahorro
-- Descripción: Aplica interés del 0.5% a todas las cuentas de ahorro
UPDATE Cuentas
SET Saldo = Saldo * 1.005,
    UltimaFechaCalculoInteres = GETDATE()
WHERE TipoCuenta = 'CuentaAhorros'
  AND Saldo > 0;

-- =============================================
-- SECCIÓN 8: OPERACIONES DE ELIMINACIÓN (DELETE)
-- =============================================

-- Eliminación 1: Eliminar movimientos antiguos
-- Descripción: Elimina movimientos de más de 2 años (USAR CON CUIDADO)
DELETE FROM Movimientos
WHERE Fecha < DATEADD(YEAR, -2, GETDATE());

-- Eliminación 2: Eliminar cuentas con saldo cero sin movimientos
-- Descripción: Limpia cuentas inactivas (USAR CON CUIDADO)
DELETE FROM Cuentas
WHERE Saldo = 0
  AND NOT EXISTS (
      SELECT 1 FROM Movimientos WHERE CuentaId = Cuentas.Id
  );

-- =============================================
-- SECCIÓN 9: CONSULTAS CON VISTAS
-- =============================================

-- Vista 1: Vista de clientes con información resumida
CREATE OR ALTER VIEW vw_ResumenClientes AS
SELECT 
    cl.Id,
    cl.Identificacion,
    cl.Nombre,
    cl.Celular,
    cl.Usuario,
    cl.FechaRegistro,
    cl.CuentaBloqueada,
    COUNT(DISTINCT c.Id) AS TotalCuentas,
    SUM(c.Saldo) AS SaldoTotal,
    COUNT(DISTINCT m.Id) AS TotalMovimientos
FROM Clientes cl
LEFT JOIN Cuentas c ON cl.Id = c.ClienteId
LEFT JOIN Movimientos m ON c.Id = m.CuentaId
GROUP BY cl.Id, cl.Identificacion, cl.Nombre, cl.Celular, cl.Usuario, cl.FechaRegistro, cl.CuentaBloqueada;
GO

-- Usar la vista
SELECT * FROM vw_ResumenClientes
WHERE SaldoTotal > 0
ORDER BY SaldoTotal DESC;

-- =============================================
-- SECCIÓN 10: TRANSACCIONES
-- =============================================

-- Transacción 1: Transferencia entre cuentas
-- Descripción: Realiza una transferencia segura entre dos cuentas
BEGIN TRANSACTION;

DECLARE @CuentaOrigen INT = 1;
DECLARE @CuentaDestino INT = 2;
DECLARE @MontoTransferencia DECIMAL(18,2) = 50000;
DECLARE @SaldoOrigen DECIMAL(18,2);
DECLARE @SaldoDestino DECIMAL(18,2);

-- Obtener saldos actuales
SELECT @SaldoOrigen = Saldo FROM Cuentas WHERE Id = @CuentaOrigen;
SELECT @SaldoDestino = Saldo FROM Cuentas WHERE Id = @CuentaDestino;

-- Validar saldo suficiente
IF @SaldoOrigen >= @MontoTransferencia
BEGIN
    -- Realizar débito
    UPDATE Cuentas SET Saldo = Saldo - @MontoTransferencia WHERE Id = @CuentaOrigen;
    
    -- Registrar movimiento de débito
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES (@CuentaOrigen, 'Transferencia Enviada', @MontoTransferencia, 
            'Transferencia a cuenta', @SaldoOrigen, @SaldoOrigen - @MontoTransferencia, GETDATE());
    
    -- Realizar crédito
    UPDATE Cuentas SET Saldo = Saldo + @MontoTransferencia WHERE Id = @CuentaDestino;
    
    -- Registrar movimiento de crédito
    INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
    VALUES (@CuentaDestino, 'Transferencia Recibida', @MontoTransferencia, 
            'Transferencia recibida', @SaldoDestino, @SaldoDestino + @MontoTransferencia, GETDATE());
    
    COMMIT TRANSACTION;
    PRINT 'Transferencia realizada exitosamente';
END
ELSE
BEGIN
    ROLLBACK TRANSACTION;
    PRINT 'Saldo insuficiente';
END

-- =============================================
-- FIN DEL DOCUMENTO
-- =============================================
PRINT 'Documento de consultas SQL cargado exitosamente';
GO
