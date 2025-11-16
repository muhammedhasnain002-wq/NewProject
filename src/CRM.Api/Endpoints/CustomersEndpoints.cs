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

public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/customers", async (CreateCustomerRequest req, ICustomerRepository repo, IUnitOfWork uow) =>
        {
            var errors = CustomerValidation.ValidateCreate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            if (await repo.EmailExistsAsync(req.Email)) return Results.Conflict("Email already exists");
            var customer = new Customer(req.Name, req.Email, req.Phone, req.Status, AddressMapping.ToValue(req.Address));
            await repo.AddAsync(customer);
            await uow.SaveChangesAsync();
            var dto = new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.Status, AddressMapping.FromValue(customer.Address), customer.CreatedAt);
            return Results.Created($"/customers/{customer.Id}", dto);
        }).WithName("AddCustomer").WithOpenApi(op => { op.OperationId = "AddCustomer"; op.Summary = "Add a new customer"; return op; }).WithTags("Customers");

        app.MapGet("/customers/{id:guid}", async (Guid id, ICustomerRepository repo) =>
        {
            var customer = await repo.GetByIdAsync(id);
            if (customer is null) return Results.NotFound();
            var dto = new CustomerDto(customer.Id, customer.Name, customer.Email, customer.Phone, customer.Status, AddressMapping.FromValue(customer.Address), customer.CreatedAt);
            return Results.Ok(dto);
        }).WithName("GetCustomerById").WithOpenApi(op => { op.OperationId = "GetCustomerById"; op.Summary = "Get customer by ID"; return op; }).WithTags("Customers");

        app.MapGet("/customers", async (HttpContext http, CrmDbContext db, [AsParameters] QueryParameters parameters) =>
        {
            var customersQuery = db.Customers.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(parameters.Search))
            {
                customersQuery = customersQuery.Where(c => c.Name.Contains(parameters.Search) || c.Email.Contains(parameters.Search));
            }
            customersQuery = parameters.Sort switch
            {
                "createdAt" => customersQuery.OrderByDescending(c => c.CreatedAt),
                _ => customersQuery.OrderBy(c => c.Name)
            };
            var total = await customersQuery.CountAsync();
            var items = await customersQuery
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(c => new CustomerDto(c.Id, c.Name, c.Email, c.Phone, c.Status, new AddressDto(c.Address.Line1, c.Address.Line2, c.Address.City, c.Address.State, c.Address.PostalCode, c.Address.Country), c.CreatedAt))
                .ToListAsync();
            http.Response.Headers["X-Total-Count"] = total.ToString();
            return Results.Ok(new PagedResult<CustomerDto>(items, total, parameters.Page, parameters.PageSize));
        }).WithName("ListAllCustomers").WithOpenApi(op => { op.OperationId = "ListAllCustomers"; op.Summary = "List all customers with pagination and sorting"; return op; }).WithTags("Customers");

        app.MapPut("/customers/{id:guid}", async (Guid id, UpdateCustomerRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var errors = CustomerValidation.ValidateUpdate(req);
            if (errors is not null) return Results.ValidationProblem(errors);
            var customer = await db.Customers.FirstOrDefaultAsync(c => c.Id == id);
            if (customer is null) return Results.NotFound();
            customer.Update(req.Name, req.Phone, req.Status, AddressMapping.ToValue(req.Address));
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("UpdateCustomerById").WithOpenApi(op => { op.OperationId = "UpdateCustomerById"; op.Summary = "Update customer by ID"; return op; }).WithTags("Customers");

        app.MapDelete("/customers/{id:guid}", async (Guid id, ICustomerRepository repo, IUnitOfWork uow) =>
        {
            var customer = await repo.GetByIdAsync(id);
            if (customer is null) return Results.NotFound();
            repo.Remove(customer);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteCustomerById").WithOpenApi(op => { op.OperationId = "DeleteCustomerById"; op.Summary = "Delete customer by ID"; return op; }).WithTags("Customers");

        app.MapGet("/customers/search", async (HttpContext http, ICustomerRepository repo, [AsParameters] QueryParameters parameters) =>
        {
            var customersQuery = repo.Query().AsNoTracking().Where(c => c.Name.Contains(parameters.Search ?? "") || c.Email.Contains(parameters.Search ?? ""));
            customersQuery = parameters.Sort switch
            {
                "createdAt" => customersQuery.OrderByDescending(c => c.CreatedAt),
                _ => customersQuery.OrderBy(c => c.Name)
            };
            var total = await customersQuery.CountAsync();
            var items = await customersQuery
                .Skip((parameters.Page - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .Select(c => new CustomerDto(c.Id, c.Name, c.Email, c.Phone, c.Status, new AddressDto(c.Address.Line1, c.Address.Line2, c.Address.City, c.Address.State, c.Address.PostalCode, c.Address.Country), c.CreatedAt))
                .ToListAsync();
            http.Response.Headers["X-Total-Count"] = total.ToString();
            return Results.Ok(new PagedResult<CustomerDto>(items, total, parameters.Page, parameters.PageSize));
        }).WithName("SearchCustomers").WithOpenApi(op => { op.OperationId = "SearchCustomers"; op.Summary = "Search customers by name or email"; return op; }).WithTags("Customers");

        app.MapGet("/customers/{id:guid}/orders", async (Guid id, IOrderRepository orders) =>
        {
            var list = await orders.Query().AsNoTracking()
                .Where(o => o.CustomerId == id)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderDto(o.Id, o.CustomerId, o.OrderDate, o.Status, o.Total))
                .ToListAsync();
            return Results.Ok(list);
        }).WithName("GetCustomerOrders").WithOpenApi(op => { op.OperationId = "GetCustomerOrders"; op.Summary = "Get orders for a specific customer"; return op; }).WithTags("Customers");

        app.MapGet("/customers/{id:guid}/activities", async (Guid id, CrmDbContext db) =>
        {
            var list = await db.Activities.AsNoTracking().Include(a => a.Opportunity)
                .Where(a => a.Opportunity.CustomerId == id)
                .OrderByDescending(a => a.DueDate)
                .Select(a => new ActivityDto(a.Id, a.OpportunityId, a.Type, a.Subject, a.Notes, a.DueDate, a.Completed))
                .ToListAsync();
            return Results.Ok(list);
        }).WithName("GetCustomerActivities").WithOpenApi(op => { op.OperationId = "GetCustomerActivities"; op.Summary = "Get activities for a specific customer"; return op; }).WithTags("Customers");

        app.MapGet("/customers/{id:guid}/contacts", async (Guid id, CrmDbContext db) =>
        {
            var contacts = await db.Contacts.AsNoTracking()
                .Where(c => c.CustomerId == id)
                .OrderByDescending(c => c.IsPrimary)
                .ThenBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .Select(c => new ContactDto(c.Id, c.CustomerId, c.FirstName, c.LastName, c.Email, c.Phone, c.IsPrimary))
                .ToListAsync();
            return Results.Ok(contacts);
        }).WithName("GetCustomerContacts").WithOpenApi(op => { op.OperationId = "GetCustomerContacts"; op.Summary = "Get contacts for a specific customer"; return op; }).WithTags("Customers");

        app.MapPost("/customers/{id:guid}/contacts", async (Guid id, CreateContactRequest req, CrmDbContext db, IUnitOfWork uow) =>
        {
            var exists = await db.Customers.AsNoTracking().AnyAsync(c => c.Id == id);
            if (!exists) return Results.NotFound();
            var contact = new Contact(id, req.FirstName, req.LastName, req.Email, req.Phone, req.IsPrimary);
            await db.Contacts.AddAsync(contact);
            await uow.SaveChangesAsync();
            var dto = new ContactDto(contact.Id, contact.CustomerId, contact.FirstName, contact.LastName, contact.Email, contact.Phone, contact.IsPrimary);
            return Results.Created($"/customers/{id}/contacts/{contact.Id}", dto);
        }).WithName("AddCustomerContact").WithOpenApi(op => { op.OperationId = "AddCustomerContact"; op.Summary = "Add a new contact to a customer"; return op; }).WithTags("Customers");

        app.MapDelete("/customers/{id:guid}/contacts/{contactId:guid}", async (Guid id, Guid contactId, CrmDbContext db, IUnitOfWork uow) =>
        {
            var contact = await db.Contacts.FirstOrDefaultAsync(c => c.Id == contactId && c.CustomerId == id);
            if (contact is null) return Results.NotFound();
            db.Contacts.Remove(contact);
            await uow.SaveChangesAsync();
            return Results.NoContent();
        }).WithName("DeleteCustomerContact").WithOpenApi(op => { op.OperationId = "DeleteCustomerContact"; op.Summary = "Delete a contact from a customer"; return op; }).WithTags("Customers");

        return app;
    }
}
