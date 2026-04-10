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

            // 2. TA FACTORY EST ICI : On détermine le code HTTP selon le type d'exception
            var (statusCode, title, detail) = exception switch
            {
                // Erreurs Docker spécifiques
                DockerContainerNotFoundException => (404, "Conteneur introuvable", exception.Message),
                DockerApiException apiEx when apiEx.StatusCode == System.Net.HttpStatusCode.Conflict => (409, "Conflit Docker", "Le conteneur est peut-être déjà dans cet état."),
                
                // Erreurs classiques .NET
                ArgumentNullException => (400, "Données invalides", "Un paramètre requis est manquant."),
                UnauthorizedAccessException => (401, "Accès refusé", "Vous n'avez pas les droits."),
                
                // Le cas par défaut (500)
                _ => (500, "Erreur Serveur", "Une erreur interne s'est produite.")
            };

            // 3. On construit la réponse standard (ProblemDetails)
            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = statusCode;

            // 4. On écrit la réponse en JSON
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true; // On signale qu'on a géré l'erreur
        }
    }
}