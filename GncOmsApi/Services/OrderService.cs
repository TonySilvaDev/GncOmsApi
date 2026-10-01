using Azure.Core;
using GncOmsApi.DTO;
using GncOmsApi.Exceptions;
using GncOmsApi.Models;
using GncOmsApi.Repositories;
using GncOmsApi.Utilities;
using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using System.Text.Json;

namespace GncOmsApi.Services
{
    public class OrderService(IOrderRepository repository) : IOrderService
    {
        public async Task<CreateOrderResponseDto> ProcessOrderAsync(CreateOrderRequestDto requestDto)
        {
            var existingOrder = await repository.GetByOrderNumberAsync(requestDto.OrderNumber);
            if (existingOrder != null)
            {
                return new CreateOrderResponseDto
                {
                    OrderId = existingOrder.PedidoId,
                    OrderNumber = existingOrder.OrderNumber,
                    Estatus = "Ordenado",
                    YaExistia = true
                };
            }

            var canalVenta = await repository.GetCanalByCodigoAsync(requestDto.Channel)
                        ?? throw new ApiException(
                                statusCode: StatusCodes.Status404NotFound,
                                errorCode: "CanalInvalido",
                                mensaje: $"No se encontró el canal",
                                detalle: $"El canal de venta '{requestDto.Channel}' no existe o no está activo");

            var estatusInicial = await repository.GetEstatusByCodigoAsync("Ordenado")
                        ?? throw new ApiException(
                                statusCode: StatusCodes.Status404NotFound,
                                errorCode: "EstatusInvalido",
                                mensaje: "No se encontro el estatus",
                                detalle: "El estatus 'Ordenado' no existe");

            var cliente = await repository.GetClienteByEmailAsync(requestDto.Customer.Email);
            if (cliente == null)
            {
                cliente = new Cliente
                {
                    ClienteId = Guid.Empty, // Se asigna en el repositorio
                    ExternalDestinationId = requestDto.ExternalDestinationId,
                    CustomerExternalId = requestDto.Customer.CustomerExternalId,
                    Nombre = requestDto.Customer.Nombre,
                    Apellido = requestDto.Customer.Apellido,
                    Email = requestDto.Customer.Email,
                    Telefono = requestDto.Customer.Telefono
                };
            }
            else
            {
                cliente.Nombre = requestDto.Customer.Nombre;
                cliente.Apellido = requestDto.Customer.Apellido;
                cliente.Telefono = requestDto.Customer.Telefono ?? cliente.Telefono;
            }

            var direccionEnvio = new DireccionEnvio
            {
                DireccionId = Guid.NewGuid(),
                Address1 = requestDto.ShippingAddress.Address1,
                Address2 = requestDto.ShippingAddress.Address2,
                Ciudad = requestDto.ShippingAddress.Ciudad,
                Estado = requestDto.ShippingAddress.Estado,
                CodigoPostal = requestDto.ShippingAddress.CodigoPostal,
                Pais = requestDto.ShippingAddress.Pais,
                Latitud = requestDto.ShippingAddress.Latitud,
                Longitud = requestDto.ShippingAddress.Longitud,
                Referencia = requestDto.ShippingAddress.Referencia
            };

            var newPedido = new Pedido
            {
                PedidoId = Guid.NewGuid(),
                OrderNumber = requestDto.OrderNumber,
                CanalVentaId = canalVenta.Id,
                EstatusActualId = estatusInicial.Id,
                IntentosReroute = 0,
                SyncIrrouteConfirmado = false,
                SyncSaConfirmado = false,
                SyncMerchConfirmado = false,
                PedidoDetalle = requestDto.Items.Select(x => new PedidoDetalle
                {
                    DetalleId = Guid.NewGuid(),
                    Sku = x.Sku,
                    Cantidad = x.Cantidad,
                    Precio = x.Precio,
                    Impuesto = x.Impuesto,
                    EstadoReserva = "Pendiente"
                }).ToList()
            };

            var payload = JsonSerializer.Serialize(new
            {
                requestDto.OrderNumber,
                requestDto.Channel,
                requestDto.Origen,
                requestDto.Customer,
                requestDto.ShippingAddress,
                requestDto.Items
            });

            var outboxEvent = new OutboxEvents
            {
                EventId = Guid.NewGuid(),
                PedidoId = newPedido.PedidoId,
                EventType = "OrderCreated",
                Payload = payload,
                Status = "Ordenado"
            };

            var historicoEstatusPedido = new HistoricoEstatusPedido
            {
                PedidoId = newPedido.PedidoId,
                EstatusNuevoId = newPedido.EstatusActualId,
                Origen = requestDto.Origen,
                MetadataJson = payload
            };

            await repository.CreateOrderAsync(cliente, direccionEnvio, newPedido, outboxEvent, historicoEstatusPedido);

            return new CreateOrderResponseDto
            {
                OrderId = newPedido.PedidoId,
                OrderNumber = newPedido.OrderNumber,
                Estatus = estatusInicial.Codigo,
                FechaCreacion = newPedido.FechaCreacion,
                YaExistia = false
            };
        }

        public async Task<OrderLookupResponseDto> LookupOrderByNumber(string orderNumber)
        {
            var order = await repository.GetByOrderNumberAsync(orderNumber);

            if (order == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "OrdenNoEncontrada",
                    mensaje: $"No se encontró la orden",
                    detalle: $"No se encontró la orden con numero '{orderNumber}'"
                    );
            }

            return new OrderLookupResponseDto
            {
                OrderId = order.PedidoId,
                OrderNumber = order.OrderNumber,
                Estatus = order.EstatusActual.Codigo
            };
        }

        public async Task<UpdateOrderResponseDto> UpdateOrderAsync(Guid orderId, UpdateOrderRequestDto requestDto)
        {
            var order = await repository.GetOrderByIdAsync(orderId);            

            if (order == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "OrdenNoEncontrada",
                    mensaje: $"No se encontró la orden",
                    detalle: $"No se encontró la orden con ID '{orderId}'"
                    );
            }

            bool existeCambio = false;
            CarrierAsignado? carrierAsignado = null;
            var actualizarCarrier = !string.IsNullOrWhiteSpace(requestDto.CarrierId);
            var actualizarTracking = !string.IsNullOrWhiteSpace(requestDto.TrackingId);

            if (actualizarCarrier || actualizarTracking)
            {
                carrierAsignado = await repository.GetLatestCarrierAssignmentAsync(orderId)
                    ?? throw new ApiException(
                        statusCode: StatusCodes.Status409Conflict,
                        errorCode: "CarrierNoAsignado",
                        mensaje: "No se puede actualizar el carrier o tracking de una orden sin asignación.",
                        detalle: "Proporcione carrierId y trackingId al cambiar el estatus a 'Surtido' antes de corregir esos datos.");
            }

            if (!string.IsNullOrWhiteSpace(requestDto.TiendaId))
            {
                order.TiendaAsignadaId = requestDto.TiendaId;
                existeCambio = true;
            }

            if (!string.IsNullOrWhiteSpace(requestDto.CarrierId))
            {
                var carrier = await repository.GetCarrierCatalogoByExternalIdAsync(requestDto.CarrierId)
                            ?? throw new ApiException(
                                        statusCode: StatusCodes.Status404NotFound,
                                        errorCode: "CarrierInvalido",
                                        mensaje: "No se encontró el carrier.",
                                        detalle: $"El carrier '{requestDto.CarrierId}' no existe o no está activo.");

                order.CarrierFinalId = carrier.Id;
                carrierAsignado!.CarrierId = carrier.Id;
                existeCambio = true;
            }

            if (!string.IsNullOrWhiteSpace(requestDto.TrackingId))
            {
                order.TrackingId = requestDto.TrackingId;
                carrierAsignado!.NumeroGuia = requestDto.TrackingId;
                existeCambio = true;
            }

            if (existeCambio)
            {
                order.FechaUltimaActualizacion = DateTime.UtcNow;
                await repository.UpdateOrderAsync(order, carrierAsignado);
            }
            

            return new UpdateOrderResponseDto
            {
                OrderId = orderId,
                ActualizadoEn = order.FechaUltimaActualizacion
            };
        }

        public async Task<OrderDetailResponseDto?> GetOrderByIdWithDetailsAsync(Guid orderId)
        {
            var order = await repository.GetOrderByIdWithDetailsAsync(orderId);

            if (order == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "OrdenNoEncontrada",
                    mensaje: "No se encontró la orden",
                    detalle: $"No se encontró la orden con ID '{orderId}'"
                    );
            }

            return new OrderDetailResponseDto
            {
                OrderId = order.PedidoId,
                OrderNumber = order.OrderNumber,
                Channel = order.CanalVenta.Codigo,
                Estatus = order.EstatusActual.Codigo,
                TiendaId = order.TiendaAsignadaId,
                CarrierId = order.CarrierFinal.ExternalCarrierId,
                TrackingId = order.TrackingId,
                IntentosReroute = order.IntentosReroute,
                FechaCreacion = order.FechaCreacion,
                FechaAsignacion = order.FechaAsignacion,
                FechaAceptacion = order.FechaAceptacion,
                FechaSurtido = order.FechaSurtido,
                FechaEntrega = order.FechaEntrega,
                SyncIrouteConfirmado = order.SyncSaConfirmado,
                Items = order.PedidoDetalle.Select(x => new OrderDetailItemDto
                {
                    SKU = x.Sku,
                    Cantidad = x.Cantidad
                }).ToList()
            };
        }

        public async Task<OrderHistoryResponseDto> GetOrderHistoryByIdAsync(Guid orderId)
        {
            var orderHistory = await repository.GetOrderHistoryByIdAsync(orderId);

            if (orderHistory == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "HistorialOrdenNoEncontrado",
                    mensaje: "No se encontró el historial",
                    detalle: $"No se encontró el historial de la orden con ID '{orderId}'"
                    );
            }

            return new OrderHistoryResponseDto
            {
                OrderId = orderId,
                Historial = orderHistory.Select(x => new OrderStatusHistoryDto
                {
                    EstatusAnterior = x.EstatusAnterior?.Codigo,
                    EstatusNuevo = x.EstatusNuevo.Codigo,
                    Origen = x.Origen,
                    Timestamp = x.Timestamp
                }).ToList()
            };
        }

        public async Task<OrdersFilterResponseDto> GetOrdersByFilterAsync(OrdersFilterRequestDto requestDto)
        {
            if (requestDto.Limit > 500)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status400BadRequest,
                    errorCode: "LimiteExcedido",
                    mensaje: "El parámetro 'limit' no puede ser mayor a 500 registros",
                    detalle: new { limitSolicitado = requestDto.Limit, maximoPermitido = 500 }
                    );
            }

            if (!string.IsNullOrWhiteSpace(requestDto.MetodoTracking))
            {
                var metodo = requestDto.MetodoTracking.ToLower().Trim();

                if(metodo != "webhook" && metodo != "polling")
                {
                    throw new ApiException(
                    statusCode: StatusCodes.Status400BadRequest,
                    errorCode: "ParametroInvalido",
                    mensaje: "El parámetro 'metodoTracking' solo acepta los valores 'webhook' o 'polling'",
                    detalle: new { valorEnviado = requestDto.MetodoTracking }
                    );
                }
            }

            var orders = await repository.GetOrdersByFilterAsync(requestDto);

            var mappedOrders = orders?.Select(x => new OrderSummaryDto
            {
                OrderId = x.PedidoId,
                OrderNumber = x.OrderNumber,
                Estatus = x.EstatusActual.Codigo,
                CarrierId = x.CarrierFinal.ExternalCarrierId
            }).ToList() ?? new List<OrderSummaryDto>();

            return new OrdersFilterResponseDto
            {
                Total = mappedOrders.Count,
                Orders = mappedOrders
            };
        }

        public async Task<AckIRouteResponseDto> ConfirmIrrouteSyncAsync(Guid orderId)
        {
            var order = await repository.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "OrdenNoEncontrada",
                    mensaje: "No se encontró la orden",
                    detalle: $"No se encontró la orden con ID '{orderId}'"
                    );
            }

            DateTime fechaConfirmacion = DateTime.UtcNow;

            if (!order.SyncIrrouteConfirmado)
            {
                order.SyncIrrouteConfirmado = true;
                order.FechaUltimaActualizacion = fechaConfirmacion;

                await repository.UpdateOrderAsync(order);
            }

            return new AckIRouteResponseDto
            {
                OrderId = order.PedidoId,
                SyncIrrouteConfirmado = order.SyncIrrouteConfirmado,
                ConfirmadoEn = fechaConfirmacion
            };
        }

        public async Task<ChangeStatusResponseDto> ChangeStatusAsync(Guid orderId, ChangeStatusRequestDto requestDto)
        {
            var order = await repository.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "OrdenNoEncontrada",
                    mensaje: "No se encontró la orden",
                    detalle: $"No se encontró la orden con ID '{orderId}'"
                    );
            }
            var estatusActual = await repository.GetEstatusByIdAsync(order.EstatusActualId);
            if (estatusActual == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status500InternalServerError,
                    errorCode: "EstatusActualInvalido",
                    mensaje: "No se pudo determinar el estatus actual de la orden.");
            }

            var estatusNuevo = await repository.GetEstatusByCodigoAsync(requestDto.NuevoEstatus);
            if (estatusNuevo == null)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status404NotFound,
                    errorCode: "EstatusInvalido",
                    mensaje: "No se encontró el estatus",
                    detalle: $"El estatus '{requestDto.NuevoEstatus}' no existe en el catálogo");
            }

            bool esTransicionValida = await repository.IsValidTransitionAsync(order.EstatusActualId, estatusNuevo.Id);
            if (!esTransicionValida)
            {
                throw new ApiException(
                    statusCode: StatusCodes.Status409Conflict,
                    errorCode: "TransicionInvalida",
                    mensaje: $"No se puede pasar de {estatusActual.Codigo} a {requestDto.NuevoEstatus}",
                    detalle: new { estatusActual = estatusActual.Codigo, estatusSolicitado = requestDto.NuevoEstatus });
            }

            if (estatusNuevo.Codigo == "Asignado")
            {
                order.FechaAsignacion = DateTime.UtcNow;
            }
            else if (estatusNuevo.Codigo == "Surtido")
            {
                order.FechaSurtido = DateTime.UtcNow;
            }

            CarrierAsignado? nuevaAsignacion = null;
            CarrierAsignado? asignacionActual = null;
            if (estatusNuevo.Codigo == "Surtido")
            {
                var carrierId = requestDto.Campos.GetStringField("carrierId");
                var trackingId = requestDto.Campos.GetStringField("trackingId");

                if (string.IsNullOrWhiteSpace(carrierId) || string.IsNullOrWhiteSpace(trackingId))
                {
                    throw new ApiException(
                        statusCode: StatusCodes.Status400BadRequest,
                        errorCode: "AsignacionIncompleta",
                        mensaje: "Para surtir un pedido se requieren carrierId y trackingId en campos.");
                }

                var carrier = await repository.GetCarrierCatalogoByExternalIdAsync(carrierId)
                    ?? throw new ApiException(
                        statusCode: StatusCodes.Status404NotFound,
                        errorCode: "CarrierInvalido",
                        mensaje: "No se encontró el carrier activo.",
                        detalle: $"El carrier '{carrierId}' no existe o no está activo.");

                var fechaAsignacionCarrier = DateTime.UtcNow;
                order.CarrierFinalId = carrier.Id;
                order.TrackingId = trackingId;

                nuevaAsignacion = new CarrierAsignado
                {
                    Id = Guid.NewGuid(),
                    PedidoId = order.PedidoId,
                    CarrierId = carrier.Id,
                    FuenteSistema = requestDto.Origen,
                    NumeroGuia = trackingId,
                    Status = "Asignado",
                    FechaAsignacion = fechaAsignacionCarrier
                };
            }
            else
            {
                var actualizaTracking = requestDto.Campos.HasField("trackingId");
                var actualizaTrackingStatus = estatusNuevo.Codigo == "En tránsito" || estatusNuevo.Codigo == "Entregado";
                if (actualizaTracking || actualizaTrackingStatus)
                {
                    asignacionActual = await repository.GetLatestCarrierAssignmentAsync(orderId)
                        ?? throw new ApiException(
                            statusCode: StatusCodes.Status409Conflict,
                            errorCode: "CarrierNoAsignado",
                            mensaje: "No se puede actualizar el tracking de una orden que no tiene carrier asignado.");
                    asignacionActual.Status = estatusNuevo.Codigo;

                    if (actualizaTracking)
                    {
                        var trackingId = requestDto.Campos.GetStringField("trackingId");
                        if (string.IsNullOrWhiteSpace(trackingId))
                        {
                            throw new ApiException(
                                statusCode: StatusCodes.Status400BadRequest,
                                errorCode: "TrackingInvalido",
                                mensaje: "trackingId no puede estar vacío.");
                        }

                        order.TrackingId = trackingId;
                        asignacionActual.NumeroGuia = trackingId;
                    }
                }
            }

            if (estatusActual.Codigo == "Negado/Reroute" && estatusNuevo.Codigo == "Asignado")
            {
                if (order.IntentosReroute >= 5)
                {
                    throw new ApiException(
                        statusCode: StatusCodes.Status409Conflict,
                        errorCode: "LimiteExcedidoReroute",
                        mensaje: "Limite excedido de intentos de reroute",
                        detalle: "Se ha alcanzado el limite de 5 intentos de reroute, se sugiere cambiar a UTF"
                        );
                }
                order.IntentosReroute++;
            }

            if (estatusNuevo.Codigo == "Devuelto")
            {
                if (!order.FechaEntrega.HasValue || (DateTime.UtcNow - order.FechaEntrega.Value).TotalDays > 90)
                {
                    throw new ApiException(
                        statusCode: StatusCodes.Status409Conflict,
                        errorCode :"VentanaDevolucionExcedida",
                        mensaje: "Ventana excedida para devolución",
                        detalle: "No se puede procesar la devolución, han transcurrido mas de 90 días desde la fecha de entrega"
                        );
                }
            }

            if (requestDto.Campos != null)
            {
                if (requestDto.Campos.TryGetPropertyValue("tiendaId", out var tiendaNode))
                {
                    order.TiendaAsignadaId = tiendaNode?.GetValue<string>();
                }

                if (estatusNuevo.Codigo != "Surtido" &&
                    requestDto.Campos.TryGetPropertyValue("trackingId", out var trackingNode))
                {
                    order.TrackingId = trackingNode?.GetValue<string>();
                }

                var fechaEntrega = requestDto.Campos.GetDateTimeField("fechaEntrega");
                if (fechaEntrega.HasValue)
                {
                    order.FechaEntrega = fechaEntrega.Value;
                    if (asignacionActual != null && estatusNuevo.Codigo == "Entregado")
                    {
                        asignacionActual.FechaEntregaReal = fechaEntrega.Value;
                    }
                }
            }            

            var historicoEstatusPedido = new HistoricoEstatusPedido
            {
                PedidoId = order.PedidoId,
                EstatusAnteriorId = order.EstatusActualId,
                EstatusNuevoId = estatusNuevo.Id,
                Origen = requestDto.Origen,
                MetadataJson = requestDto.Metadata?.ToJsonString(),
                Timestamp = DateTime.UtcNow
            };

             var outboxEvent = new OutboxEvents
            {
                EventId = Guid.NewGuid(),
                PedidoId = order.PedidoId,
                EventType = "OrderStatusChanged",
                Status = estatusNuevo.Codigo,
                Payload = JsonSerializer.Serialize(requestDto),
                CreatedAt = DateTime.UtcNow
            };

            var estatusAnterior = estatusActual.Codigo;
            order.EstatusActualId = estatusNuevo.Id;
            order.FechaUltimaActualizacion = DateTime.UtcNow;

            await repository.ChangeOrderStatusAcync(
                order,
                historicoEstatusPedido,
                outboxEvent,
                nuevaAsignacion,
                asignacionActual);

            return new ChangeStatusResponseDto
            {
                OrderId = order.PedidoId,
                EstatusAnterior = estatusAnterior,
                EstatusNuevo = estatusNuevo.Codigo,
                ActualizadoEn = order.FechaUltimaActualizacion
            };
        }

    }
}
