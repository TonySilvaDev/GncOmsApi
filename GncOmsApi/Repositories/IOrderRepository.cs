using GncOmsApi.DTO;
using GncOmsApi.Models;

namespace GncOmsApi.Repositories
{
    public interface IOrderRepository
    {
        Task<Pedido?> GetByOrderNumberAsync(string orderNumber);
        Task<CanalVenta?> GetCanalByCodigoAsync(string codigo);
        Task<EstatusPedido?> GetEstatusByCodigoAsync(string codigo);
        Task<EstatusPedido?> GetEstatusByIdAsync(Guid estatusId);
        Task<Pedido> CreateOrderAsync(Cliente cliente, DireccionEnvio direccionEnvio, Pedido pedido, OutboxEvents outboxEvents, HistoricoEstatusPedido historicoEstatusPedido);
        Task<Cliente?> GetClienteByEmailAsync(string email);
        Task<CarrierCatalogo?> GetCarrierCatalogoByExternalIdAsync(string externalCarrierId);
        Task<Pedido?> GetOrderByIdAsync(Guid pedidoId);
        Task UpdateOrderAsync(Pedido pedido);
        Task<Pedido?> GetOrderByIdWithDetailsAsync(Guid pedidoId);
        Task<List<HistoricoEstatusPedido>?> GetOrderHistoryByIdAsync(Guid pedidoId);
        Task<List<Pedido>?> GetOrdersByFilterAsync(OrdersFilterRequestDto filter);
        Task<bool> IsValidTransitionAsync(Guid estatusOrigenId, Guid estatudDestinoId);
        Task ChangeOrderStatusAcync(Pedido pedido, HistoricoEstatusPedido historicoEstatusPedido, OutboxEvents outboxEvents);
    }
}
