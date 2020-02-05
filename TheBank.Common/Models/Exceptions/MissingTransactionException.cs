using System;

namespace TheBank.Common.Models.Exceptions
{
    public class MissingTransactionException : Exception
    {
        public MissingTransactionException(string message)
        {
            Message = message;
        }
        
        public override string Message { get; }
    }
}