

//MANEJO GLOBAL DE ERRORES


using System;
//not found exception salta cuando un usuario busque algo que no existe. (un producto borrado)
namespace ClaveOnce.Exceptions
{
        public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}