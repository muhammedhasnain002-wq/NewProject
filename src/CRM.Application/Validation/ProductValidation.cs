using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CRM.Application.DTOs;

namespace CRM.Application;

public static class ProductValidation
{
    public static IDictionary<string, string[]>? ValidateCreate(CreateProductRequest req)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(req);
        if (!Validator.TryValidateObject(req, context, results, true))
        {
            return results.GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
                          .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "").ToArray());
        }
        return null;
    }

    public static IDictionary<string, string[]>? ValidateUpdate(UpdateProductRequest req)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(req);
        if (!Validator.TryValidateObject(req, context, results, true))
        {
            return results.GroupBy(r => r.MemberNames.FirstOrDefault() ?? "")
                          .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage ?? "").ToArray());
        }
        return null;
    }
}
