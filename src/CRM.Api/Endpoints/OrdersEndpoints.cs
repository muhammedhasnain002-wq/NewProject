using CRM.Application.Abstractions;
using CRM.Application.Abstractions.Repositories;
using CRM.Application.DTOs;
using CRM.Domain.Enums;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CRM.Api.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/orders", async (CreateOrderRequest req, IOrderRepository repo, IUnitOfWork uow) =>
        {
            var order = new CRM.Domain.Entities.Order(req.CustomerId);
            await repo.AddAsync(order);
            await uow.SaveChangesAsync();
            var dto = new OrderDto(order.Id, order.CustomerId, order.OrderDate, order.Status, order.Total);
            return Results.Created($"/orders/{order.Id}", dto);
        }).WithName("AddOrder").WithOpenApi(op => { op.OperationId = "AddOrder"; op.Summary = "Add a new order"; return op; }).WithTags("Orders");

        app.MapPost("/orders/{id:guid}/lines", async (Guid id, AddOrderLineRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var order = await db.Orders.Include(o => o.Lines).FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return Results.NotFound();
            order.AddLine(req.ProductId, req.Quantity, req.UnitPrice);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("AddOrderLine").WithOpenApi(op => { op.OperationId = "AddOrderLine"; op.Summary = "Add a new line item to an order"; return op; }).WithTags("Orders");

        app.MapGet("/orders/{id:guid}", async (Guid id, IOrderRepository repo) =>
        {
            var order = await repo.GetByIdAsync(id);
            if (order is null) return Results.NotFound();
            var dto = new OrderDto(order.Id, order.CustomerId, order.OrderDate, order.Status, order.Total);
            return Results.Ok(dto);
        }).WithName("GetOrderById").WithOpenApi(op => { op.OperationId = "GetOrderById"; op.Summary = "Get order by ID"; return op; }).WithTags("Orders");

        app.MapGet("/orders", async (HttpContext http, IOrderRepository repo, Guid? customerId, OrderStatus? status, [AsParameters] QueryParameters parameters) =>
        {
            var ordersQuery = repo.Query().AsNoTracking();
            if (customerId.HasValue) ordersQuery = ordersQuery.Where(o => o.CustomerId == customerId);
            if (status.HasValue) ordersQuery = ordersQuery.Where(o => o.Status == status);
            ordersQuery = parameters.Sort switch
            {
                "date" => ordersQuery.OrderByDescending(o => o.OrderDate),
                "total" => ordersQuery.OrderByDescending(o => o.Total),
                _ => ordersQuery.OrderByDescending(o => o.OrderDate)
            };
            var total = await ordersQuery.CountAsync();
            var items = await ordersQuery
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(o => new OrderDto(o.Id, o.CustomerId, o.OrderDate, o.Status, o.Total))
                .ToListAsync();
            http.Response.Headers["X-Total-Count"] = total.ToString();
            return Results.Ok(new PagedResult<OrderDto>(items, total, parameters.Page, parameters.PageSize));
        }).WithName("ListAllOrders").WithOpenApi(op => { op.OperationId = "ListAllOrders"; op.Summary = "List all orders with pagination and sorting"; return op; }).WithTags("Orders");

        app.MapGet("/orders/{id:guid}/lines", async (Guid id, CrmDbContext db) =>
        {
            var lines = await db.OrderLines.Include(l => l.Product)
                .Where(l => l.OrderId == id)
                .Select(l => new OrderLineDto(l.Id, l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal, l.Product.Sku, l.Product.Name))
                .ToListAsync();
            return Results.Ok(lines);
        }).WithName("GetOrderLines").WithOpenApi(op => { op.OperationId = "GetOrderLines"; op.Summary = "Get line items for a specific order"; return op; }).WithTags("Orders");

        app.MapPut("/orders/{id:guid}/status", async (Guid id, UpdateOrderStatusRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return Results.NotFound();
            order.SetStatus(req.Status);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("UpdateOrderStatus").WithOpenApi(op => { op.OperationId = "UpdateOrderStatus"; op.Summary = "Update the status of an order"; return op; }).WithTags("Orders");

        app.MapDelete("/orders/{id:guid}", async (Guid id, CrmDbContext db, IUnitOfWork uow) =>
        {
            var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);
            if (order is null) return Results.NotFound();
            db.Orders.Remove(order);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteOrderById").WithOpenApi(op => { op.OperationId = "DeleteOrderById"; op.Summary = "Delete order by ID"; return op; }).WithTags("Orders");

        return app;
    }
}
