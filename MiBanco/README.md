# 🏦 Mi Plata - Sistema Bancario

## 📋 Descripción del Proyecto

**Mi Plata** es un sistema bancario completo desarrollado en C# ASP.NET Core Razor Pages que simula las operaciones de un cajero automático. El proyecto implementa los conceptos fundamentales de Programación Orientada a Objetos (POO) incluyendo herencia, polimorfismo, encapsulación y abstracción.

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
- Visual Studio 2022 o Visual Studio Code
- Git (opcional)

### Pasos para Ejecutar

1. **Clonar o descargar el proyecto**
   ```bash
   # Si tienes Git instalado
   git clone [URL_DEL_REPOSITORIO]
   
   # O simplemente descargar y extraer el ZIP
   ```

2. **Abrir el proyecto**
   ```bash
   # Navegar al directorio
   cd MiBanco
   
   # Abrir en Visual Studio Code
   code .
   
   # O abrir MiBanco.sln en Visual Studio 2022
   ```

3. **Restaurar dependencias**
   ```bash
   dotnet restore
   ```

4. **Compilar el proyecto**
   ```bash
   dotnet build
   ```

5. **Ejecutar la aplicación**
   ```bash
   dotnet run
   ```

6. **Acceder a la aplicación**
   - Abrir el navegador web
   - Ir a `https://localhost:7xxx` (el puerto se mostrará en la terminal)
   - O hacer clic en el enlace que aparece en la terminal

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
- **Estilos**: CSS3 personalizado, Font Awesome
- **Datos**: Almacenamiento en memoria (Singleton Pattern)
- **Validación**: Data Annotations + JavaScript

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
El sistema usa almacenamiento en memoria. Los datos se reinician al reiniciar la aplicación.

### Logs
Los errores se muestran en la interfaz. Para desarrollo, revisar la consola del navegador.

---

## 📚 Documentación Adicional

- **📋 CHECKLIST_COMPLETO.md**: Lista completa de requisitos cumplidos
- **🏗️ DiagramaUML.md**: Diagrama de clases y arquitectura POO
- **📂 Estructura**: Código bien documentado con comentarios XML

---

## 🤝 Soporte

Para cualquier consulta o problema:

1. Revisar este archivo README
2. Consultar los archivos de documentación incluidos
3. Verificar que tengas .NET 9.0 SDK instalado
4. Asegurarte de que todos los puertos estén disponibles

---

## 📄 Licencia

Este proyecto es desarrollado con fines académicos para la materia de **Herramientas de Programación II** - Universidad XYZ.

---

## 🎉 ¡Listo para usar!

El sistema **Mi Plata** está completamente funcional y listo para demostrar todos los conceptos de POO requeridos en el curso. ¡Disfruta explorando todas las funcionalidades del cajero automático virtual!

---

*Desarrollado con ❤️ usando C# y ASP.NET Core*