

//MANEJO GLOBAL DE ERRORES


using System;
//not found exception salta cuando un usuario busque algo que no existe. (un producto borrado)

namespace MiniApi.Exceptions;

public class NotFoundException : Exception
{
    public string ErrorCode { get; }
    public NotFoundException(string errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
}