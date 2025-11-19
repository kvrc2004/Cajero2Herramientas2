# Diagrama UML del Sistema Bancario "Mi Plata"

## Descripción General
Este diagrama muestra la estructura de clases del sistema bancario que implementa los conceptos de Programación Orientada a Objetos (POO), específicamente herencia y polimorfismo.

## Diagrama de Clases

```mermaid
classDiagram
    class Cliente {
        -int Id
        -string Identificacion
        -string Nombre
        -string Celular
        -string Usuario
        -string Clave
        -int IntentosLogin
        -bool CuentaBloqueada
        -DateTime FechaRegistro
        -List~Cuenta~ Cuentas
        +Cliente()
        +Cliente(string, string, string, string, string)
        +bool VerificarCredenciales(string, string)
        +void IncrementarIntentosLogin()
        +void ReiniciarIntentosLogin()
        +Cuenta ObtenerCuentaPorTipo(string)
    }

    class Cuenta {
        <<abstract>>
        -int Id
        -string NumeroCuenta
        -decimal Saldo
        -int ClienteId
        -DateTime FechaCreacion
        -List~Movimiento~ Movimientos
        +Cuenta()
        #Cuenta(string, int)
        +abstract bool Consignar(decimal, string)
        +abstract bool Retirar(decimal, string)
        +virtual bool Transferir(Cuenta, decimal, string)
        #void RegistrarMovimiento(string, decimal, string, decimal)
        +virtual string ObtenerTipoCuenta()
        +virtual string ObtenerInformacionEspecifica()
    }

    class CuentaAhorros {
        -const decimal TASA_INTERES_MENSUAL = 0.015m
        -DateTime UltimaFechaCalculoInteres
        +CuentaAhorros()
        +CuentaAhorros(string, int)
        +override bool Consignar(decimal, string)
        +override bool Retirar(decimal, string)
        +void CalcularYAplicarIntereses()
        -int CalcularMesesTranscurridos(DateTime, DateTime)
        +override string ObtenerInformacionEspecifica()
        +override string ObtenerTipoCuenta()
        +decimal ObtenerInteresesProyectados()
    }

    class CuentaCorriente {
        -const decimal PORCENTAJE_SOBREGIRO = 0.20m
        -decimal MontoSobregiro
        +CuentaCorriente()
        +CuentaCorriente(string, int)
        +override bool Consignar(decimal, string)
        +override bool Retirar(decimal, string)
        +override string ObtenerInformacionEspecifica()
        +override string ObtenerTipoCuenta()
        +decimal ObtenerSobregiroMaximo()
        +decimal ObtenerSobregiroDisponible()
        +bool EstaEnSobregiro()
        +override bool Transferir(Cuenta, decimal, string)
    }

    class TarjetaCredito {
        -const decimal TASA_SIN_INTERES = 0.00m
        -const decimal TASA_INTERES_MEDIO = 0.019m
        -const decimal TASA_INTERES_ALTO = 0.023m
        -decimal LimiteCredito
        +decimal CreditoDisponible
        +TarjetaCredito()
        +TarjetaCredito(string, int, decimal)
        +override bool Consignar(decimal, string)
        +override bool Retirar(decimal, string)
        +bool RealizarCompraEnCuotas(decimal, int, string)
        -decimal ObtenerTasaInteresPorCuotas(int)
        +decimal CalcularPagoMensual(decimal, int)
        +override string ObtenerInformacionEspecifica()
        +override string ObtenerTipoCuenta()
        +bool TieneDeuda()
        +decimal ObtenerDeuda()
        +override bool Transferir(Cuenta, decimal, string)
        +bool AvanceEfectivo(decimal, Cuenta, string)
    }

    class Movimiento {
        -int Id
        -DateTime Fecha
        -string Tipo
        -decimal Monto
        -string Descripcion
        -decimal SaldoAnterior
        -decimal SaldoNuevo
        -int CuentaId
        +Movimiento()
        +Movimiento(string, decimal, string, decimal, decimal, int)
        +string ObtenerDescripcionCompleta()
        +bool EsDebito()
        +bool EsCredito()
    }

    class BancoService {
        <<Singleton>>
        -static BancoService _instance
        -static object _lock
        -List~Cliente~ _clientes
        -List~Cuenta~ _cuentas
        -int _siguienteIdCliente
        -int _siguienteIdCuenta
        +static BancoService Instance
        -BancoService()
        -void InicializarDatosPrueba()
        +bool RegistrarCliente(Cliente)
        -void CrearCuentasIniciales(Cliente)
        +Cliente AutenticarCliente(string, string)
        +Cliente ObtenerCliente(int)
        +Cuenta ObtenerCuentaPorNumero(string)
        +Cuenta ObtenerCuenta(int)
        +bool ActualizarCliente(Cliente)
        +bool RealizarTransferencia(int, string, decimal, string)
        +List~Cliente~ ObtenerTodosLosClientes()
        +List~Cuenta~ BuscarCuentas(string)
        +Dictionary~string,object~ ObtenerResumenCliente(int)
    }

    %% Relaciones de Herencia
    Cuenta <|-- CuentaAhorros : hereda
    Cuenta <|-- CuentaCorriente : hereda
    Cuenta <|-- TarjetaCredito : hereda

    %% Relaciones de Composición y Agregación
    Cliente ||--o{ Cuenta : posee
    Cuenta ||--o{ Movimiento : registra
    BancoService ||--o{ Cliente : gestiona
    BancoService ||--o{ Cuenta : administra

    %% Notas sobre Patrones y Conceptos POO
    note for Cuenta "Clase abstracta que demuestra\nel uso de polimorfismo.\nLos métodos abstractos deben ser\nimplementados por las clases hijas."
    
    note for CuentaAhorros "Implementa lógica específica:\n- Interés del 1.5% mensual\n- Cálculo automático de intereses"
    
    note for CuentaCorriente "Implementa lógica específica:\n- Sobregiro del 20%\n- Sin intereses"
    
    note for TarjetaCredito "Implementa lógica específica:\n- Sistema de cuotas\n- Intereses variables según cuotas\n- ≤2 cuotas: 0%\n- ≤6 cuotas: 1.9%\n- ≥7 cuotas: 2.3%"
    
    note for BancoService "Patrón Singleton\nGestiona todos los datos\nen memoria de forma centralizada"
```

## Conceptos POO Implementados

### 1. **Herencia**
- **Clase Base:** `Cuenta` (abstracta)
- **Clases Derivadas:** `CuentaAhorros`, `CuentaCorriente`, `TarjetaCredito`
- **Beneficio:** Reutilización de código y estructura común

### 2. **Polimorfismo**
- **Métodos Abstractos:** `Consignar()`, `Retirar()`
- **Métodos Virtuales:** `Transferir()`, `ObtenerTipoCuenta()`, `ObtenerInformacionEspecifica()`
- **Implementaciones Específicas:** Cada tipo de cuenta implementa su propia lógica de negocio

### 3. **Encapsulación**
- **Campos Privados:** Protección de datos sensibles
- **Propiedades Públicas:** Acceso controlado a los datos
- **Métodos Protegidos:** `RegistrarMovimiento()` solo accesible por clases hijas

### 4. **Abstracción**
- **Clase Abstracta Cuenta:** Define la interfaz común sin implementación
- **Servicios:** Separación de lógica de negocio en `BancoService`

## Patrones de Diseño

### 1. **Singleton**
- **Clase:** `BancoService`
- **Propósito:** Garantizar una única instancia del servicio de banco
- **Implementación:** Thread-safe con double-check locking

### 2. **Template Method** (Implícito)
- **Método:** `Transferir()` en clase base
- **Uso:** Utiliza los métodos abstractos `Retirar()` y `Consignar()` de las clases hijas

## Reglas de Negocio Implementadas

### Cuenta de Ahorros
- ✅ Interés del 1.5% mensual aplicado al retirar
- ✅ Cálculo automático de intereses por meses transcurridos

### Cuenta Corriente  
- ✅ Sobregiro del 20% sobre saldo actual
- ✅ Sin generación de intereses
- ✅ Manejo de pagos que cubren sobregiro

### Tarjeta de Crédito
- ✅ Sin interés para compras ≤2 cuotas
- ✅ 1.9% mensual para compras ≤6 cuotas  
- ✅ 2.3% mensual para compras ≥7 cuotas
- ✅ Cálculo y mostrar pago mensual por transacción

## Flujo de Datos

1. **Cliente** se registra/autentica a través de **BancoService**
2. **BancoService** crea automáticamente las tres tipos de **Cuenta**
3. Cada **Cuenta** registra **Movimientos** en todas las transacciones
4. Las operaciones utilizan **polimorfismo** según el tipo de cuenta
5. **BancoService** coordina transferencias entre cuentas diferentes

Este diseño cumple completamente con los requisitos de POO y arquitectura solicitados en el proyecto.