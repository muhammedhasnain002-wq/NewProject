using CRM.Domain.Enums;

namespace CRM.Application.DTOs;

public sealed record CreateOrderRequest(Guid CustomerId);
public sealed record AddOrderLineRequest(Guid ProductId, int Quantity, decimal UnitPrice);
public sealed record UpdateOrderStatusRequest(OrderStatus Status);
public sealed record OrderDto(Guid Id, Guid CustomerId, DateTime OrderDate, OrderStatus Status, decimal Total);
public sealed record OrderLineDto(Guid Id, Guid ProductId, int Quantity, decimal UnitPrice, decimal LineTotal, string Sku, string Name);
