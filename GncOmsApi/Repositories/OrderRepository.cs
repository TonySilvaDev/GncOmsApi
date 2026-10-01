using GncOmsApi.Data;
using GncOmsApi.DTO;
using GncOmsApi.Models;
using Microsoft.EntityFrameworkCore;

namespace GncOmsApi.Repositories
{
    public class OrderRepository(AppDbContext context) : IOrderRepository
    {
        public async Task<Pedido> CreateOrderAsync(Cliente cliente, DireccionEnvio direccionEnvio, Pedido pedido, OutboxEvents outboxEvents, HistoricoEstatusPedido historicoEstatusPedido)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                if (context.Entry(cliente).State == EntityState.Detached && cliente.ClienteId == Guid.Empty)
                {
                    cliente.ClienteId = Guid.NewGuid();
                    await context.Cliente.AddAsync(cliente);
                }
                else
                {
                    context.Cliente.Update(cliente);
                }

                direccionEnvio.ClienteId = cliente.ClienteId;
                await context.DireccionEnvio.AddAsync(direccionEnvio);

                pedido.ClienteId = cliente.ClienteId;
                pedido.DireccionEnvioId = direccionEnvio.DireccionId;

                await context.Pedido.AddAsync(pedido);
                await context.OutboxEvents.AddAsync(outboxEvents);
                await context.HistoricoEstatusPedido.AddAsync(historicoEstatusPedido);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();

                return pedido;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<Pedido?> GetByOrderNumberAsync(string orderNumber)
        {
            return await context.Pedido
                        .AsNoTracking()
                        .Include(x => x.EstatusActual)
                        .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);
        }

        public async Task<CanalVenta?> GetCanalByCodigoAsync(string codigo)
        {
            return await context.CanalVenta
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Codigo == codigo && x.Activo);
        }

        public async Task<EstatusPedido?> GetEstatusByCodigoAsync(string codigo)
        {
            return await context.EstatusPedido
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Codigo == codigo);
        }

        public async Task<EstatusPedido?> GetEstatusByIdAsync(Guid estatusId)
        {
            return await context.EstatusPedido
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.Id == estatusId);
        }

        public async Task<Cliente?> GetClienteByEmailAsync(string email)
        {
            return await context.Cliente
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<CarrierCatalogo?> GetCarrierCatalogoByExternalIdAsync(string externalCarrierId)
        {
            return await context.CarrierCatalogo
                        .FirstOrDefaultAsync(x => x.ExternalCarrierId == externalCarrierId && x.Activo);
        }

        public async Task<Pedido?> GetOrderByIdAsync(Guid pedidoId)
        {
            return await context.Pedido
                        .FirstOrDefaultAsync(x => x.PedidoId == pedidoId);
        }

        public async Task UpdateOrderAsync(Pedido pedido, CarrierAsignado? carrierAsignado = null)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Pedido.Update(pedido);
                if (carrierAsignado != null)
                {
                    context.CarrierAsignado.Update(carrierAsignado);
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<CarrierAsignado?> GetLatestCarrierAssignmentAsync(Guid pedidoId)
        {
            return await context.CarrierAsignado
                .Where(x => x.PedidoId == pedidoId)
                .OrderByDescending(x => x.FechaAsignacion)
                .ThenByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Pedido?> GetOrderByIdWithDetailsAsync(Guid pedidoId)
        {
            return await context.Pedido
                        .AsNoTracking()
                        .Include(x => x.PedidoDetalle)
                        .Include(x => x.CanalVenta)
                        .Include(x => x.EstatusActual)
                        .Include(x => x.CarrierFinal)
                        .FirstOrDefaultAsync(x => x.PedidoId == pedidoId);
        }

        public async Task<List<HistoricoEstatusPedido>?> GetOrderHistoryByIdAsync(Guid pedidoId)
        {
            return await context.HistoricoEstatusPedido
                        .Include(x => x.EstatusAnterior)
                        .Include(x => x.EstatusNuevo)
                        .Where(x => x.PedidoId == pedidoId)
                        .ToListAsync();
        }

        public async Task<List<Pedido>?> GetOrdersByFilterAsync(OrdersFilterRequestDto filter)
        {
            var query = context.Pedido
                        .AsNoTracking()
                        .Include(x => x.CarrierFinal)
                        .Include(x => x.EstatusActual)
                        .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Estatus))
            {
                var listEstatus = filter.Estatus
                        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                        .ToList();

                if (listEstatus.Count > 0)
                {
                    query = query.Where(x => listEstatus.Contains(x.EstatusActual.Codigo));
                }
            }

            if (!string.IsNullOrWhiteSpace(filter.CarrierId))
            {
                query = query.Where(x => x.CarrierFinal.ExternalCarrierId == filter.CarrierId);
            }

            if (!string.IsNullOrWhiteSpace(filter.MetodoTracking))
            {
                query = query.Where(x => x.CarrierFinal != null && x.CarrierFinal.MetodoTracking.ToLower() == filter.MetodoTracking.ToLower());
            }

            if (filter.PendienteSyncIRoute.HasValue && filter.PendienteSyncIRoute.Value)
            {
                query = query.Where(x => x.SyncIrrouteConfirmado == false);
            }

            int limit = Math.Min(filter.Limit <= 0 ? 100 : filter.Limit, 500);

            return await query
                    .OrderByDescending(x => x.FechaCreacion)
                    .Take(limit)
                    .ToListAsync();
        }

        public async Task<bool> IsValidTransitionAsync(Guid estatusOrigenId, Guid estatudDestinoId)
        {
            return await context.TransicionEstatusConfig
                    .AsNoTracking()
                    .AnyAsync(x => x.EstatusOrigenId == estatusOrigenId &&
                                   x.EstatusDestinoId == estatudDestinoId &&
                                   x.Activo);
        }

        public async Task ChangeOrderStatusAcync(
            Pedido pedido,
            HistoricoEstatusPedido historicoEstatusPedido,
            OutboxEvents outboxEvents,
            CarrierAsignado? nuevaAsignacion,
            CarrierAsignado? asignacionActual)
        {
            await using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                context.Pedido.Update(pedido);
                if (nuevaAsignacion != null)
                {
                    await context.CarrierAsignado.AddAsync(nuevaAsignacion);
                }
                if (asignacionActual != null)
                {
                    context.CarrierAsignado.Update(asignacionActual);
                }
                await context.HistoricoEstatusPedido.AddAsync(historicoEstatusPedido);
                await context.OutboxEvents.AddAsync(outboxEvents);

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
