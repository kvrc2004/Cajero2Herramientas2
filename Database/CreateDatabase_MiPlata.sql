-- ============================================
-- SISTEMA BANCARIO "MI PLATA"
-- Base de Datos para SQL Server Management Studio 21
-- ============================================
-- Proyecto: Cajero Automático - ASP.NET Core
-- Autor: Sistema Bancario Mi Plata
-- Fecha: Noviembre 2025
-- Compatible con: SQL Server 2019+, SSMS 21
-- ============================================

USE master;
GO

-- Eliminar base de datos si existe (para pruebas)
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'MiPlataDB')
BEGIN
    ALTER DATABASE MiPlataDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE MiPlataDB;
END
GO

-- Crear la base de datos (SIN especificar ruta - SQL Server usa ubicación predeterminada)
-- Esto funciona en cualquier instalación de SQL Server
CREATE DATABASE MiPlataDB;
GO

USE MiPlataDB;
GO

PRINT '==========================================='
PRINT 'Creando estructura de tablas...'
PRINT '==========================================='
GO

-- ============================================
-- TABLA: Clientes
-- Descripción: Almacena toda la información de los usuarios del sistema
-- Características POO: Representa la clase Cliente del modelo
-- ============================================
CREATE TABLE Clientes (
    -- Clave primaria autoincremental
    Id INT PRIMARY KEY IDENTITY(1,1),
    
    -- Datos personales del cliente
    Identificacion NVARCHAR(20) NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Celular NVARCHAR(15) NOT NULL,
    
    -- Credenciales de acceso
    Usuario NVARCHAR(50) NOT NULL,
    Clave NVARCHAR(100) NOT NULL,
    
    -- Control de seguridad (Sistema de 3 intentos)
    IntentosLogin INT NOT NULL DEFAULT 0,
    CuentaBloqueada BIT NOT NULL DEFAULT 0,
    
    -- Auditoria
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- Constraints de unicidad
    CONSTRAINT UQ_Clientes_Identificacion UNIQUE (Identificacion),
    CONSTRAINT UQ_Clientes_Usuario UNIQUE (Usuario),
    
    -- Validaciones de negocio
    CONSTRAINT CHK_Clientes_Celular CHECK (LEN(Celular) >= 10 AND LEN(Celular) <= 15),
    CONSTRAINT CHK_Clientes_Usuario CHECK (LEN(Usuario) >= 3 AND LEN(Usuario) <= 50),
    CONSTRAINT CHK_Clientes_Clave CHECK (LEN(Clave) >= 6),
    CONSTRAINT CHK_Clientes_IntentosLogin CHECK (IntentosLogin >= 0 AND IntentosLogin <= 10)
);
GO

PRINT 'Tabla Clientes creada exitosamente'
GO

-- ============================================
-- TABLA: Cuentas
-- Descripción: Tabla que implementa herencia TPH (Table Per Hierarchy)
-- POO: Representa la clase abstracta Cuenta y sus 3 hijas
--      - CuentaAhorros (interés 1.5% mensual)
--      - CuentaCorriente (sobregiro 20%)
--      - TarjetaCredito (cuotas con intereses variables)
-- ============================================
CREATE TABLE Cuentas (
    -- Clave primaria autoincremental
    Id INT PRIMARY KEY IDENTITY(1,1),
    
    -- Relación con Cliente (Foreign Key)
    ClienteId INT NOT NULL,
    
    -- Datos básicos de la cuenta
    NumeroCuenta NVARCHAR(50) NOT NULL,
    TipoCuenta NVARCHAR(50) NOT NULL, -- Discriminador de herencia
    Saldo DECIMAL(18,2) NOT NULL DEFAULT 0.00,
    FechaCreacion DATETIME NOT NULL DEFAULT GETDATE(),
    
    -- ==========================================
    -- Campos específicos de CUENTA DE AHORROS
    -- ==========================================
    UltimaFechaCalculoInteres DATETIME NULL,
    
    -- ==========================================
    -- Campos específicos de CUENTA CORRIENTE
    -- ==========================================
    MontoSobregiro DECIMAL(18,2) NULL DEFAULT 0.00,
    
    -- ==========================================
    -- Campos específicos de TARJETA DE CRÉDITO
    -- ==========================================
    LimiteCredito DECIMAL(18,2) NULL,
    
    -- Foreign Key con eliminación en cascada
    CONSTRAINT FK_Cuentas_Clientes FOREIGN KEY (ClienteId) 
        REFERENCES Clientes(Id) ON DELETE CASCADE,
    
    -- Constraint de unicidad para número de cuenta
    CONSTRAINT UQ_Cuentas_NumeroCuenta UNIQUE (NumeroCuenta),
    
    -- Validación del discriminador (solo 3 tipos permitidos)
    CONSTRAINT CHK_Cuentas_TipoCuenta CHECK (
        TipoCuenta IN ('CuentaAhorros', 'CuentaCorriente', 'TarjetaCredito')
    ),
    
    -- Validación de saldo según tipo de cuenta
    CONSTRAINT CHK_Cuentas_Saldo CHECK (
        -- Tarjetas de crédito pueden tener saldo negativo (deuda)
        (TipoCuenta = 'TarjetaCredito') OR 
        -- Otras cuentas deben tener saldo >= 0
        (TipoCuenta != 'TarjetaCredito' AND Saldo >= 0)
    ),
    
    -- Validación de campos específicos por tipo
    CONSTRAINT CHK_Cuentas_CamposEspecificos CHECK (
        -- Cuenta de Ahorros debe tener fecha de cálculo de interés
        (TipoCuenta = 'CuentaAhorros' AND UltimaFechaCalculoInteres IS NOT NULL) OR
        -- Cuenta Corriente debe tener monto de sobregiro definido
        (TipoCuenta = 'CuentaCorriente' AND MontoSobregiro IS NOT NULL) OR
        -- Tarjeta de Crédito debe tener límite de crédito
        (TipoCuenta = 'TarjetaCredito' AND LimiteCredito IS NOT NULL AND LimiteCredito > 0)
    )
);
GO

PRINT 'Tabla Cuentas creada exitosamente'
GO

-- ============================================
-- TABLA: Movimientos
-- Descripción: Historial completo de transacciones bancarias
-- POO: Representa la clase Movimiento (registro de auditoría)
-- ============================================
CREATE TABLE Movimientos (
    -- Clave primaria autoincremental
    Id INT PRIMARY KEY IDENTITY(1,1),
    
    -- Relación con Cuenta (Foreign Key)
    CuentaId INT NOT NULL,
    
    -- Información de la transacción
    Fecha DATETIME NOT NULL DEFAULT GETDATE(),
    Tipo NVARCHAR(50) NOT NULL,
    Monto DECIMAL(18,2) NOT NULL,
    Descripcion NVARCHAR(200) NOT NULL DEFAULT '',
    
    -- Auditoría de saldos (antes y después)
    SaldoAnterior DECIMAL(18,2) NOT NULL,
    SaldoNuevo DECIMAL(18,2) NOT NULL,
    
    -- Foreign Key con eliminación en cascada
    CONSTRAINT FK_Movimientos_Cuentas FOREIGN KEY (CuentaId) 
        REFERENCES Cuentas(Id) ON DELETE CASCADE,
    
    -- Validación de monto positivo
    CONSTRAINT CHK_Movimientos_Monto CHECK (Monto > 0),
    
    -- Validación de tipos de movimiento permitidos
    CONSTRAINT CHK_Movimientos_Tipo CHECK (
        Tipo IN (
            'Consignación', 'Retiro', 'Transferencia', 
            'Pago', 'Compra', 'Compra en cuotas',
            'Intereses Ahorros', 'Avance en efectivo'
        )
    )
);
GO

PRINT 'Tabla Movimientos creada exitosamente'
GO

PRINT '==========================================='
PRINT 'Creando índices para optimización...'
PRINT '==========================================='
GO

-- ============================================
-- ÍNDICES DE RENDIMIENTO
-- Mejoran la velocidad de consultas frecuentes
-- ============================================

-- Índices para tabla Clientes
CREATE NONCLUSTERED INDEX IX_Clientes_Usuario 
    ON Clientes(Usuario ASC)
    INCLUDE (Clave, CuentaBloqueada, IntentosLogin);
GO

CREATE NONCLUSTERED INDEX IX_Clientes_Identificacion 
    ON Clientes(Identificacion ASC);
GO

-- Índices para tabla Cuentas
CREATE NONCLUSTERED INDEX IX_Cuentas_ClienteId 
    ON Cuentas(ClienteId ASC)
    INCLUDE (NumeroCuenta, TipoCuenta, Saldo);
GO

CREATE NONCLUSTERED INDEX IX_Cuentas_NumeroCuenta 
    ON Cuentas(NumeroCuenta ASC);
GO

CREATE NONCLUSTERED INDEX IX_Cuentas_TipoCuenta 
    ON Cuentas(TipoCuenta ASC, ClienteId ASC);
GO

-- Índices para tabla Movimientos
CREATE NONCLUSTERED INDEX IX_Movimientos_CuentaId_Fecha 
    ON Movimientos(CuentaId ASC, Fecha DESC)
    INCLUDE (Tipo, Monto, Descripcion);
GO

CREATE NONCLUSTERED INDEX IX_Movimientos_Fecha 
    ON Movimientos(Fecha DESC);
GO

CREATE NONCLUSTERED INDEX IX_Movimientos_Tipo 
    ON Movimientos(Tipo ASC, Fecha DESC);
GO

PRINT 'Índices creados exitosamente'
GO

PRINT '==========================================='
PRINT 'Creando vistas de consulta...'
PRINT '==========================================='
GO

-- ============================================
-- VISTA: VW_ResumenClientes
-- Descripción: Resume información financiera de cada cliente
-- Uso: Dashboard, reportes generales
-- ============================================
CREATE VIEW VW_ResumenClientes AS
SELECT 
    c.Id AS ClienteId,
    c.Identificacion,
    c.Nombre,
    c.Usuario,
    c.Celular,
    c.FechaRegistro,
    c.CuentaBloqueada,
    c.IntentosLogin,
    
    -- Contadores
    COUNT(DISTINCT cu.Id) AS TotalCuentas,
    COUNT(DISTINCT m.Id) AS TotalMovimientos,
    
    -- Saldos por tipo de cuenta
    SUM(CASE WHEN cu.TipoCuenta = 'CuentaAhorros' THEN cu.Saldo ELSE 0 END) AS SaldoAhorros,
    SUM(CASE WHEN cu.TipoCuenta = 'CuentaCorriente' THEN cu.Saldo ELSE 0 END) AS SaldoCorriente,
    
    -- Para tarjetas de crédito
    SUM(CASE WHEN cu.TipoCuenta = 'TarjetaCredito' THEN cu.LimiteCredito ELSE 0 END) AS LimiteCreditoTotal,
    SUM(CASE WHEN cu.TipoCuenta = 'TarjetaCredito' THEN ABS(cu.Saldo) ELSE 0 END) AS DeudaCredito,
    SUM(CASE WHEN cu.TipoCuenta = 'TarjetaCredito' THEN (cu.LimiteCredito - ABS(cu.Saldo)) ELSE 0 END) AS CreditoDisponible,
    
    -- Total consolidado (sin incluir deuda de crédito)
    SUM(CASE WHEN cu.TipoCuenta != 'TarjetaCredito' THEN cu.Saldo ELSE 0 END) AS PatrimonioTotal
    
FROM Clientes c
LEFT JOIN Cuentas cu ON c.Id = cu.ClienteId
LEFT JOIN Movimientos m ON cu.Id = m.CuentaId
GROUP BY 
    c.Id, c.Identificacion, c.Nombre, c.Usuario, 
    c.Celular, c.FechaRegistro, c.CuentaBloqueada, c.IntentosLogin;
GO

PRINT 'Vista VW_ResumenClientes creada'
GO

-- ============================================
-- VISTA: VW_HistorialMovimientos
-- Descripción: Historial completo con información del cliente
-- Uso: Consulta de movimientos, auditoría
-- ============================================
CREATE VIEW VW_HistorialMovimientos AS
SELECT 
    m.Id AS MovimientoId,
    m.Fecha,
    m.Tipo,
    m.Monto,
    m.Descripcion,
    m.SaldoAnterior,
    m.SaldoNuevo,
    
    -- Información de la cuenta
    cu.Id AS CuentaId,
    cu.NumeroCuenta,
    cu.TipoCuenta,
    
    -- Información del cliente
    c.Id AS ClienteId,
    c.Nombre AS NombreCliente,
    c.Identificacion,
    
    -- Clasificación de movimiento
    CASE 
        WHEN m.Tipo IN ('Retiro', 'Transferencia', 'Compra', 'Compra en cuotas') THEN 'Débito'
        WHEN m.Tipo IN ('Consignación', 'Pago', 'Intereses Ahorros') THEN 'Crédito'
        ELSE 'Otro'
    END AS ClasificacionMovimiento
    
FROM Movimientos m
INNER JOIN Cuentas cu ON m.CuentaId = cu.Id
INNER JOIN Clientes c ON cu.ClienteId = c.Id;
GO

PRINT 'Vista VW_HistorialMovimientos creada'
GO

-- ============================================
-- VISTA: VW_EstadoCuentas
-- Descripción: Estado detallado de todas las cuentas con cálculos
-- Uso: Dashboard de cuentas, consultas de saldo
-- ============================================
CREATE VIEW VW_EstadoCuentas AS
SELECT 
    cu.Id AS CuentaId,
    cu.NumeroCuenta,
    cu.TipoCuenta,
    cu.Saldo,
    cu.FechaCreacion,
    DATEDIFF(DAY, cu.FechaCreacion, GETDATE()) AS DiasAbierta,
    
    -- Información del cliente
    c.Id AS ClienteId,
    c.Nombre AS NombreCliente,
    c.Identificacion,
    
    -- Cálculos específicos por tipo de cuenta
    CASE cu.TipoCuenta
        -- Cuenta de Ahorros: Intereses proyectados (1.5% mensual)
        WHEN 'CuentaAhorros' THEN 
            CAST(cu.Saldo * 0.015 AS DECIMAL(18,2))
        ELSE 0 
    END AS InteresesProyectados,
    
    CASE cu.TipoCuenta
        -- Cuenta Corriente: Sobregiro disponible (20% del saldo)
        WHEN 'CuentaCorriente' THEN 
            CAST((cu.Saldo * 0.20) - ISNULL(cu.MontoSobregiro, 0) AS DECIMAL(18,2))
        ELSE 0 
    END AS SobregiroDisponible,
    
    CASE cu.TipoCuenta
        -- Tarjeta de Crédito: Crédito disponible
        WHEN 'TarjetaCredito' THEN 
            cu.LimiteCredito - ABS(cu.Saldo)
        ELSE NULL 
    END AS CreditoDisponible,
    
    -- Campos específicos
    cu.LimiteCredito,
    cu.MontoSobregiro,
    cu.UltimaFechaCalculoInteres,
    
    -- Estadísticas de movimientos
    COUNT(m.Id) AS TotalMovimientos,
    ISNULL(SUM(CASE WHEN m.Tipo IN ('Consignación', 'Pago') THEN m.Monto ELSE 0 END), 0) AS TotalIngresos,
    ISNULL(SUM(CASE WHEN m.Tipo IN ('Retiro', 'Compra', 'Compra en cuotas') THEN m.Monto ELSE 0 END), 0) AS TotalEgresos
    
FROM Cuentas cu
INNER JOIN Clientes c ON cu.ClienteId = c.Id
LEFT JOIN Movimientos m ON cu.Id = m.CuentaId
GROUP BY 
    cu.Id, cu.NumeroCuenta, cu.TipoCuenta, cu.Saldo, cu.FechaCreacion,
    c.Id, c.Nombre, c.Identificacion, 
    cu.LimiteCredito, cu.MontoSobregiro, cu.UltimaFechaCalculoInteres;
GO

PRINT 'Vista VW_EstadoCuentas creada'
GO

PRINT '==========================================='
PRINT 'Creando procedimientos almacenados...'
PRINT '==========================================='
GO

-- ============================================
-- SP: SP_RegistrarClienteCompleto
-- Descripción: Registra un cliente nuevo con sus 3 cuentas iniciales
-- Parámetros: Datos del cliente
-- Retorna: ID del cliente creado
-- Transaccional: Sí (todo o nada)
-- ============================================
CREATE PROCEDURE SP_RegistrarClienteCompleto
    @Identificacion NVARCHAR(20),
    @Nombre NVARCHAR(100),
    @Celular NVARCHAR(15),
    @Usuario NVARCHAR(50),
    @Clave NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. Validar que el usuario no exista
        IF EXISTS (SELECT 1 FROM Clientes WHERE Usuario = @Usuario)
        BEGIN
            THROW 50001, 'El usuario ya existe en el sistema', 1;
        END
        
        -- 2. Validar que la identificación no exista
        IF EXISTS (SELECT 1 FROM Clientes WHERE Identificacion = @Identificacion)
        BEGIN
            THROW 50002, 'La identificación ya está registrada', 1;
        END
        
        -- 3. Insertar el cliente
        INSERT INTO Clientes (Identificacion, Nombre, Celular, Usuario, Clave)
        VALUES (@Identificacion, @Nombre, @Celular, @Usuario, @Clave);
        
        DECLARE @ClienteId INT = SCOPE_IDENTITY();
        DECLARE @NumeroBase NVARCHAR(6) = RIGHT('000000' + CAST(@ClienteId AS NVARCHAR), 6);
        
        -- 4. Crear Cuenta de Ahorros (Interés 1.5% mensual)
        INSERT INTO Cuentas (ClienteId, NumeroCuenta, TipoCuenta, Saldo, UltimaFechaCalculoInteres)
        VALUES (@ClienteId, 'AH' + @NumeroBase, 'CuentaAhorros', 0, GETDATE());
        
        -- 5. Crear Cuenta Corriente (Sobregiro 20%)
        INSERT INTO Cuentas (ClienteId, NumeroCuenta, TipoCuenta, Saldo, MontoSobregiro)
        VALUES (@ClienteId, 'CC' + @NumeroBase, 'CuentaCorriente', 0, 0);
        
        -- 6. Crear Tarjeta de Crédito (Límite inicial $1,000,000)
        INSERT INTO Cuentas (ClienteId, NumeroCuenta, TipoCuenta, Saldo, LimiteCredito)
        VALUES (@ClienteId, 'TC' + @NumeroBase, 'TarjetaCredito', 0, 1000000);
        
        COMMIT TRANSACTION;
        
        -- Retornar resultado exitoso
        SELECT 
            @ClienteId AS ClienteId,
            'Cliente registrado exitosamente' AS Mensaje,
            1 AS Exitoso;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        -- Retornar error
        SELECT 
            0 AS ClienteId,
            ERROR_MESSAGE() AS Mensaje,
            0 AS Exitoso;
    END CATCH
END;
GO

PRINT 'Procedimiento SP_RegistrarClienteCompleto creado'
GO

-- ============================================
-- SP: SP_AutenticarCliente
-- Descripción: Valida credenciales y controla intentos de login
-- Parámetros: Usuario y Clave
-- Retorna: Información del cliente o error
-- ============================================
CREATE PROCEDURE SP_AutenticarCliente
    @Usuario NVARCHAR(50),
    @Clave NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Buscar el cliente
    DECLARE @ClienteId INT;
    DECLARE @ClaveReal NVARCHAR(100);
    DECLARE @CuentaBloqueada BIT;
    DECLARE @IntentosLogin INT;
    
    SELECT 
        @ClienteId = Id,
        @ClaveReal = Clave,
        @CuentaBloqueada = CuentaBloqueada,
        @IntentosLogin = IntentosLogin
    FROM Clientes 
    WHERE Usuario = @Usuario;
    
    -- Cliente no encontrado
    IF @ClienteId IS NULL
    BEGIN
        SELECT 0 AS Exitoso, 'Usuario no encontrado' AS Mensaje;
        RETURN;
    END
    
    -- Cuenta bloqueada
    IF @CuentaBloqueada = 1
    BEGIN
        SELECT 0 AS Exitoso, 'Cuenta bloqueada. Contacte al administrador' AS Mensaje;
        RETURN;
    END
    
    -- Validar clave
    IF @Clave = @ClaveReal
    BEGIN
        -- Login exitoso: Resetear intentos
        UPDATE Clientes 
        SET IntentosLogin = 0, CuentaBloqueada = 0
        WHERE Id = @ClienteId;
        
        -- Retornar información del cliente
        SELECT 
            1 AS Exitoso,
            'Login exitoso' AS Mensaje,
            c.Id,
            c.Identificacion,
            c.Nombre,
            c.Celular,
            c.Usuario
        FROM Clientes c
        WHERE c.Id = @ClienteId;
    END
    ELSE
    BEGIN
        -- Clave incorrecta: Incrementar intentos
        UPDATE Clientes 
        SET IntentosLogin = IntentosLogin + 1,
            CuentaBloqueada = CASE WHEN IntentosLogin + 1 >= 3 THEN 1 ELSE 0 END
        WHERE Id = @ClienteId;
        
        SET @IntentosLogin = @IntentosLogin + 1;
        
        IF @IntentosLogin >= 3
        BEGIN
            SELECT 0 AS Exitoso, 'Cuenta bloqueada por múltiples intentos fallidos' AS Mensaje;
        END
        ELSE
        BEGIN
            SELECT 
                0 AS Exitoso, 
                'Clave incorrecta. Intentos restantes: ' + CAST(3 - @IntentosLogin AS NVARCHAR) AS Mensaje;
        END
    END
END;
GO

PRINT 'Procedimiento SP_AutenticarCliente creado'
GO

-- ============================================
-- SP: SP_ConsignarDinero
-- Descripción: Realiza una consignación en una cuenta
-- Parámetros: CuentaId, Monto, Descripción
-- Retorna: Nuevo saldo
-- Transaccional: Sí
-- ============================================
CREATE PROCEDURE SP_ConsignarDinero
    @CuentaId INT,
    @Monto DECIMAL(18,2),
    @Descripcion NVARCHAR(200) = 'Consignación'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar monto positivo
        IF @Monto <= 0
        BEGIN
            THROW 50010, 'El monto debe ser mayor a cero', 1;
        END
        
        -- Obtener información de la cuenta
        DECLARE @SaldoAnterior DECIMAL(18,2);
        DECLARE @TipoCuenta NVARCHAR(50);
        DECLARE @MontoSobregiro DECIMAL(18,2);
        
        SELECT 
            @SaldoAnterior = Saldo,
            @TipoCuenta = TipoCuenta,
            @MontoSobregiro = ISNULL(MontoSobregiro, 0)
        FROM Cuentas 
        WHERE Id = @CuentaId;
        
        IF @SaldoAnterior IS NULL
        BEGIN
            THROW 50011, 'Cuenta no encontrada', 1;
        END
        
        -- Lógica para Cuenta Corriente: Cubrir sobregiro primero
        IF @TipoCuenta = 'CuentaCorriente' AND @MontoSobregiro > 0
        BEGIN
            IF @Monto >= @MontoSobregiro
            BEGIN
                -- El monto cubre el sobregiro completamente
                DECLARE @Exceso DECIMAL(18,2) = @Monto - @MontoSobregiro;
                UPDATE Cuentas 
                SET Saldo = Saldo + @Exceso, MontoSobregiro = 0
                WHERE Id = @CuentaId;
                
                SET @Descripcion = @Descripcion + ' - Cancelación de sobregiro';
            END
            ELSE
            BEGIN
                -- El monto cubre parcialmente el sobregiro
                UPDATE Cuentas 
                SET MontoSobregiro = MontoSobregiro - @Monto
                WHERE Id = @CuentaId;
                
                SET @Descripcion = @Descripcion + ' - Abono a sobregiro';
            END
        END
        ELSE
        BEGIN
            -- Consignación normal
            UPDATE Cuentas 
            SET Saldo = Saldo + @Monto
            WHERE Id = @CuentaId;
        END
        
        -- Obtener nuevo saldo
        DECLARE @SaldoNuevo DECIMAL(18,2);
        SELECT @SaldoNuevo = Saldo FROM Cuentas WHERE Id = @CuentaId;
        
        -- Registrar movimiento
        INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo)
        VALUES (@CuentaId, 'Consignación', @Monto, @Descripcion, @SaldoAnterior, @SaldoNuevo);
        
        COMMIT TRANSACTION;
        
        -- Retornar resultado
        SELECT 
            1 AS Exitoso,
            'Consignación exitosa' AS Mensaje,
            @SaldoNuevo AS NuevoSaldo;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 
            0 AS Exitoso,
            ERROR_MESSAGE() AS Mensaje,
            NULL AS NuevoSaldo;
    END CATCH
END;
GO

PRINT 'Procedimiento SP_ConsignarDinero creado'
GO

-- ============================================
-- SP: SP_RetirarDinero
-- Descripción: Realiza un retiro considerando reglas por tipo de cuenta
-- Parámetros: CuentaId, Monto, Descripción
-- Retorna: Nuevo saldo
-- Transaccional: Sí
-- ============================================
CREATE PROCEDURE SP_RetirarDinero
    @CuentaId INT,
    @Monto DECIMAL(18,2),
    @Descripcion NVARCHAR(200) = 'Retiro'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar monto positivo
        IF @Monto <= 0
        BEGIN
            THROW 50020, 'El monto debe ser mayor a cero', 1;
        END
        
        -- Obtener información de la cuenta
        DECLARE @SaldoAnterior DECIMAL(18,2);
        DECLARE @TipoCuenta NVARCHAR(50);
        DECLARE @MontoSobregiro DECIMAL(18,2);
        DECLARE @LimiteCredito DECIMAL(18,2);
        
        SELECT 
            @SaldoAnterior = Saldo,
            @TipoCuenta = TipoCuenta,
            @MontoSobregiro = ISNULL(MontoSobregiro, 0),
            @LimiteCredito = ISNULL(LimiteCredito, 0)
        FROM Cuentas 
        WHERE Id = @CuentaId;
        
        IF @SaldoAnterior IS NULL
        BEGIN
            THROW 50021, 'Cuenta no encontrada', 1;
        END
        
        -- Validar según tipo de cuenta
        IF @TipoCuenta = 'CuentaAhorros'
        BEGIN
            -- Cuenta de Ahorros: Calcular intereses antes del retiro
            -- (Simplificado: en producción llamaría a función de intereses)
            IF @Monto > @SaldoAnterior
            BEGIN
                THROW 50022, 'Fondos insuficientes en cuenta de ahorros', 1;
            END
        END
        ELSE IF @TipoCuenta = 'CuentaCorriente'
        BEGIN
            -- Cuenta Corriente: Validar con sobregiro (20%)
            DECLARE @SobregiroMaximo DECIMAL(18,2) = @SaldoAnterior * 0.20;
            DECLARE @LimiteRetiro DECIMAL(18,2) = @SaldoAnterior + @SobregiroMaximo - @MontoSobregiro;
            
            IF @Monto > @LimiteRetiro
            BEGIN
                THROW 50023, 'Monto excede el límite de sobregiro permitido', 1;
            END
            
            -- Aplicar retiro con sobregiro si es necesario
            IF @Monto <= @SaldoAnterior
            BEGIN
                -- Retiro normal
                UPDATE Cuentas SET Saldo = Saldo - @Monto WHERE Id = @CuentaId;
            END
            ELSE
            BEGIN
                -- Usar sobregiro
                DECLARE @MontoSobregiroUsado DECIMAL(18,2) = @Monto - @SaldoAnterior;
                UPDATE Cuentas 
                SET Saldo = 0, MontoSobregiro = MontoSobregiro + @MontoSobregiroUsado
                WHERE Id = @CuentaId;
                
                SET @Descripcion = @Descripcion + ' - Usando sobregiro: $' + CAST(@MontoSobregiroUsado AS NVARCHAR);
            END
        END
        ELSE IF @TipoCuenta = 'TarjetaCredito'
        BEGIN
            -- Tarjeta de Crédito: Validar crédito disponible
            DECLARE @CreditoDisponible DECIMAL(18,2) = @LimiteCredito - ABS(@SaldoAnterior);
            
            IF @Monto > @CreditoDisponible
            BEGIN
                THROW 50024, 'Crédito insuficiente', 1;
            END
            
            -- Usar crédito (saldo negativo = deuda)
            UPDATE Cuentas SET Saldo = Saldo - @Monto WHERE Id = @CuentaId;
        END
        
        -- Obtener nuevo saldo
        DECLARE @SaldoNuevo DECIMAL(18,2);
        SELECT @SaldoNuevo = Saldo FROM Cuentas WHERE Id = @CuentaId;
        
        -- Registrar movimiento
        DECLARE @TipoMovimiento NVARCHAR(50) = CASE 
            WHEN @TipoCuenta = 'TarjetaCredito' THEN 'Compra'
            ELSE 'Retiro'
        END;
        
        INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo)
        VALUES (@CuentaId, @TipoMovimiento, @Monto, @Descripcion, @SaldoAnterior, @SaldoNuevo);
        
        COMMIT TRANSACTION;
        
        -- Retornar resultado
        SELECT 
            1 AS Exitoso,
            'Retiro exitoso' AS Mensaje,
            @SaldoNuevo AS NuevoSaldo;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 
            0 AS Exitoso,
            ERROR_MESSAGE() AS Mensaje,
            NULL AS NuevoSaldo;
    END CATCH
END;
GO

PRINT 'Procedimiento SP_RetirarDinero creado'
GO

-- ============================================
-- SP: SP_TransferirDinero
-- Descripción: Realiza transferencia entre dos cuentas
-- Parámetros: CuentaOrigenId, NumeroCuentaDestino, Monto, Descripción
-- Retorna: Resultado de la operación
-- Transaccional: Sí (ambas operaciones o ninguna)
-- ============================================
CREATE PROCEDURE SP_TransferirDinero
    @CuentaOrigenId INT,
    @NumeroCuentaDestino NVARCHAR(50),
    @Monto DECIMAL(18,2),
    @Descripcion NVARCHAR(200) = 'Transferencia'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validar monto positivo
        IF @Monto <= 0
        BEGIN
            THROW 50030, 'El monto debe ser mayor a cero', 1;
        END
        
        -- Obtener ID de cuenta destino
        DECLARE @CuentaDestinoId INT;
        SELECT @CuentaDestinoId = Id FROM Cuentas WHERE NumeroCuenta = @NumeroCuentaDestino;
        
        IF @CuentaDestinoId IS NULL
        BEGIN
            THROW 50031, 'Cuenta destino no encontrada', 1;
        END
        
        -- Validar que no sea la misma cuenta
        IF @CuentaOrigenId = @CuentaDestinoId
        BEGIN
            THROW 50032, 'No puede transferir a la misma cuenta', 1;
        END
        
        -- Realizar retiro de cuenta origen
        DECLARE @ResultadoRetiro TABLE (Exitoso BIT, Mensaje NVARCHAR(200), NuevoSaldo DECIMAL(18,2));
        INSERT INTO @ResultadoRetiro
        EXEC SP_RetirarDinero @CuentaOrigenId, @Monto, 'Transferencia enviada';
        
        DECLARE @ExitosoRetiro BIT;
        SELECT @ExitosoRetiro = Exitoso FROM @ResultadoRetiro;
        
        IF @ExitosoRetiro = 0
        BEGIN
            THROW 50033, 'Error al retirar de cuenta origen', 1;
        END
        
        -- Realizar consignación en cuenta destino
        DECLARE @ResultadoConsignacion TABLE (Exitoso BIT, Mensaje NVARCHAR(200), NuevoSaldo DECIMAL(18,2));
        INSERT INTO @ResultadoConsignacion
        EXEC SP_ConsignarDinero @CuentaDestinoId, @Monto, 'Transferencia recibida';
        
        COMMIT TRANSACTION;
        
        -- Retornar resultado exitoso
        SELECT 
            1 AS Exitoso,
            'Transferencia exitosa' AS Mensaje,
            @Monto AS MontoTransferido;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 
            0 AS Exitoso,
            ERROR_MESSAGE() AS Mensaje,
            0 AS MontoTransferido;
    END CATCH
END;
GO

PRINT 'Procedimiento SP_TransferirDinero creado'
GO

-- ============================================
-- SP: SP_ComprarEnCuotas
-- Descripción: Realiza compra en cuotas con tarjeta de crédito
-- Lógica de intereses:
--   ≤2 cuotas: 0% interés
--   ≤6 cuotas: 1.9% mensual
--   ≥7 cuotas: 2.3% mensual
-- ============================================
CREATE PROCEDURE SP_ComprarEnCuotas
    @CuentaId INT,
    @Monto DECIMAL(18,2),
    @NumeroCuotas INT,
    @Descripcion NVARCHAR(200) = 'Compra en cuotas'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- Validaciones básicas
        IF @Monto <= 0 OR @NumeroCuotas <= 0
        BEGIN
            THROW 50040, 'Monto y número de cuotas deben ser mayores a cero', 1;
        END
        
        -- Verificar que sea tarjeta de crédito
        DECLARE @TipoCuenta NVARCHAR(50);
        DECLARE @SaldoAnterior DECIMAL(18,2);
        DECLARE @LimiteCredito DECIMAL(18,2);
        
        SELECT 
            @TipoCuenta = TipoCuenta,
            @SaldoAnterior = Saldo,
            @LimiteCredito = LimiteCredito
        FROM Cuentas 
        WHERE Id = @CuentaId;
        
        IF @TipoCuenta != 'TarjetaCredito'
        BEGIN
            THROW 50041, 'Esta operación solo está disponible para tarjetas de crédito', 1;
        END
        
        -- Calcular tasa de interés según cuotas
        DECLARE @TasaInteres DECIMAL(5,4);
        IF @NumeroCuotas <= 2
            SET @TasaInteres = 0.00;      -- Sin interés
        ELSE IF @NumeroCuotas <= 6
            SET @TasaInteres = 0.019;     -- 1.9% mensual
        ELSE
            SET @TasaInteres = 0.023;     -- 2.3% mensual
        
        -- Calcular monto total con intereses compuestos
        DECLARE @MontoTotal DECIMAL(18,2);
        IF @TasaInteres > 0
            SET @MontoTotal = @Monto * POWER(1 + @TasaInteres, @NumeroCuotas);
        ELSE
            SET @MontoTotal = @Monto;
        
        -- Calcular pago mensual
        DECLARE @PagoMensual DECIMAL(18,2) = @MontoTotal / @NumeroCuotas;
        
        -- Validar crédito disponible
        DECLARE @CreditoDisponible DECIMAL(18,2) = @LimiteCredito - ABS(@SaldoAnterior);
        IF @MontoTotal > @CreditoDisponible
        BEGIN
            THROW 50042, 'Crédito insuficiente para esta compra', 1;
        END
        
        -- Aplicar el cargo
        UPDATE Cuentas 
        SET Saldo = Saldo - @MontoTotal
        WHERE Id = @CuentaId;
        
        -- Obtener nuevo saldo
        DECLARE @SaldoNuevo DECIMAL(18,2);
        SELECT @SaldoNuevo = Saldo FROM Cuentas WHERE Id = @CuentaId;
        
        -- Construir descripción detallada
        DECLARE @DescripcionCompleta NVARCHAR(200);
        SET @DescripcionCompleta = @Descripcion + ' - ' + CAST(@NumeroCuotas AS NVARCHAR) + ' cuotas - Pago mensual: $' + 
            CAST(CAST(@PagoMensual AS DECIMAL(18,2)) AS NVARCHAR);
        
        IF @TasaInteres > 0
            SET @DescripcionCompleta = @DescripcionCompleta + ' - Interés: ' + 
                CAST(CAST(@TasaInteres * 100 AS DECIMAL(5,2)) AS NVARCHAR) + '%';
        
        -- Registrar movimiento
        INSERT INTO Movimientos (CuentaId, Tipo, Monto, Descripcion, SaldoAnterior, SaldoNuevo)
        VALUES (@CuentaId, 'Compra en cuotas', @MontoTotal, @DescripcionCompleta, @SaldoAnterior, @SaldoNuevo);
        
        COMMIT TRANSACTION;
        
        -- Retornar resultado
        SELECT 
            1 AS Exitoso,
            'Compra en cuotas exitosa' AS Mensaje,
            @PagoMensual AS PagoMensual,
            @MontoTotal AS MontoTotal,
            @NumeroCuotas AS NumeroCuotas,
            @CreditoDisponible - @MontoTotal AS CreditoDisponible;
            
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;
            
        SELECT 
            0 AS Exitoso,
            ERROR_MESSAGE() AS Mensaje,
            NULL AS PagoMensual,
            NULL AS MontoTotal,
            NULL AS NumeroCuotas,
            NULL AS CreditoDisponible;
    END CATCH
END;
GO

PRINT 'Procedimiento SP_ComprarEnCuotas creado'
GO

PRINT '==========================================='
PRINT 'Creando funciones de cálculo...'
PRINT '==========================================='
GO

-- ============================================
-- FUNCIÓN: FN_CalcularInteresesAhorros
-- Descripción: Calcula intereses de cuenta de ahorros (1.5% mensual)
-- Parámetros: Saldo actual, Fecha último cálculo
-- Retorna: Monto de intereses generados
-- ============================================
CREATE FUNCTION FN_CalcularInteresesAhorros
(
    @Saldo DECIMAL(18,2),
    @FechaUltimoCalculo DATETIME
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Interes DECIMAL(18,2) = 0;
    DECLARE @MesesTranscurridos INT;
    DECLARE @TasaMensual DECIMAL(5,4) = 0.015; -- 1.5% mensual
    
    -- Calcular meses transcurridos
    SET @MesesTranscurridos = DATEDIFF(MONTH, @FechaUltimoCalculo, GETDATE());
    
    -- Si hay meses transcurridos y hay saldo
    IF @MesesTranscurridos > 0 AND @Saldo > 0
    BEGIN
        -- Fórmula de interés compuesto: Saldo * ((1 + tasa)^meses - 1)
        SET @Interes = @Saldo * (POWER(1 + @TasaMensual, @MesesTranscurridos) - 1);
    END
    
    RETURN ROUND(@Interes, 2);
END;
GO

PRINT 'Función FN_CalcularInteresesAhorros creada'
GO

-- ============================================
-- FUNCIÓN: FN_CalcularPagoMensualTC
-- Descripción: Calcula el pago mensual para compras en cuotas
-- Parámetros: Monto, Número de cuotas
-- Retorna: Pago mensual con intereses incluidos
-- ============================================
CREATE FUNCTION FN_CalcularPagoMensualTC
(
    @Monto DECIMAL(18,2),
    @NumeroCuotas INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @PagoMensual DECIMAL(18,2);
    DECLARE @TasaInteres DECIMAL(5,4);
    DECLARE @MontoTotal DECIMAL(18,2);
    
    -- Validación
    IF @NumeroCuotas <= 0
        RETURN 0;
    
    -- Determinar tasa de interés según número de cuotas
    IF @NumeroCuotas <= 2
        SET @TasaInteres = 0.00;      -- 0% interés
    ELSE IF @NumeroCuotas <= 6
        SET @TasaInteres = 0.019;     -- 1.9% mensual
    ELSE
        SET @TasaInteres = 0.023;     -- 2.3% mensual
    
    -- Calcular monto total con intereses
    IF @TasaInteres > 0
        SET @MontoTotal = @Monto * POWER(1 + @TasaInteres, @NumeroCuotas);
    ELSE
        SET @MontoTotal = @Monto;
    
    -- Calcular pago mensual
    SET @PagoMensual = @MontoTotal / @NumeroCuotas;
    
    RETURN ROUND(@PagoMensual, 2);
END;
GO

PRINT 'Función FN_CalcularPagoMensualTC creada'
GO

-- ============================================
-- FUNCIÓN: FN_ObtenerSobregiroDisponible
-- Descripción: Calcula el sobregiro disponible en cuenta corriente
-- Parámetros: CuentaId
-- Retorna: Monto disponible de sobregiro
-- ============================================
CREATE FUNCTION FN_ObtenerSobregiroDisponible
(
    @CuentaId INT
)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @Resultado DECIMAL(18,2) = 0;
    DECLARE @Saldo DECIMAL(18,2);
    DECLARE @MontoSobregiro DECIMAL(18,2);
    DECLARE @TipoCuenta NVARCHAR(50);
    
    -- Obtener información de la cuenta
    SELECT 
        @Saldo = Saldo,
        @MontoSobregiro = ISNULL(MontoSobregiro, 0),
        @TipoCuenta = TipoCuenta
    FROM Cuentas
    WHERE Id = @CuentaId;
    
    -- Solo aplica para cuentas corrientes
    IF @TipoCuenta = 'CuentaCorriente'
    BEGIN
        -- Sobregiro máximo permitido: 20% del saldo
        DECLARE @SobregiroMaximo DECIMAL(18,2) = @Saldo * 0.20;
        -- Disponible = Máximo - Usado
        SET @Resultado = @SobregiroMaximo - @MontoSobregiro;
        IF @Resultado < 0
            SET @Resultado = 0;
    END
    
    RETURN ROUND(@Resultado, 2);
END;
GO

PRINT 'Función FN_ObtenerSobregiroDisponible creada'
GO

PRINT '==========================================='
PRINT 'Insertando datos de prueba...'
PRINT '==========================================='
GO

-- Insertar cliente de prueba usando el procedimiento almacenado
EXEC SP_RegistrarClienteCompleto
    @Identificacion = '12345678',
    @Nombre = 'Juan Pérez',
    @Celular = '3001234567',
    @Usuario = 'juan.perez',
    @Clave = '123456';
GO

-- Obtener IDs de las cuentas del cliente de prueba
DECLARE @ClienteId INT = (SELECT Id FROM Clientes WHERE Usuario = 'juan.perez');
DECLARE @CuentaAhorrosId INT = (SELECT Id FROM Cuentas WHERE ClienteId = @ClienteId AND TipoCuenta = 'CuentaAhorros');
DECLARE @CuentaCorrienteId INT = (SELECT Id FROM Cuentas WHERE ClienteId = @ClienteId AND TipoCuenta = 'CuentaCorriente');

-- Consignar saldo inicial en cuenta de ahorros ($1,000,000)
EXEC SP_ConsignarDinero @CuentaAhorrosId, 1000000, 'Saldo inicial';

-- Consignar saldo inicial en cuenta corriente ($500,000)
EXEC SP_ConsignarDinero @CuentaCorrienteId, 500000, 'Saldo inicial';

PRINT 'Datos de prueba insertados exitosamente'
GO

PRINT '==========================================='
PRINT 'BASE DE DATOS CREADA EXITOSAMENTE'
PRINT '==========================================='
GO

PRINT ''
PRINT '📊 RESUMEN DE LA BASE DE DATOS:'
PRINT '================================'
PRINT ''
PRINT '✅ Tablas creadas: 3'
PRINT '   - Clientes (usuarios del sistema)'
PRINT '   - Cuentas (herencia TPH: Ahorros, Corriente, Crédito)'
PRINT '   - Movimientos (historial de transacciones)'
PRINT ''
PRINT '✅ Índices creados: 8 (optimización de consultas)'
PRINT ''
PRINT '✅ Vistas creadas: 3'
PRINT '   - VW_ResumenClientes (dashboard de clientes)'
PRINT '   - VW_HistorialMovimientos (auditoría completa)'
PRINT '   - VW_EstadoCuentas (estado detallado de cuentas)'
PRINT ''
PRINT '✅ Procedimientos almacenados: 6'
PRINT '   - SP_RegistrarClienteCompleto'
PRINT '   - SP_AutenticarCliente'
PRINT '   - SP_ConsignarDinero'
PRINT '   - SP_RetirarDinero'
PRINT '   - SP_TransferirDinero'
PRINT '   - SP_ComprarEnCuotas'
PRINT ''
PRINT '✅ Funciones: 3'
PRINT '   - FN_CalcularInteresesAhorros'
PRINT '   - FN_CalcularPagoMensualTC'
PRINT '   - FN_ObtenerSobregiroDisponible'
PRINT ''
PRINT '✅ Datos de prueba insertados:'
PRINT '   Usuario: juan.perez'
PRINT '   Clave: 123456'
PRINT '   Cuenta Ahorros: $1,000,000'
PRINT '   Cuenta Corriente: $500,000'
PRINT '   Tarjeta Crédito: Límite $1,000,000'
PRINT ''
PRINT '================================'
PRINT 'La base de datos está lista para usar'
PRINT '================================'
GO

-- Consultas de verificación
SELECT 'CLIENTES REGISTRADOS' AS Tabla, COUNT(*) AS Total FROM Clientes
UNION ALL
SELECT 'CUENTAS CREADAS', COUNT(*) FROM Cuentas
UNION ALL
SELECT 'MOVIMIENTOS', COUNT(*) FROM Movimientos;
GO
