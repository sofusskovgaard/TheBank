using System;

namespace TheBank.Common.Models.Exceptions
{
    public class MissingAccountTypeException : Exception
    {
        public MissingAccountTypeException(string message)
        {
            Message = message;
        }
        
        public override string Message { get; }
    }
}