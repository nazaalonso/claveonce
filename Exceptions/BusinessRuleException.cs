
//MANEJO GLOBAL DE ERRORES


using System;
// business rule exception se lanza cuando el usuario intente realizar una acción no permitida por el sistema.
namespace ClaveOnce.Exceptions
{
    public class BusinessRuleException : Exception
    {
        public BusinessRuleException(string message) : base(message)
        {
        }
    }
}