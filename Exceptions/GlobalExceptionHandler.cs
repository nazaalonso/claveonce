
//MANEJO GLOBAL DE ERRORES

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Threading;
using System.Threading.Tasks;
using MiniApi.Exceptions;

namespace MiniApi.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(     //este metodo se ejecuta de .Net se ejecuta automaticamente cuando ocurre un error no controlado por la API
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            // Variables básicas para armar la respuesta obligatoria del TP
            int estadoStatus = 500; 
            string codigoError = "ERR-GENERICO";
            string mensajeError = exception.Message;

            // Revisamos el tipo de error usando IF tradicional
            if (exception is NotFoundException)
            {
                estadoStatus = 404;
                codigoError = "CRT-001"; // El código de error que inventarmos con el grupo
            }
            else if (exception is BusinessRuleException)
            {
                estadoStatus = 400;
                codigoError = "BR-002";
            }

            // Configuramos la respuesta web
            httpContext.Response.StatusCode = estadoStatus;
            httpContext.Response.ContentType = "application/json";

            // Armamos el texto JSON a mano, de forma clásica y sin vueltas raras
            string jsonResponse = "{\n" +
                "  \"type\": \"https://tools.ietf.org/html/rfc7231#section-6.5.4\",\n" +
                "  \"title\": \"Error de aplicacion\",\n" +
                "  \"status\": " + estadoStatus + ",\n" +
                "  \"errorCode\": \"" + codigoError + "\",\n" +
                "  \"errorMessage\": \"" + mensajeError + "\"\n" +
                "}";

            // Mandamos la respuesta al navegador/cliente
            await httpContext.Response.WriteAsync(jsonResponse, cancellationToken);

            // Devolvemos true para avisar que ya controlamos el error nosotros
            return true;
        }
    }
}