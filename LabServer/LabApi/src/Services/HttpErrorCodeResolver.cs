using Microsoft.AspNetCore.Mvc;
using LabApi.Models;
using LabApi.Services;
using LabApi.Enum;

namespace LabApi.Services;

public class HttpErrorCodeResolver 
{
    public IActionResult Resolve(Result result)
    {
        if (result.IsSuccess)
        {
            return new OkResult();
        }

        return MapError(result);
    }

    public IActionResult Resolve<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return new OkObjectResult(result.Value);
        }

        return MapError(result);
    }

    private IActionResult MapError(Result result)
        {
            if (result.OriginalStatusCode.HasValue)
            {
                return new ObjectResult(new { Error = result.ErrorMessage })
                {
                    StatusCode = result.OriginalStatusCode.Value
                };
            }
            
            return result.Type switch
            {
                ErrorType.NotFound => new NotFoundObjectResult(new { Error = result.ErrorMessage }),
                ErrorType.Conflict => new ConflictObjectResult(new { Error = result.ErrorMessage }),
                ErrorType.Validation => new BadRequestObjectResult(new { Error = result.ErrorMessage }),
                ErrorType.Unauthorized => new UnauthorizedObjectResult(new { Error = result.ErrorMessage }),
                
                _ => new ObjectResult(new { Error = "Une erreur interne est survenue." }) 
                     { StatusCode = 500 }
            };
        }
}