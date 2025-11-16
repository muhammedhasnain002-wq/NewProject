using CRM.Application.Abstractions;
using CRM.Application.Abstractions.Repositories;
using CRM.Application.DTOs;
using CRM.Domain.Entities;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CRM.Api.Endpoints;

public static class ProductsEndpoints
{
    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/products", async (CreateProductRequest req, IProductRepository repo, IUnitOfWork uow) =>
        {
            var errors = ProductValidation.ValidateCreate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var product = new Product(req.Sku, req.Name, req.UnitPrice, req.IsActive);
            await repo.AddAsync(product);
            await uow.SaveChangesAsync();
            var dto = new ProductDto(product.Id, product.Sku, product.Name, product.UnitPrice, product.IsActive);
            return Results.Created($"/products/{product.Id}", dto);
        }).WithName("AddProduct").WithOpenApi(op => { op.OperationId = "AddProduct"; op.Summary = "Add a new product"; return op; }).WithTags("Products");

        app.MapGet("/products/{id:guid}", async (Guid id, CrmDbContext db) =>
        {
            var p = await db.Products.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();
            var dto = new ProductDto(p.Id, p.Sku, p.Name, p.UnitPrice, p.IsActive);
            return Results.Ok(dto);
        }).WithName("GetProductById").WithOpenApi(op => { op.OperationId = "GetProductById"; op.Summary = "Get product by ID"; return op; }).WithTags("Products");

        app.MapGet("/products/by-sku/{sku}", async (string sku, IProductRepository repo) =>
        {
            var prod = await repo.GetBySkuAsync(sku);
            if (prod is null) return Results.NotFound();
            var dto = new ProductDto(prod.Id, prod.Sku, prod.Name, prod.UnitPrice, prod.IsActive);
            return Results.Ok(dto);
        }).WithName("GetProductBySku").WithOpenApi(op => { op.OperationId = "GetProductBySku"; op.Summary = "Get product by SKU"; return op; }).WithTags("Products");

        app.MapPut("/products/{id:guid}", async (Guid id, UpdateProductRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var errors = ProductValidation.ValidateUpdate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();
            p.Update(req.Sku, req.Name, req.UnitPrice, req.IsActive);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("UpdateProductById").WithOpenApi(op => { op.OperationId = "UpdateProductById"; op.Summary = "Update product by ID"; return op; }).WithTags("Products");

        app.MapDelete("/products/{id:guid}", async (Guid id, CrmDbContext db, IUnitOfWork uow) =>
        {
            var p = await db.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (p is null) return Results.NotFound();
            db.Products.Remove(p);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteProductById").WithOpenApi(op => { op.OperationId = "DeleteProductById"; op.Summary = "Delete product by ID"; return op; }).WithTags("Products");

        app.MapGet("/products", async (HttpContext http, IProductRepository repo, [AsParameters] QueryParameters parameters) =>
        {
            var productsQuery = repo.Query().AsNoTracking();
            if (!string.IsNullOrWhiteSpace(parameters.Search)) productsQuery = productsQuery.Where(p => p.Name.Contains(parameters.Search) || p.Sku.Contains(parameters.Search));
            productsQuery = parameters.Sort switch
            {
                "sku" => productsQuery.OrderBy(p => p.Sku),
                _ => productsQuery.OrderBy(p => p.Name)
            };
            var total = await productsQuery.CountAsync();
            var items = await productsQuery
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(p => new ProductDto(p.Id, p.Sku, p.Name, p.UnitPrice, p.IsActive))
                .ToListAsync();
            http.Response.Headers["X-Total-Count"] = total.ToString();
            return Results.Ok(new PagedResult<ProductDto>(items, total, parameters.Page, parameters.PageSize));
        }).WithName("ListAllProducts").WithOpenApi(op => { op.OperationId = "ListAllProducts"; op.Summary = "List all products with pagination and sorting"; return op; }).WithTags("Products");

        return app;
    }
}
