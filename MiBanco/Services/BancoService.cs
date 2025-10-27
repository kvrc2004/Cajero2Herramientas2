using MiBanco.Models;

namespace MiBanco.Services
{
    /// <summary>
    /// Servicio para manejar la lógica de negocio del sistema bancario
    /// Implementa el patrón Singleton para mantener los datos en memoria
    /// </summary>
    public class BancoService
    {
        private static BancoService? _instance;
        private static readonly object _lock = new object();

        // Listas para almacenar datos en memoria (simulando base de datos)
        private List<Cliente> _clientes = new List<Cliente>();
        private List<Cuenta> _cuentas = new List<Cuenta>();
        private int _siguienteIdCliente = 1;
        private int _siguienteIdCuenta = 1;

        /// <summary>
        /// Implementación del patrón Singleton
        /// </summary>
        public static BancoService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new BancoService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Constructor privado para el patrón Singleton
        /// Inicializa datos de prueba
        /// </summary>
        private BancoService()
        {
            InicializarDatosPrueba();
        }

        /// <summary>
        /// Método para inicializar datos de prueba
        /// </summary>
        private void InicializarDatosPrueba()
        {
            // Cliente de prueba
            var clientePrueba = new Cliente
            {
                Id = _siguienteIdCliente++,
                Identificacion = "12345678",
                Nombre = "Juan Pérez",
                Celular = "3001234567",
                Usuario = "juan.perez",
                Clave = "123456"
            };

            _clientes.Add(clientePrueba);

            // Cuentas de prueba para el cliente
            var cuentaAhorros = new CuentaAhorros($"AH{_siguienteIdCuenta:000000}", clientePrueba.Id)
            {
                Id = _siguienteIdCuenta++,
                Saldo = 1000000 // $1,000,000 inicial
            };

            var cuentaCorriente = new CuentaCorriente($"CC{_siguienteIdCuenta:000000}", clientePrueba.Id)
            {
                Id = _siguienteIdCuenta++,
                Saldo = 500000 // $500,000 inicial
            };

            var tarjetaCredito = new TarjetaCredito($"TC{_siguienteIdCuenta:000000}", clientePrueba.Id, 2000000)
            {
                Id = _siguienteIdCuenta++,
                LimiteCredito = 2000000 // Límite de $2,000,000
            };

            _cuentas.AddRange(new Cuenta[] { cuentaAhorros, cuentaCorriente, tarjetaCredito });
            clientePrueba.Cuentas.AddRange(new Cuenta[] { cuentaAhorros, cuentaCorriente, tarjetaCredito });
        }

        /// <summary>
        /// Registrar un nuevo cliente
        /// </summary>
        public bool RegistrarCliente(Cliente cliente)
        {
            // Verificar que el usuario no exista
            if (_clientes.Any(c => c.Usuario.Equals(cliente.Usuario, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Verificar que la identificación no exista
            if (_clientes.Any(c => c.Identificacion == cliente.Identificacion))
                return false;

            cliente.Id = _siguienteIdCliente++;
            _clientes.Add(cliente);

            // Crear cuentas iniciales para el nuevo cliente
            CrearCuentasIniciales(cliente);

            return true;
        }

        /// <summary>
        /// Crear cuentas iniciales para un nuevo cliente
        /// </summary>
        private void CrearCuentasIniciales(Cliente cliente)
        {
            // Crear cuenta de ahorros
            var cuentaAhorros = new CuentaAhorros($"AH{_siguienteIdCuenta:000000}", cliente.Id)
            {
                Id = _siguienteIdCuenta++,
                Saldo = 0
            };

            // Crear cuenta corriente
            var cuentaCorriente = new CuentaCorriente($"CC{_siguienteIdCuenta:000000}", cliente.Id)
            {
                Id = _siguienteIdCuenta++,
                Saldo = 0
            };

            // Crear tarjeta de crédito con límite inicial
            var tarjetaCredito = new TarjetaCredito($"TC{_siguienteIdCuenta:000000}", cliente.Id, 1000000)
            {
                Id = _siguienteIdCuenta++,
                LimiteCredito = 1000000 // Límite inicial de $1,000,000
            };

            _cuentas.AddRange(new Cuenta[] { cuentaAhorros, cuentaCorriente, tarjetaCredito });
            cliente.Cuentas.AddRange(new Cuenta[] { cuentaAhorros, cuentaCorriente, tarjetaCredito });
        }

        /// <summary>
        /// Autenticar un cliente
        /// </summary>
        public Cliente? AutenticarCliente(string usuario, string clave)
        {
            var cliente = _clientes.FirstOrDefault(c => c.Usuario.Equals(usuario, StringComparison.OrdinalIgnoreCase));
            
            Console.WriteLine($"DEBUG LOGIN - Usuario: '{usuario}', Clave ingresada: '{clave}'");
            
            if (cliente == null)
            {
                Console.WriteLine("DEBUG LOGIN - Cliente no encontrado");
                return null;
            }

            Console.WriteLine($"DEBUG LOGIN - Cliente encontrado. Clave en sistema: '{cliente.Clave}'");

            if (cliente.CuentaBloqueada)
            {
                Console.WriteLine("DEBUG LOGIN - Cuenta bloqueada");
                return null;
            }

            if (cliente.VerificarCredenciales(usuario, clave))
            {
                Console.WriteLine("DEBUG LOGIN - Credenciales correctas");
                cliente.ReiniciarIntentosLogin();
                return cliente;
            }
            else
            {
                cliente.IncrementarIntentosLogin();
                return null;
            }
        }

        /// <summary>
        /// Obtener cliente por ID
        /// </summary>
        public Cliente? ObtenerCliente(int id)
        {
            var cliente = _clientes.FirstOrDefault(c => c.Id == id);
            if (cliente != null)
            {
                // Cargar las cuentas del cliente
                cliente.Cuentas = _cuentas.Where(c => c.ClienteId == cliente.Id).ToList();
            }
            return cliente;
        }

        /// <summary>
        /// Obtener cuenta por número de cuenta
        /// </summary>
        public Cuenta? ObtenerCuentaPorNumero(string numeroCuenta)
        {
            return _cuentas.FirstOrDefault(c => c.NumeroCuenta == numeroCuenta);
        }

        /// <summary>
        /// Obtener cuenta por ID
        /// </summary>
        public Cuenta? ObtenerCuenta(int id)
        {
            return _cuentas.FirstOrDefault(c => c.Id == id);
        }

        /// <summary>
        /// Actualizar información del cliente
        /// </summary>
        public bool ActualizarCliente(Cliente cliente)
        {
            var clienteExistente = _clientes.FirstOrDefault(c => c.Id == cliente.Id);
            if (clienteExistente == null)
                return false;

            // Verificar que el nuevo usuario no esté en uso por otro cliente
            if (_clientes.Any(c => c.Id != cliente.Id && c.Usuario.Equals(cliente.Usuario, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Debug: Verificar valores antes de actualizar
            Console.WriteLine($"DEBUG - Actualizando cliente ID: {cliente.Id}");
            Console.WriteLine($"DEBUG - Clave anterior: '{clienteExistente.Clave}'");
            Console.WriteLine($"DEBUG - Nueva clave: '{cliente.Clave}'");
            Console.WriteLine($"DEBUG - Clave no es null o empty: {!string.IsNullOrEmpty(cliente.Clave)}");

            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Celular = cliente.Celular;
            clienteExistente.Usuario = cliente.Usuario;
            
            // Solo actualizar la clave si se proporcionó una nueva
            if (!string.IsNullOrEmpty(cliente.Clave))
            {
                clienteExistente.Clave = cliente.Clave;
                Console.WriteLine($"DEBUG - Clave actualizada a: '{clienteExistente.Clave}'");
            }
            else
            {
                Console.WriteLine("DEBUG - No se actualizó la clave (está vacía o null)");
            }

            return true;
        }

        /// <summary>
        /// Realizar transferencia entre cuentas
        /// </summary>
        public bool RealizarTransferencia(int cuentaOrigenId, string numeroCuentaDestino, decimal monto, string descripcion = "Transferencia")
        {
            var cuentaOrigen = ObtenerCuenta(cuentaOrigenId);
            var cuentaDestino = ObtenerCuentaPorNumero(numeroCuentaDestino);

            if (cuentaOrigen == null || cuentaDestino == null)
                return false;

            // Verificar que no sea la misma cuenta
            if (cuentaOrigen.Id == cuentaDestino.Id)
                return false;

            return cuentaOrigen.Transferir(cuentaDestino, monto, descripcion);
        }

        /// <summary>
        /// Obtener todos los clientes (para búsquedas de transferencia)
        /// </summary>
        public List<Cliente> ObtenerTodosLosClientes()
        {
            return _clientes.ToList();
        }

        /// <summary>
        /// Buscar cuentas por criterio
        /// </summary>
        public List<Cuenta> BuscarCuentas(string criterio)
        {
            return _cuentas.Where(c => 
                c.NumeroCuenta.Contains(criterio, StringComparison.OrdinalIgnoreCase) ||
                _clientes.Any(cliente => cliente.Id == c.ClienteId && 
                    (cliente.Nombre.Contains(criterio, StringComparison.OrdinalIgnoreCase) ||
                     cliente.Identificacion.Contains(criterio)))
            ).ToList();
        }

        /// <summary>
        /// Obtener resumen de todas las cuentas de un cliente
        /// </summary>
        public Dictionary<string, object> ObtenerResumenCliente(int clienteId)
        {
            var cliente = ObtenerCliente(clienteId);
            if (cliente == null)
                return new Dictionary<string, object>();

            var resumen = new Dictionary<string, object>
            {
                ["Cliente"] = cliente,
                ["TotalCuentas"] = cliente.Cuentas.Count,
                ["TotalSaldos"] = cliente.Cuentas.Sum(c => c.Saldo),
                ["TotalMovimientos"] = cliente.Cuentas.Sum(c => c.Movimientos.Count)
            };

            return resumen;
        }
    }
}