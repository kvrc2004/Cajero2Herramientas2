using Microsoft.EntityFrameworkCore;
using MiBanco.Data;
using MiBanco.Models;

namespace MiBanco.Services
{
    public class BancoService
    {
        private readonly MiBancoDbContext _context;

        public BancoService(MiBancoDbContext context)
        {
            _context = context;
        }

        public async Task<bool> RegistrarCliente(Cliente cliente)
        {
            if (await _context.Clientes.AnyAsync(c => c.Usuario.ToLower() == cliente.Usuario.ToLower()))
                return false;

            if (await _context.Clientes.AnyAsync(c => c.Identificacion == cliente.Identificacion))
                return false;

            // Agregar el cliente a la base de datos
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync(); // Esto genera el Id del cliente

            // Ahora crear las cuentas con el Id del cliente ya generado
            await CrearCuentasIniciales(cliente);

            return true;
        }

        private async Task CrearCuentasIniciales(Cliente cliente)
        {
            int siguienteNumero = await _context.Cuentas.CountAsync() + 1;

            // Crear cuenta de ahorros
            var cuentaAhorros = new CuentaAhorros
            {
                NumeroCuenta = $"AH{siguienteNumero:000000}",
                ClienteId = cliente.Id,
                Saldo = 0,
                FechaCreacion = DateTime.Now
            };
            siguienteNumero++;

            // Crear cuenta corriente
            var cuentaCorriente = new CuentaCorriente
            {
                NumeroCuenta = $"CC{siguienteNumero:000000}",
                ClienteId = cliente.Id,
                Saldo = 0,
                FechaCreacion = DateTime.Now
            };
            siguienteNumero++;

            // Crear tarjeta de crédito
            var tarjetaCredito = new TarjetaCredito
            {
                NumeroCuenta = $"TC{siguienteNumero:000000}",
                ClienteId = cliente.Id,
                Saldo = 0,
                LimiteCredito = 1000000,
                FechaCreacion = DateTime.Now
            };

            _context.Cuentas.AddRange(new Cuenta[] { cuentaAhorros, cuentaCorriente, tarjetaCredito });
            await _context.SaveChangesAsync();
        }

        public async Task<Cliente?> AutenticarCliente(string usuario, string clave)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Cuentas)
                .FirstOrDefaultAsync(c => c.Usuario.ToLower() == usuario.ToLower());

            if (cliente == null || cliente.CuentaBloqueada)
                return null;

            if (cliente.VerificarCredenciales(usuario, clave))
            {
                cliente.ReiniciarIntentosLogin();
                await _context.SaveChangesAsync();
                return cliente;
            }
            else
            {
                cliente.IncrementarIntentosLogin();
                await _context.SaveChangesAsync();
                return null;
            }
        }

        public async Task<Cliente?> ObtenerCliente(int id)
        {
            return await _context.Clientes
                .Include(c => c.Cuentas)
                    .ThenInclude(cuenta => cuenta.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cuenta?> ObtenerCuentaPorNumero(string numeroCuenta)
        {
            return await _context.Cuentas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.NumeroCuenta == numeroCuenta);
        }

        public async Task<Cuenta?> ObtenerCuenta(int id)
        {
            return await _context.Cuentas
                .Include(c => c.Movimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> ActualizarCliente(Cliente cliente)
        {
            Console.WriteLine($"DEBUG ActualizarCliente - Inicio. ClienteId: {cliente.Id}");
            Console.WriteLine($"DEBUG ActualizarCliente - Datos a actualizar: Nombre={cliente.Nombre}, Celular={cliente.Celular}, Usuario={cliente.Usuario}, Clave={(!string.IsNullOrEmpty(cliente.Clave) ? "CON VALOR" : "VACÍA")}");
            
            // Recargar el cliente desde la base de datos para asegurar que tenemos la versión más reciente
            var clienteExistente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.Id == cliente.Id);
            
            if (clienteExistente == null)
            {
                Console.WriteLine("DEBUG ActualizarCliente - Cliente no encontrado");
                return false;
            }

            Console.WriteLine($"DEBUG ActualizarCliente - Cliente encontrado: Nombre={clienteExistente.Nombre}, Celular={clienteExistente.Celular}, Usuario={clienteExistente.Usuario}, Clave={clienteExistente.Clave}");

            // Verificar si el usuario ya existe para otro cliente
            if (await _context.Clientes.AnyAsync(c => c.Id != cliente.Id && c.Usuario.ToLower() == cliente.Usuario.ToLower()))
            {
                Console.WriteLine("DEBUG ActualizarCliente - Usuario ya existe para otro cliente");
                return false;
            }

            // Actualizar los campos
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Celular = cliente.Celular;
            clienteExistente.Usuario = cliente.Usuario;

            Console.WriteLine($"DEBUG ActualizarCliente - Campos actualizados: Nombre={clienteExistente.Nombre}, Celular={clienteExistente.Celular}, Usuario={clienteExistente.Usuario}");

            // Solo actualizar la clave si se proporcionó una nueva
            if (!string.IsNullOrEmpty(cliente.Clave))
            {
                Console.WriteLine($"DEBUG ActualizarCliente - Actualizando clave de '{clienteExistente.Clave}' a '{cliente.Clave}'");
                clienteExistente.Clave = cliente.Clave;
            }

            // Marcar como modificado y guardar los cambios
            _context.Clientes.Update(clienteExistente);
            Console.WriteLine("DEBUG ActualizarCliente - Cliente marcado como modificado");
            
            var cambios = await _context.SaveChangesAsync();
            Console.WriteLine($"DEBUG ActualizarCliente - SaveChangesAsync ejecutado. Cambios guardados: {cambios}");
            
            return true;
        }

        public async Task<bool> RealizarTransferencia(int cuentaOrigenId, string numeroCuentaDestino, decimal monto, string descripcion = "Transferencia")
        {
            var cuentaOrigen = await ObtenerCuenta(cuentaOrigenId);
            var cuentaDestino = await ObtenerCuentaPorNumero(numeroCuentaDestino);

            if (cuentaOrigen == null || cuentaDestino == null || cuentaOrigen.Id == cuentaDestino.Id)
                return false;

            var resultado = cuentaOrigen.Transferir(cuentaDestino, monto, descripcion);

            if (resultado)
            {
                _context.Movimientos.AddRange(cuentaOrigen.Movimientos.Where(m => m.Id == 0));
                _context.Movimientos.AddRange(cuentaDestino.Movimientos.Where(m => m.Id == 0));
                await _context.SaveChangesAsync();
            }

            return resultado;
        }

        public async Task<bool> Consignar(int cuentaId, decimal monto, string descripcion = "Consignación")
        {
            var cuenta = await ObtenerCuenta(cuentaId);
            if (cuenta == null) return false;

            var resultado = cuenta.Consignar(monto, descripcion);

            if (resultado)
            {
                _context.Movimientos.AddRange(cuenta.Movimientos.Where(m => m.Id == 0));
                await _context.SaveChangesAsync();
            }

            return resultado;
        }

        public async Task<bool> Retirar(int cuentaId, decimal monto, string descripcion = "Retiro")
        {
            var cuenta = await ObtenerCuenta(cuentaId);
            if (cuenta == null) return false;

            var resultado = cuenta.Retirar(monto, descripcion);

            if (resultado)
            {
                _context.Movimientos.AddRange(cuenta.Movimientos.Where(m => m.Id == 0));
                await _context.SaveChangesAsync();
            }

            return resultado;
        }

        public async Task<List<Cliente>> ObtenerTodosLosClientes()
        {
            return await _context.Clientes.Include(c => c.Cuentas).ToListAsync();
        }

        public async Task<List<Cuenta>> BuscarCuentas(string criterio)
        {
            var criterioLower = criterio.ToLower();
            return await _context.Cuentas
                .Where(c => c.NumeroCuenta.ToLower().Contains(criterioLower))
                .ToListAsync();
        }

        public async Task<Dictionary<string, object>> ObtenerResumenCliente(int clienteId)
        {
            var cliente = await ObtenerCliente(clienteId);
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

        public async Task<bool> ComprarEnCuotas(int cuentaId, decimal monto, int numeroCuotas, string descripcion = "Compra en cuotas")
        {
            var cuenta = await ObtenerCuenta(cuentaId);
            if (cuenta is not TarjetaCredito tarjeta) return false;

            var resultado = tarjeta.RealizarCompraEnCuotas(monto, numeroCuotas, descripcion);

            if (resultado)
            {
                _context.Movimientos.AddRange(tarjeta.Movimientos.Where(m => m.Id == 0));
                await _context.SaveChangesAsync();
            }

            return resultado;
        }
    }
}
