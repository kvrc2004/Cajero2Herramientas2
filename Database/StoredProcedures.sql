-- =============================================
-- PROCEDIMIENTOS ALMACENADOS - MiBanco
-- Base de Datos: MiPlataDB
-- Fecha: 18 de Noviembre de 2025
-- =============================================

USE MiPlataDB;
GO

-- =============================================
-- 1. Procedimiento: Obtener Resumen Completo del Cliente
-- Descripción: Retorna información completa del cliente con sus cuentas y totales
-- =============================================
CREATE OR ALTER PROCEDURE sp_ObtenerResumenCliente
    @ClienteId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Información del cliente
    SELECT 
        c.Id,
        c.Identificacion,
        c.Nombre,
        c.Celular,
        c.Usuario,
        c.FechaRegistro,
        c.CuentaBloqueada,
        COUNT(DISTINCT cu.Id) AS TotalCuentas,
        SUM(CASE WHEN cu.Saldo > 0 THEN cu.Saldo ELSE 0 END) AS TotalSaldoPositivo,
        SUM(CASE WHEN cu.Saldo < 0 THEN ABS(cu.Saldo) ELSE 0 END) AS TotalDeuda,
        COUNT(DISTINCT m.Id) AS TotalMovimientos
    FROM Clientes c
    LEFT JOIN Cuentas cu ON c.Id = cu.ClienteId
    LEFT JOIN Movimientos m ON cu.Id = m.CuentaId
    WHERE c.Id = @ClienteId
    GROUP BY c.Id, c.Identificacion, c.Nombre, c.Celular, c.Usuario, c.FechaRegistro, c.CuentaBloqueada;
    
    -- Detalle de cuentas
    SELECT 
        cu.Id,
        cu.NumeroCuenta,
        cu.TipoCuenta,
        cu.Saldo,
        cu.FechaCreacion,
        CASE 
            WHEN cu.TipoCuenta = 'CuentaAhorros' THEN cu.UltimaFechaCalculoInteres
            ELSE NULL
        END AS UltimaFechaCalculoInteres,
        CASE 
            WHEN cu.TipoCuenta = 'CuentaCorriente' THEN cu.MontoSobregiro
            ELSE NULL
        END AS MontoSobregiro,
        CASE 
            WHEN cu.TipoCuenta = 'TarjetaCredito' THEN cu.LimiteCredito
            ELSE NULL
        END AS LimiteCredito,
        COUNT(m.Id) AS CantidadMovimientos
    FROM Cuentas cu
    LEFT JOIN Movimientos m ON cu.Id = m.CuentaId
    WHERE cu.ClienteId = @ClienteId
    GROUP BY cu.Id, cu.NumeroCuenta, cu.TipoCuenta, cu.Saldo, cu.FechaCreacion, 
             cu.UltimaFechaCalculoInteres, cu.MontoSobregiro, cu.LimiteCredito
    ORDER BY cu.FechaCreacion;
END;
GO

-- =============================================
-- 2. Procedimiento: Realizar Transferencia Segura
-- Descripción: Realiza una transferencia entre dos cuentas de forma transaccional
-- =============================================
CREATE OR ALTER PROCEDURE sp_RealizarTransferencia
    @CuentaOrigenId INT,
    @CuentaDestinoId INT,
    @Monto DECIMAL(18,2),
    @Descripcion NVARCHAR(MAX) = 'Transferencia',
    @Resultado INT OUTPUT,
    @Mensaje NVARCHAR(255) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Inicializar variables de salida
    SET @Resultado = 0;
    SET @Mensaje = '';
    
    -- Iniciar transacción
    BEGIN TRANSACTION;
    
    BEGIN TRY
        DECLARE @SaldoOrigen DECIMAL(18,2);
        DECLARE @SaldoDestino DECIMAL(18,2);
        DECLARE @NumeroCuentaOrigen NVARCHAR(50);
        DECLARE @NumeroCuentaDestino NVARCHAR(50);
        
        -- Validar que las cuentas existan y obtener saldos actuales
        SELECT @SaldoOrigen = Saldo, @NumeroCuentaOrigen = NumeroCuenta
        FROM Cuentas WITH (UPDLOCK)
        WHERE Id = @CuentaOrigenId;
        
        IF @SaldoOrigen IS NULL
        BEGIN
            SET @Mensaje = 'Cuenta origen no encontrada';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        SELECT @SaldoDestino = Saldo, @NumeroCuentaDestino = NumeroCuenta
        FROM Cuentas WITH (UPDLOCK)
        WHERE Id = @CuentaDestinoId;
        
        IF @SaldoDestino IS NULL
        BEGIN
            SET @Mensaje = 'Cuenta destino no encontrada';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validar que las cuentas sean diferentes
        IF @CuentaOrigenId = @CuentaDestinoId
        BEGIN
            SET @Mensaje = 'No se puede transferir a la misma cuenta';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validar monto positivo
        IF @Monto <= 0
        BEGIN
            SET @Mensaje = 'El monto debe ser mayor a cero';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Validar saldo suficiente
        IF @SaldoOrigen < @Monto
        BEGIN
            SET @Mensaje = 'Saldo insuficiente en cuenta origen';
            ROLLBACK TRANSACTION;
            RETURN;
        END
        
        -- Realizar el débito en cuenta origen
        UPDATE Cuentas
        SET Saldo = Saldo - @Monto
        WHERE Id = @CuentaOrigenId;
        
        -- Registrar movimiento de débito
        INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
        VALUES (@CuentaOrigenId, 'Transferencia Enviada', @Monto, 
                @Descripcion + ' a cuenta ' + @NumeroCuentaDestino, 
                @SaldoOrigen, @SaldoOrigen - @Monto, GETDATE());
        
        -- Realizar el crédito en cuenta destino
        UPDATE Cuentas
        SET Saldo = Saldo + @Monto
        WHERE Id = @CuentaDestinoId;
        
        -- Registrar movimiento de crédito
        INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo, Fecha)
        VALUES (@CuentaDestinoId, 'Transferencia Recibida', @Monto, 
                @Descripcion + ' de cuenta ' + @NumeroCuentaOrigen, 
                @SaldoDestino, @SaldoDestino + @Monto, GETDATE());
        
        -- Confirmar transacción
        COMMIT TRANSACTION;
        
        SET @Resultado = 1;
        SET @Mensaje = 'Transferencia realizada exitosamente';
        
    END TRY
    BEGIN CATCH
        -- En caso de error, revertir transacción
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SET @Mensaje = 'Error: ' + ERROR_MESSAGE();
    END CATCH
END;
GO

-- =============================================
-- 3. Procedimiento: Obtener Movimientos por Rango de Fechas
-- Descripción: Retorna movimientos de una cuenta en un rango de fechas
-- =============================================
CREATE OR ALTER PROCEDURE sp_ObtenerMovimientosPorFecha
    @CuentaId INT,
    @FechaInicio DATETIME,
    @FechaFin DATETIME
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        m.Id,
        m.Tipo,
        m.Monto,
        m.Descripcion,
        m.SaldoAnterior,
        m.SaldoNuevo,
        m.Fecha,
        c.NumeroCuenta,
        c.TipoCuenta
    FROM Movimientos m
    INNER JOIN Cuentas c ON m.CuentaId = c.Id
    WHERE m.CuentaId = @CuentaId
      AND m.Fecha >= @FechaInicio
      AND m.Fecha <= @FechaFin
    ORDER BY m.Fecha DESC;
END;
GO

-- =============================================
-- 4. Procedimiento: Calcular Totales por Tipo de Movimiento
-- Descripción: Retorna el resumen de movimientos agrupados por tipo
-- =============================================
CREATE OR ALTER PROCEDURE sp_ObtenerResumenMovimientos
    @ClienteId INT,
    @FechaInicio DATETIME = NULL,
    @FechaFin DATETIME = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Si no se especifican fechas, usar el mes actual
    IF @FechaInicio IS NULL
        SET @FechaInicio = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
    
    IF @FechaFin IS NULL
        SET @FechaFin = GETDATE();
    
    SELECT 
        m.Tipo,
        COUNT(*) AS Cantidad,
        SUM(m.Monto) AS TotalMonto,
        AVG(m.Monto) AS PromedioMonto,
        MIN(m.Fecha) AS PrimeraTransaccion,
        MAX(m.Fecha) AS UltimaTransaccion
    FROM Movimientos m
    INNER JOIN Cuentas c ON m.CuentaId = c.Id
    WHERE c.ClienteId = @ClienteId
      AND m.Fecha >= @FechaInicio
      AND m.Fecha <= @FechaFin
    GROUP BY m.Tipo
    ORDER BY TotalMonto DESC;
END;
GO

-- =============================================
-- 5. Función: Calcular Saldo Total del Cliente
-- Descripción: Retorna la suma de todos los saldos de las cuentas del cliente
-- =============================================
CREATE OR ALTER FUNCTION fn_CalcularSaldoTotal(@ClienteId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @SaldoTotal DECIMAL(18,2);
    
    SELECT @SaldoTotal = ISNULL(SUM(Saldo), 0)
    FROM Cuentas
    WHERE ClienteId = @ClienteId;
    
    RETURN @SaldoTotal;
END;
GO

-- =============================================
-- 6. Función: Obtener Crédito Disponible Total
-- Descripción: Retorna el crédito disponible en todas las tarjetas del cliente
-- =============================================
CREATE OR ALTER FUNCTION fn_ObtenerCreditoDisponible(@ClienteId INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @CreditoDisponible DECIMAL(18,2);
    
    SELECT @CreditoDisponible = ISNULL(SUM(LimiteCredito + Saldo), 0)
    FROM Cuentas
    WHERE ClienteId = @ClienteId
      AND TipoCuenta = 'TarjetaCredito';
    
    RETURN @CreditoDisponible;
END;
GO

-- =============================================
-- EJEMPLOS DE USO
-- =============================================

-- Ejemplo 1: Obtener resumen de cliente
/*
DECLARE @ClienteId INT = 1;
EXEC sp_ObtenerResumenCliente @ClienteId;
*/

-- Ejemplo 2: Realizar transferencia
/*
DECLARE @Resultado INT, @Mensaje NVARCHAR(255);
EXEC sp_RealizarTransferencia 
    @CuentaOrigenId = 1,
    @CuentaDestinoId = 2,
    @Monto = 50000,
    @Descripcion = 'Pago de servicios',
    @Resultado = @Resultado OUTPUT,
    @Mensaje = @Mensaje OUTPUT;
SELECT @Resultado AS Resultado, @Mensaje AS Mensaje;
*/

-- Ejemplo 3: Obtener movimientos del mes actual
/*
DECLARE @FechaInicio DATETIME = DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1);
DECLARE @FechaFin DATETIME = GETDATE();
EXEC sp_ObtenerMovimientosPorFecha @CuentaId = 1, @FechaInicio = @FechaInicio, @FechaFin = @FechaFin;
*/

-- Ejemplo 4: Obtener resumen de movimientos
/*
EXEC sp_ObtenerResumenMovimientos @ClienteId = 1;
*/

-- Ejemplo 5: Usar función para calcular saldo total
/*
SELECT dbo.fn_CalcularSaldoTotal(1) AS SaldoTotal;
*/

-- Ejemplo 6: Usar función para obtener crédito disponible
/*
SELECT dbo.fn_ObtenerCreditoDisponible(1) AS CreditoDisponible;
*/

PRINT 'Procedimientos almacenados y funciones creados exitosamente';
GO
