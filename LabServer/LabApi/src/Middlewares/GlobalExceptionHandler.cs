using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Docker.DotNet; 

namespace LabApi.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            
            _logger.LogError(exception, "Une erreur non gérée est survenue : {Message}", exception.Message);

            var (statusCode, title, detail) = exception switch
            {
                DockerContainerNotFoundException => (404, "Conteneur introuvable", exception.Message),
                DockerApiException apiEx when apiEx.StatusCode == System.Net.HttpStatusCode.Conflict => (409, "Conflit Docker", "Le conteneur est peut-être déjà dans cet état."),
                
                ArgumentNullException => (400, "Données invalides", "Un paramètre requis est manquant."),
                UnauthorizedAccessException => (401, "Accès refusé", "Vous n'avez pas les droits."),
                
                _ => (500, "Erreur Serveur", "Une erreur interne s'est produite.")
            };

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; 
        }
    }
}