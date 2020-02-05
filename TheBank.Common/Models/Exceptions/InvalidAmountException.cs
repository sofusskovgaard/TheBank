using System;

namespace TheBank.Common.Models.Exceptions
{
    public class InvalidAmountException : Exception
    {
        public InvalidAmountException(string message)
        {
            Message = message;
        }
        
        public override string Message { get; }
    }
}