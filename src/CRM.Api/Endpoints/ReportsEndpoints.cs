using CRM.Infrastructure.Data;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.EntityFrameworkCore;

namespace CRM.Api.Endpoints;

public static class ReportsEndpoints
{
    public static IEndpointRouteBuilder MapReportEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/reports/top-customers", async (CrmDbContext db, int top = 5) =>
        {
            var sql = @"SELECT TOP (@p0) c.Id, c.Name, SUM(ol.LineTotal) AS Revenue
                        FROM Customers c
                        JOIN Orders o ON c.Id = o.CustomerId
                        JOIN OrderLines ol ON o.Id = ol.OrderId
                        GROUP BY c.Id, c.Name
                        ORDER BY Revenue DESC";
            var result = await db.Database.SqlQueryRaw<TopCustomerReport>(sql, parameters: new object[] { top }).ToListAsync();
            return Results.Ok(result);
        }).WithName("GenerateCustomerReport").WithOpenApi(op => { op.OperationId = "GenerateCustomerReport"; return op; }).WithTags("Reports");

        app.MapGet("/reports/sales-by-date-range", async (CrmDbContext db, DateTime from, DateTime to) =>
        {
            var sql = @"SELECT CAST(o.OrderDate AS DATE) AS [Date], SUM(ol.LineTotal) AS Sales
                        FROM Orders o
                        JOIN OrderLines ol ON o.Id = ol.OrderId
                        WHERE o.OrderDate BETWEEN @p0 AND @p1
                        GROUP BY CAST(o.OrderDate AS DATE)
                        ORDER BY [Date]";
            var result = await db.Database.SqlQueryRaw<SalesByDateReport>(sql, parameters: new object[] { from, to }).ToListAsync();
            return Results.Ok(result);
        }).WithName("GenerateSalesReport").WithOpenApi(op => { op.OperationId = "GenerateSalesReport"; return op; }).WithTags("Reports");

        return app;
    }
}

public sealed record TopCustomerReport(Guid Id, string Name, decimal Revenue);
public sealed record SalesByDateReport(DateTime Date, decimal Sales);
