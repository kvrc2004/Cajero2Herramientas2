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

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            await CrearCuentasIniciales(cliente);

            return true;
        }

        private async Task CrearCuentasIniciales(Cliente cliente)
        {
            int siguienteNumero = await _context.Cuentas.CountAsync() + 1;

            var cuentaAhorros = new CuentaAhorros($"AH{siguienteNumero:000000}", cliente.Id)
            {
                Saldo = 0
            };
            siguienteNumero++;

            var cuentaCorriente = new CuentaCorriente($"CC{siguienteNumero:000000}", cliente.Id)
            {
                Saldo = 0
            };
            siguienteNumero++;

            var tarjetaCredito = new TarjetaCredito($"TC{siguienteNumero:000000}", cliente.Id, 1000000)
            {
                LimiteCredito = 1000000
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
            var clienteExistente = await _context.Clientes.FindAsync(cliente.Id);
            if (clienteExistente == null)
                return false;

            if (await _context.Clientes.AnyAsync(c => c.Id != cliente.Id && c.Usuario.ToLower() == cliente.Usuario.ToLower()))
                return false;

            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Celular = cliente.Celular;
            clienteExistente.Usuario = cliente.Usuario;

            if (!string.IsNullOrEmpty(cliente.Clave))
            {
                clienteExistente.Clave = cliente.Clave;
            }

            await _context.SaveChangesAsync();
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
    }
}
