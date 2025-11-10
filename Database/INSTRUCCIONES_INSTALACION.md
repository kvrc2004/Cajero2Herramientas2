# 🗄️ Instalación de Base de Datos - MiBanco

## Requisitos Previos

- **SQL Server 2019 o superior** (Express, Developer o Enterprise)
- **SQL Server Management Studio (SSMS)** 18.0 o superior
- Windows con permisos de administrador

## 📦 Instrucciones de Instalación

### Paso 1: Clonar el Repositorio

```bash
git clone https://github.com/kvrc2004/Cajero2Herramientas2.git
cd Cajero2Herramientas2
```

### Paso 2: Configurar SQL Server

1. Abre **SQL Server Management Studio (SSMS)**
2. Conéctate a tu instancia local:
   - **Server name**: `localhost` o `(local)` o `.\SQLEXPRESS` (si usas SQL Express)
   - **Authentication**: Windows Authentication (recomendado)
3. Asegúrate de que el servicio SQL Server esté corriendo

### Paso 3: Ejecutar el Script de Base de Datos

#### Opción A: Desde SSMS (Recomendado)

1. En SSMS, abre el archivo: `Database/CreateDatabase_MiPlata.sql`
2. Presiona **F5** o haz clic en **Execute**
3. Espera a que termine (verás mensajes de confirmación en verde)

#### Opción B: Desde Línea de Comandos

```cmd
sqlcmd -S localhost -E -i "Database\CreateDatabase_MiPlata.sql"
```

### Paso 4: Verificar la Instalación

Ejecuta esta consulta en SSMS para verificar:

```sql
USE MiPlataDB;
GO

-- Verificar tablas
SELECT 'CLIENTES' AS Tabla, COUNT(*) AS Total FROM Clientes
UNION ALL
SELECT 'CUENTAS', COUNT(*) FROM Cuentas
UNION ALL
SELECT 'MOVIMIENTOS', COUNT(*) FROM Movimientos;
GO

-- Verificar datos de prueba
SELECT * FROM Clientes;
SELECT * FROM Cuentas;
```

Deberías ver:
- ✅ 1 cliente registrado (juan.perez)
- ✅ 3 cuentas creadas (Ahorros, Corriente, Tarjeta)
- ✅ Movimientos iniciales

### Paso 5: Configurar la Aplicación

1. Abre el archivo `appsettings.json` en el proyecto
2. Ajusta la cadena de conexión según tu configuración:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiPlataDB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Variaciones comunes:**

- SQL Express: `Server=localhost\\SQLEXPRESS;...`
- SQL Server con autenticación SQL: `Server=localhost;Database=MiPlataDB;User Id=tu_usuario;Password=tu_contraseña;...`
- Instancia específica: `Server=NOMBRE_PC\\INSTANCIA;...`

### Paso 6: Ejecutar la Aplicación

```cmd
cd MiBanco
dotnet restore
dotnet run
```

La aplicación estará disponible en: `https://localhost:5001`

## 👤 Usuario de Prueba

Credenciales para iniciar sesión:

- **Usuario**: `juan.perez`
- **Contraseña**: `123456`

## 🔧 Solución de Problemas Comunes

### Error: "Cannot open database MiPlataDB"

**Solución**: Ejecuta el script completo desde el inicio. El script elimina la BD si existe y la recrea.

### Error: "A network-related or instance-specific error"

**Soluciones**:
1. Verifica que SQL Server esté corriendo:
   - Abre **SQL Server Configuration Manager**
   - Ve a **SQL Server Services**
   - Asegúrate de que **SQL Server (MSSQLSERVER)** esté iniciado

2. Verifica el nombre del servidor:
   ```cmd
   sqlcmd -L
   ```
   Esto lista todos los servidores SQL disponibles

### Error: "Login failed for user"

**Solución**: Si usas autenticación SQL, asegúrate de:
1. Habilitar SQL Server Authentication en SSMS
2. Crear un usuario con permisos suficientes
3. Actualizar la cadena de conexión con las credenciales correctas

### La aplicación no conecta a la base de datos

**Solución**:
1. Verifica que `appsettings.json` tenga la cadena de conexión correcta
2. Prueba la conexión directamente desde SSMS
3. Revisa que `TrustServerCertificate=True` esté presente

## 📊 Estructura de la Base de Datos

El script crea automáticamente:

### Tablas (3)
- **Clientes**: Usuarios del sistema
- **Cuentas**: Implementa herencia TPH (Ahorros, Corriente, Crédito)
- **Movimientos**: Historial de transacciones

### Procedimientos Almacenados (6)
- `SP_RegistrarClienteCompleto`: Registra cliente con 3 cuentas
- `SP_AutenticarCliente`: Login con control de intentos
- `SP_ConsignarDinero`: Realiza consignaciones
- `SP_RetirarDinero`: Realiza retiros con validaciones
- `SP_TransferirDinero`: Transferencias entre cuentas
- `SP_ComprarEnCuotas`: Compras en cuotas con intereses

### Vistas (3)
- `VW_ResumenClientes`: Dashboard de clientes
- `VW_HistorialMovimientos`: Auditoría completa
- `VW_EstadoCuentas`: Estado detallado de cuentas

### Funciones (3)
- `FN_CalcularInteresesAhorros`: Calcula intereses 1.5% mensual
- `FN_CalcularPagoMensualTC`: Calcula cuotas con intereses
- `FN_ObtenerSobregiroDisponible`: Sobregiro disponible (20%)

## 🔄 Resetear la Base de Datos

Si necesitas empezar desde cero:

1. Ejecuta el script completo nuevamente - automáticamente elimina y recrea la BD
2. O ejecuta manualmente:

```sql
USE master;
GO

ALTER DATABASE MiPlataDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE MiPlataDB;
GO
```

Luego ejecuta `CreateDatabase_MiPlata.sql` de nuevo.

## 🤝 Colaboradores

Si encuentras algún problema durante la instalación:

1. Revisa que tengas todos los requisitos previos
2. Consulta la sección de **Solución de Problemas**
3. Abre un **issue** en GitHub con detalles del error

## 📝 Notas Importantes

- ⚠️ El script usa `Trusted_Connection=True` (Windows Authentication) por defecto
- ⚠️ SQL Server debe estar configurado para aceptar conexiones locales
- ⚠️ La base de datos se crea en la ubicación predeterminada de SQL Server
- ✅ El script es **idempotente**: puedes ejecutarlo múltiples veces sin problemas
- ✅ Incluye **datos de prueba** para comenzar a trabajar inmediatamente

## 📞 Soporte

Para más información, consulta:
- `Database/EXPLICACION_BASE_DATOS.md` - Detalles técnicos de la BD
- `CONFIGURACION_COMPLETADA.md` - Configuración completa del proyecto
- `README.md` - Documentación general del proyecto
