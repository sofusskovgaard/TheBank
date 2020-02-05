using System;

namespace TheBank.Common.Models.Exceptions
{
    public class CancelledAccountSearchException : Exception
    {
        public CancelledAccountSearchException(string message)
        {
            Message = message;
        }
        
        public override string Message { get; }
    }
}