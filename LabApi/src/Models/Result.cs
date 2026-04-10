using Docker.DotNet;
using LabApi.Enum;

namespace LabApi.Models;

public class Result
{
    public bool IsSuccess { get; }
    public ErrorType Type { get; }
    public string ErrorMessage { get; }
    public int? OriginalStatusCode { get; }
    
    protected Result(bool isSuccess, ErrorType type, string errorMessage, int? originalStatusCode = null)
    {
        this.IsSuccess = isSuccess;
        this.Type = type;
        this.ErrorMessage = errorMessage;
        this.OriginalStatusCode = originalStatusCode;
    }
    public static Result Success() 
        => new Result(true, ErrorType.None, string.Empty);
    

    // Declare manual failure
    public static Result Failure(ErrorType type, string message) 
        => new Result(false, type, message);    

    // Resolve proper exception
    public static Result Failure(Exception exception)
    {

        if (exception is DockerApiException dockerException)
        {
            
            int statusCode = (int)dockerException.StatusCode;
            
            var errorType = statusCode switch 
            {
                404 => ErrorType.NotFound,
                409 => ErrorType.Conflict,
                401 => ErrorType.Unauthorized,
                400 => ErrorType.Validation,
                _ => ErrorType.Failure 
            };
            
            return new Result(false, errorType, dockerException.Message, statusCode);
        }

        var (type, message) = exception switch
        {
            DockerContainerNotFoundException => (ErrorType.NotFound, "Conteneur introuvable."),
            ArgumentException => (ErrorType.Validation, "Paramètres invalides."),
            UnauthorizedAccessException => (ErrorType.Unauthorized, "Accès refusé."),
        
            _ => (ErrorType.Failure, $"Erreur interne : {exception.Message}")
        };

        return new Result(false, type, message);
    }
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, ErrorType type, string errorMessage, int? originalStatusCode = null) 
        : base(isSuccess, type, errorMessage)
    {
        Value = value;
    }

    public static Result<T> Success(T value) 
        => new Result<T>(true, value, ErrorType.None, string.Empty);
    
    public new static Result<T> Failure(Exception ex)
    {
        var result = Result.Failure(ex); // Reuse the parent logic
        return new Result<T>(false, default, result.Type, result.ErrorMessage);
    }
}