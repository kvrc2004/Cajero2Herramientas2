# 🔄 Compartir Datos entre Colaboradores - MiBanco

## ⚠️ Problema Identificado

No es posible subir directamente los archivos `.mdf` y `.ldf` de SQL Server a Git porque:
- ❌ Son archivos binarios muy grandes
- ❌ Cambian constantemente con cada transacción
- ❌ Causarían conflictos de merge imposibles de resolver
- ❌ Git no está diseñado para manejar bases de datos

## ✅ Soluciones Recomendadas

### 🥇 Opción 1: Scripts de Migración (RECOMENDADO para Desarrollo)

Crear scripts SQL incrementales que generen los datos necesarios.

#### Implementación:

1. **Crear carpeta de migraciones**

```
Database/
├── CreateDatabase_MiPlata.sql (estructura)
├── Migrations/
│   ├── 001_DatosIniciales.sql
│   ├── 002_ClientesDemo.sql
│   └── 003_TransaccionesEjemplo.sql
```

2. **Ejemplo de script de migración:**

```sql
-- Database/Migrations/001_DatosIniciales.sql
USE MiPlataDB;
GO

-- Cliente de prueba 1
EXEC SP_RegistrarClienteCompleto
    @Identificacion = '12345678',
    @Nombre = 'Juan Pérez',
    @Celular = '3001234567',
    @Usuario = 'juan.perez',
    @Clave = '123456';

-- Cliente de prueba 2
EXEC SP_RegistrarClienteCompleto
    @Identificacion = '87654321',
    @Nombre = 'María García',
    @Celular = '3109876543',
    @Usuario = 'maria.garcia',
    @Clave = '123456';

-- Agregar saldos iniciales
DECLARE @CuentaAhorros1 INT = (SELECT Id FROM Cuentas WHERE NumeroCuenta LIKE 'AH%' AND ClienteId = 1);
EXEC SP_ConsignarDinero @CuentaAhorros1, 1000000, 'Saldo inicial';
GO

PRINT 'Datos iniciales insertados correctamente';
```

3. **Documentar el proceso:**

Cada desarrollador ejecuta los scripts en orden después de crear la BD.

**✅ Ventajas:**
- Control de versiones de los datos
- Reproducible en cualquier entorno
- Fácil de revisar en pull requests
- No hay conflictos de merge

**❌ Desventajas:**
- Requiere ejecutar scripts manualmente
- No comparte datos en tiempo real

---

### 🥈 Opción 2: Export/Import de Datos (Para Snapshots)

Exportar e importar datos específicos cuando sea necesario.

#### Script para Exportar Datos:

```sql
-- Database/Scripts/ExportarDatos.sql
USE MiPlataDB;
GO

-- Generar INSERT statements para datos actuales
SELECT 
    'EXEC SP_RegistrarClienteCompleto ' +
    '@Identificacion = ''' + Identificacion + ''', ' +
    '@Nombre = ''' + Nombre + ''', ' +
    '@Celular = ''' + Celular + ''', ' +
    '@Usuario = ''' + Usuario + ''', ' +
    '@Clave = ''' + Clave + ''';'
FROM Clientes;

-- Generar consignaciones
SELECT 
    'DECLARE @CuentaId' + CAST(c.Id AS VARCHAR) + ' INT = (SELECT Id FROM Cuentas WHERE NumeroCuenta = ''' + c.NumeroCuenta + '''); ' +
    'EXEC SP_ConsignarDinero @CuentaId' + CAST(c.Id AS VARCHAR) + ', ' + CAST(c.Saldo AS VARCHAR) + ', ''Carga de datos'';'
FROM Cuentas c
WHERE c.Saldo > 0;
```

#### Uso:

```bash
# Exportar datos actuales a un archivo
sqlcmd -S localhost -E -d MiPlataDB -i "Database\Scripts\ExportarDatos.sql" -o "Database\Migrations\snapshot_$(date).sql"
```

**✅ Ventajas:**
- Captura estado exacto de la BD
- Útil para pruebas

**❌ Desventajas:**
- Manual
- No es en tiempo real

---

### 🥉 Opción 3: Base de Datos Compartida en la Nube (MEJOR para Colaboración Real)

Usar una base de datos central a la que todos se conecten.

#### Opciones de Base de Datos Compartida:

#### A. Azure SQL Database (Gratis con cuenta de estudiante)

**Configuración:**

1. Crear Azure SQL Database gratuito
2. Ejecutar el script de creación en Azure
3. Cada desarrollador se conecta a la misma BD

**Cadena de conexión:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=mibanco-server.database.windows.net;Database=MiPlataDB;User Id=admin;Password=tu_password;TrustServerCertificate=True;"
  }
}
```

**✅ Ventajas:**
- ✅ Datos compartidos en tiempo real
- ✅ Todos ven los mismos cambios
- ✅ No hay sincronización manual
- ✅ Gratis para estudiantes

**❌ Desventajas:**
- Requiere conexión a internet
- Configuración inicial más compleja

#### B. SQL Server en PC de un Compañero

Configurar SQL Server para aceptar conexiones remotas.

**En el PC "servidor" (uno del equipo):**

1. Habilitar TCP/IP en SQL Server Configuration Manager
2. Abrir puerto 1433 en el firewall
3. Configurar autenticación SQL

**Cadena de conexión para otros:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=192.168.1.100,1433;Database=MiPlataDB;User Id=dev;Password=dev123;TrustServerCertificate=True;"
  }
}
```

**✅ Ventajas:**
- No requiere servicios externos
- Gratis

**❌ Desventajas:**
- El PC "servidor" debe estar encendido
- Requiere estar en la misma red (o VPN)
- Configuración de firewall

---

### 🏆 Opción 4: JSON Seed Data (Intermedio - Recomendado)

Usar archivos JSON para datos de prueba que se cargan automáticamente.

#### Implementación:

<function_calls>
<invoke name="read_file">
<parameter name="filePath">c:\Users\david\OneDrive\Documentos\Herramientas 2\MiBanco\Data\MiBancoDbContext.cs