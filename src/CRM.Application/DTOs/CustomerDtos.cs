using CRM.Domain.Enums;

namespace CRM.Application.DTOs;

public sealed record AddressDto(string Line1, string? Line2, string City, string State, string PostalCode, string Country);
public sealed record CreateCustomerRequest(string Name, string Email, string Phone, CustomerStatus Status, AddressDto Address);
public sealed record UpdateCustomerRequest(string Name, string Phone, CustomerStatus Status, AddressDto Address);
public sealed record CustomerDto(Guid Id, string Name, string Email, string Phone, CustomerStatus Status, AddressDto Address, DateTime CreatedAt);
