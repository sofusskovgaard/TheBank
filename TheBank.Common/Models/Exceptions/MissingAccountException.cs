using System;

namespace TheBank.Common.Models.Exceptions
{
    public class MissingAccountException : Exception
    {
        public MissingAccountException(string message)
        {
            Message = message;
        }
        
        public override string Message { get; }
    }
}