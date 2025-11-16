namespace CRM.Application.DTOs;

public sealed record CreateProductRequest(string Sku, string Name, decimal UnitPrice, bool IsActive);
public sealed record UpdateProductRequest(string Sku, string Name, decimal UnitPrice, bool IsActive);
public sealed record ProductDto(Guid Id, string Sku, string Name, decimal UnitPrice, bool IsActive);
