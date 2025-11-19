# 🏦 Mi Plata - Sistema Bancario

## 📋 Descripción del Proyecto

**Mi Plata** es un sistema bancario completo desarrollado en C# ASP.NET Core Razor Pages que simula las operaciones de un cajero automático. El proyecto implementa los conceptos fundamentales de Programación Orientada a Objetos (POO) incluyendo herencia, polimorfismo, encapsulación y abstracción.

**Integrantes:** Kevin Romero Cano - Juan David Rosero

---

## 🎯 Características Principales

### 💰 Tipos de Cuentas
- **Cuenta de Ahorros**: Genera 1.5% de interés mensual
- **Cuenta Corriente**: Permite sobregiro del 20%
- **Tarjeta de Crédito**: Sistema de cuotas con intereses variables

### 🔐 Seguridad
- Sistema de autenticación robusto
- Límite de 3 intentos de login
- Bloqueo automático de cuentas
- Validación completa de datos

### 💻 Funcionalidades
- ✅ Registro y autenticación de usuarios
- ✅ Consultar saldo y movimientos
- ✅ Consignar y retirar dinero
- ✅ Transferencias entre cuentas
- ✅ Compras en cuotas con tarjeta de crédito
- ✅ Gestión de perfil de usuario

---

## 🚀 Instrucciones de Instalación y Ejecución

### Prerrequisitos
- .NET 9.0 SDK o superior
- SQL Server 2019+ (Express, Developer o Enterprise)
- SQL Server Management Studio (SSMS) 18.0+
- Visual Studio 2022 o Visual Studio Code
- Git (opcional)

### Pasos para Ejecutar

#### 1️⃣ Clonar el Repositorio
```bash
# Si tienes Git instalado
git clone https://github.com/kvrc2004/Cajero2Herramientas2.git
cd Cajero2Herramientas2

# O simplemente descargar y extraer el ZIP
```

#### 2️⃣ Configurar la Base de Datos

**📖 [Instrucciones Completas de Instalación de BD](Database/INSTRUCCIONES_INSTALACION.md)**

**Resumen Rápido:**

1. Abre **SQL Server Management Studio**
2. Conéctate a tu servidor local (`localhost` o `.\SQLEXPRESS`)
3. Abre el archivo: `Database/CreateDatabase_MiPlata.sql`
4. Presiona **F5** para ejecutar
5. Verifica que se creó la base de datos **MiPlataDB**

**Desde CMD (alternativa):**
```cmd
sqlcmd -S localhost -E -i "Database\CreateDatabase_MiPlata.sql"
```

#### 3️⃣ Configurar la Conexión

Abre `appsettings.json` y ajusta según tu configuración de SQL Server:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MiPlataDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Variaciones comunes:**
- SQL Express: `Server=localhost\\SQLEXPRESS;...`
- Autenticación SQL: `Server=localhost;Database=MiPlataDB;User Id=tu_usuario;Password=tu_pass;...`

#### 4️⃣ Restaurar Dependencias
```bash
cd MiBanco
dotnet restore
```

#### 5️⃣ Compilar el Proyecto
```bash
dotnet build
```

#### 6️⃣ Ejecutar la Aplicación
```bash
dotnet run
```

#### 7️⃣ Acceder a la Aplicación
- Abre el navegador web
- Navega a: `https://localhost:5001` o el puerto indicado en la terminal
- ¡Listo para usar!

---

## 👤 Datos de Prueba

### Usuario de Prueba
Para probar inmediatamente el sistema, puedes usar:

- **Usuario**: `juan.perez`
- **Clave**: `123456`

### Cuentas Incluidas
El usuario de prueba tiene:
- **Cuenta de Ahorros**: $1,000,000 COP
- **Cuenta Corriente**: $500,000 COP
- **Tarjeta de Crédito**: Límite de $2,000,000 COP

---

## 🏗️ Arquitectura del Proyecto

### Estructura POO Implementada

```
📦 Modelos (Models/)
├── 👤 Cliente.cs - Clase principal de usuarios
├── 🏦 Cuenta.cs - Clase abstracta base (HERENCIA)
├── 💰 CuentaAhorros.cs - Hereda de Cuenta (POLIMORFISMO)
├── 💳 CuentaCorriente.cs - Hereda de Cuenta (POLIMORFISMO)
├── 💎 TarjetaCredito.cs - Hereda de Cuenta (POLIMORFISMO)
└── 📄 Movimiento.cs - Historial de transacciones

📦 Servicios (Services/)
└── 🔧 BancoService.cs - Patrón Singleton para lógica de negocio

📦 Páginas (Pages/)
├── 🏠 Index.cshtml - Página principal
├── 🔐 Login.cshtml - Inicio de sesión
├── 📝 Registro.cshtml - Registro de usuarios
├── 💼 Transacciones.cshtml - Operaciones bancarias
└── 👤 Perfil.cshtml - Gestión de perfil
```

### Conceptos POO Demostrados

1. **HERENCIA** 🧬
   - `CuentaAhorros`, `CuentaCorriente` y `TarjetaCredito` heredan de `Cuenta`
   - Reutilización de código y estructura común

2. **POLIMORFISMO** 🎭
   - Métodos `Consignar()` y `Retirar()` implementados de forma diferente en cada tipo de cuenta
   - Comportamiento específico según el tipo de cuenta

3. **ENCAPSULACIÓN** 🔒
   - Propiedades privadas y públicas apropiadas
   - Métodos de acceso controlado

4. **ABSTRACCIÓN** 🎨
   - Clase `Cuenta` como plantilla abstracta
   - Definición de contratos sin implementación

---

## 💡 Reglas de Negocio Implementadas

### 💰 Cuenta de Ahorros
- **Interés**: 1.5% mensual aplicado automáticamente al retirar
- **Propósito**: Ahorrar dinero de forma segura
- **Cálculo**: Interés compuesto por meses transcurridos

### 💳 Cuenta Corriente  
- **Sobregiro**: 20% del saldo actual
- **Sin intereses**: No genera rentabilidad
- **Flexibilidad**: Transacciones ilimitadas

### 💎 Tarjeta de Crédito
- **≤ 2 cuotas**: 0% de interés
- **3-6 cuotas**: 1.9% mensual
- **≥ 7 cuotas**: 2.3% mensual
- **Simulador**: Cálculo en tiempo real del pago mensual

---

## 🛠️ Tecnologías Utilizadas

- **Backend**: C# ASP.NET Core 9.0
- **Frontend**: Razor Pages, Bootstrap 5, jQuery
- **Base de Datos**: SQL Server 2019+ con Entity Framework Core
- **ORM**: Entity Framework Core 9.0
- **Estilos**: CSS3 personalizado, Font Awesome
- **Arquitectura**: Patrón Repository + Service Layer
- **Validación**: Data Annotations + JavaScript

### 🗄️ Características de la Base de Datos

- **3 Tablas**: Clientes, Cuentas (TPH - Table Per Hierarchy), Movimientos
- **6 Procedimientos Almacenados**: Operaciones bancarias optimizadas
- **3 Vistas**: Consultas consolidadas de información
- **3 Funciones**: Cálculos de intereses y sobregiros
- **Índices optimizados**: Para búsquedas rápidas
- **Transaccionalidad**: Operaciones ACID garantizadas

---

## 📱 Características de la Interfaz

### ✨ Diseño Responsivo
- Adaptable a móviles, tablets y desktop
- Interfaz moderna y profesional
- Animaciones suaves y feedback visual

### 🎨 Experiencia de Usuario
- Navegación intuitiva
- Mensajes de confirmación claros
- Operaciones AJAX para mejor rendimiento
- Simulador de cuotas en tiempo real

---

## 📊 Funcionalidades Destacadas

### 🔍 Búsqueda Inteligente
- Búsqueda de cuentas para transferencias
- Validación en tiempo real
- Información del destinatario

### 📈 Simulador de Cuotas
- Cálculo automático de intereses
- Vista previa del pago mensual
- Comparación de opciones

### 📋 Historial Completo
- Registro detallado de todas las transacciones
- Filtros por tipo de cuenta
- Información de saldos anterior y nuevo

---

## 🔧 Configuración de Desarrollo

### Variables de Entorno
El proyecto usa configuración por defecto de ASP.NET Core. No requiere configuración adicional.

### Base de Datos
El sistema utiliza **SQL Server** con Entity Framework Core. La base de datos se crea automáticamente ejecutando el script `Database/CreateDatabase_MiPlata.sql`.

**Características:**
- Persistencia de datos entre sesiones
- Transacciones ACID
- Procedimientos almacenados optimizados
- Consultas eficientes con índices

Ver [Instrucciones de Instalación de BD](Database/INSTRUCCIONES_INSTALACION.md) para más detalles.

### Logs
Los errores se muestran en la interfaz. Para desarrollo, revisar la consola del navegador.

---

## 📚 Documentación Adicional

Toda la documentación del proyecto está organizada en la carpeta **`Documentacion/`**:

### 🗂️ Documentación Base de Datos
- **📋 [INSTRUCCIONES_INSTALACION.md](Documentacion/INSTRUCCIONES_INSTALACION.md)**: Guía completa para configurar la base de datos
- **📊 [EXPLICACION_BASE_DATOS.md](Documentacion/EXPLICACION_BASE_DATOS.md)**: Detalles técnicos de la estructura de BD
- **💾 [COMPARTIR_DATOS.md](Documentacion/COMPARTIR_DATOS.md)**: Cómo compartir datos entre desarrolladores
- **📝 [CAMBIOS_BASE_DATOS.md](Documentacion/CAMBIOS_BASE_DATOS.md)**: Registro completo de cambios en la BD

### 📋 Documentación del Proyecto
- **✅ [CHECKLIST_COMPLETO.md](Documentacion/CHECKLIST_COMPLETO.md)**: Lista completa de requisitos cumplidos
- **✅ [CHECKLIST_PROYECTO_FINAL.md](Documentacion/CHECKLIST_PROYECTO_FINAL.md)**: Evaluación final del proyecto
- **🏗️ [DiagramaUML.md](Documentacion/DiagramaUML.md)**: Diagrama de clases y arquitectura POO
- **⚙️ [CONFIGURACION_COMPLETADA.md](Documentacion/CONFIGURACION_COMPLETADA.md)**: Documentación de configuración

### 📊 Reportes y Resúmenes
- **📈 [RESUMEN_FINAL_PROYECTO.md](Documentacion/RESUMEN_FINAL_PROYECTO.md)**: Resumen ejecutivo del proyecto
- **📄 [REPORTE_TRABAJO_COMPLETADO.md](Documentacion/REPORTE_TRABAJO_COMPLETADO.md)**: Reporte detallado de trabajo
- **🤝 [GUIA_COLABORACION.md](Documentacion/GUIA_COLABORACION.md)**: Guía para colaboradores

---

## 🤝 Soporte

Para cualquier consulta o problema:

1. Revisar este archivo README
2. Consultar los archivos de documentación incluidos
3. Verificar que tengas .NET 9.0 SDK instalado
4. Asegurarte de que todos los puertos estén disponibles

---

## 📄 Licencia

Este proyecto es desarrollado con fines académicos para la materia de **Herramientas de Programación II** - Universidad Pascual Bravo.

---

## 🎉 ¡Listo para usar!

El sistema **Mi Plata** está completamente funcional y listo para demostrar todos los conceptos de POO requeridos en el curso. ¡Disfruta explorando todas las funcionalidades del cajero automático virtual!

---

*Desarrollado con ❤️ usando C# y ASP.NET Core*
