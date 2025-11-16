namespace CRM.Application.DTOs;

public sealed record QueryParameters(string? Search = null, string? Sort = null, int Page = 1, int PageSize = 10);
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);
