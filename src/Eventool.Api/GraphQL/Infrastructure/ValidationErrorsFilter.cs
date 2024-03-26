using FluentValidation;

namespace Eventool.Api.GraphQL.Infrastructure;

public class ValidationErrorsFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is null)
            return error;

        try
        {
            if (error.Exception is ValidationException validationException)
            {
                return ErrorBuilder.New()
                    .SetMessage("Validation failed.")
                    .SetCode("VALIDATION_ERROR")
                    .SetExtension("validationErrors", validationException.Errors) // Assuming your ValidationException has a collection of errors
                    .SetException(validationException)
                    .Build();
            }
            return error;
        }
        catch (Exception e)
        {
            // If something goes wrong in the error handling itself, log it and return a generic error
            // Avoid throwing exceptions from here
            return ErrorBuilder.New().SetMessage($"An unexpected error occurred.\n{e.Message}").Build();
        }
    }
}