using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace SkillForge.Api.Validation;

public static class ValidationHelper
{
    public static async Task<ActionResult?> ValidateAsync<T>(
        IValidator<T> validator,
        T instance,
        CancellationToken cancellationToken = default)
    {
        var result = await validator.ValidateAsync(instance, cancellationToken);
        return result.IsValid ? null : ToBadRequest(result);
    }

    public static ActionResult ToBadRequest(ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        return new BadRequestObjectResult(new
        {
            error = "validation_error",
            message = "One or more validation errors occurred.",
            errors,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        });
    }
}
