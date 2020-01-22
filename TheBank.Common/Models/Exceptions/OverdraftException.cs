using System;

namespace TheBank.Common.Models.Exceptions
{
    public class OverdraftException : Exception
    {
        public OverdraftException(string message)
        {
            Message = message;
        }
        
        public override string Message { get; }
    }
}