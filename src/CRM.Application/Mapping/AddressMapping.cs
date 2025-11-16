using CRM.Application.DTOs;
using CRM.Domain.ValueObjects;

namespace CRM.Application;

public static class AddressMapping
{
    public static Address ToValue(AddressDto dto) => new Address(dto.Line1, dto.Line2, dto.City, dto.State, dto.PostalCode, dto.Country);
    public static AddressDto FromValue(Address value) => new AddressDto(value.Line1, value.Line2, value.City, value.State, value.PostalCode, value.Country);
}
