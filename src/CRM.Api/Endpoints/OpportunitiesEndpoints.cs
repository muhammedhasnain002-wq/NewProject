using CRM.Application.Abstractions;
using CRM.Application.Abstractions.Repositories;
using CRM.Application.DTOs;
using CRM.Domain.Entities;
using CRM.Domain.Enums;
using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace CRM.Api.Endpoints;

public static class OpportunitiesEndpoints
{
    public static IEndpointRouteBuilder MapOpportunityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/opportunities", async (CreateOpportunityRequest req, IOpportunityRepository repo, IUnitOfWork uow) =>
        {
            var errors = OpportunityValidation.ValidateCreate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var opp = new Opportunity(req.CustomerId, req.Name, req.Stage, req.Amount, req.CloseDate);
            await repo.AddAsync(opp);
            await uow.SaveChangesAsync();
            var dto = new OpportunityDto(opp.Id, opp.CustomerId, opp.Name, opp.Stage, opp.Amount, opp.CloseDate);
            return Results.Created($"/opportunities/{opp.Id}", dto);
        }).WithName("AddOpportunity").WithOpenApi(op => { op.OperationId = "AddOpportunity"; op.Summary = "Add a new opportunity"; return op; }).WithTags("Opportunities");

        app.MapGet("/opportunities/{id:guid}", async (Guid id, IOpportunityRepository repo) =>
        {
            var opp = await repo.GetByIdAsync(id);
            if (opp is null) return Results.NotFound();
            var dto = new OpportunityDto(opp.Id, opp.CustomerId, opp.Name, opp.Stage, opp.Amount, opp.CloseDate);
            return Results.Ok(dto);
        }).WithName("GetOpportunityById").WithOpenApi(op => { op.OperationId = "GetOpportunityById"; op.Summary = "Get opportunity by ID"; return op; }).WithTags("Opportunities");

        app.MapGet("/opportunities", async (HttpContext http, CrmDbContext db, Guid? customerId, OpportunityStage? stage, [AsParameters] QueryParameters parameters) =>
        {
            var opportunitiesQuery = db.Opportunities.AsNoTracking();
            if (customerId.HasValue) opportunitiesQuery = opportunitiesQuery.Where(o => o.CustomerId == customerId);
            if (stage.HasValue) opportunitiesQuery = opportunitiesQuery.Where(o => o.Stage == stage);
            if (!string.IsNullOrWhiteSpace(parameters.Search)) opportunitiesQuery = opportunitiesQuery.Where(o => o.Name.Contains(parameters.Search));
            opportunitiesQuery = parameters.Sort switch
            {
                "amount" => opportunitiesQuery.OrderByDescending(o => o.Amount),
                "closeDate" => opportunitiesQuery.OrderByDescending(o => o.CloseDate),
                _ => opportunitiesQuery.OrderBy(o => o.Name)
            };
            var total = await opportunitiesQuery.CountAsync();
            var items = await opportunitiesQuery
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(o => new OpportunityDto(o.Id, o.CustomerId, o.Name, o.Stage, o.Amount, o.CloseDate))
                .ToListAsync();
            http.Response.Headers["X-Total-Count"] = total.ToString();
            return Results.Ok(new PagedResult<OpportunityDto>(items, total, parameters.Page, parameters.PageSize));
        }).WithName("ListAllOpportunities").WithOpenApi(op => { op.OperationId = "ListAllOpportunities"; op.Summary = "List all opportunities with pagination and sorting"; return op; }).WithTags("Opportunities");

        app.MapPut("/opportunities/{id:guid}", async (Guid id, UpdateOpportunityRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var errors = OpportunityValidation.ValidateUpdate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var opp = await db.Opportunities.FirstOrDefaultAsync(o => o.Id == id);
            if (opp is null) return Results.NotFound();
            opp.Update(req.Name, req.Stage, req.Amount, req.CloseDate);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("UpdateOpportunityById").WithOpenApi(op => { op.OperationId = "UpdateOpportunityById"; op.Summary = "Update opportunity by ID"; return op; }).WithTags("Opportunities");

        app.MapDelete("/opportunities/{id:guid}", async (Guid id, IOpportunityRepository repo, IUnitOfWork uow) =>
        {
            var opp = await repo.GetByIdAsync(id);
            if (opp is null) return Results.NotFound();
            repo.Remove(opp);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteOpportunityById").WithOpenApi(op => { op.OperationId = "DeleteOpportunityById"; op.Summary = "Delete opportunity by ID"; return op; }).WithTags("Opportunities");

        app.MapGet("/opportunities/search", async (CrmDbContext db, string q) =>
        {
            var list = await db.Opportunities.AsNoTracking()
                .Where(o => o.Name.Contains(q))
                .OrderByDescending(o => o.Amount)
                .Select(o => new OpportunityDto(o.Id, o.CustomerId, o.Name, o.Stage, o.Amount, o.CloseDate))
                .ToListAsync();
            return Results.Ok(list);
        }).WithName("SearchOpportunities").WithOpenApi(op => { op.OperationId = "SearchOpportunities"; op.Summary = "Search opportunities by name"; return op; }).WithTags("Opportunities");

        app.MapGet("/opportunities/{id:guid}/products", async (Guid id, CrmDbContext db) =>
        {
            var list = await db.OpportunityProducts.Include(op => op.Product)
                .Where(op => op.OpportunityId == id)
                .Select(op => new OpportunityProductDto(op.ProductId, op.Product.Sku, op.Product.Name, op.Quantity, op.Product.UnitPrice))
                .ToListAsync();
            return Results.Ok(list);
        }).WithName("GetOpportunityProducts").WithOpenApi(op => { op.OperationId = "GetOpportunityProducts"; op.Summary = "Get products for a specific opportunity"; return op; }).WithTags("Opportunities");

        app.MapPost("/opportunities/{id:guid}/products", async (Guid id, AddOpportunityProductRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var exists = await db.Opportunities.AnyAsync(o => o.Id == id);
            if (!exists) return Results.NotFound();
            var link = new OpportunityProduct(id, req.ProductId, req.Quantity);
            await db.OpportunityProducts.AddAsync(link);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("AddOpportunityProduct").WithOpenApi(op => { op.OperationId = "AddOpportunityProduct"; op.Summary = "Add a product to an opportunity"; return op; }).WithTags("Opportunities");

        return app;
    }
}
