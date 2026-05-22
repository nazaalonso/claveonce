
//MANEJO GLOBAL DE ERRORES


using System;
// business rule exception se lanza cuando el usuario intente realizar una acción no permitida por el sistema.
namespace MiniApi.Exceptions;

public class BusinessRuleException : Exception
{
    public string ErrorCode { get; }
    public BusinessRuleException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}