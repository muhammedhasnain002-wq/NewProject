using CRM.Infrastructure;
using CRM.Api.Endpoints;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CSAPI", Version = "v1" });
});
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
  {
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CSAPI v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.MapCustomerEndpoints();
app.MapOpportunityEndpoints();
app.MapProductEndpoints();
app.MapOrderEndpoints();
app.MapReportEndpoints();

app.Run();
