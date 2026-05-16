namespace Claveonce.Helpers
{
    public static class ErrorResponse
    {
        public static object Create(
            string type,
            string title,
            int status,
            string detail,
            string instance,
            string errorCode,
            string errorMessage)
        {
            return new
            {
                type = type,
                title = title,
                status = status,
                detail = detail,
                instance = instance,
                errorCode = errorCode,
                errorMessage = errorMessage
            };
        }
    }
}