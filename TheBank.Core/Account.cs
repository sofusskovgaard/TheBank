using System;

namespace TheBank.Core
{
    public class Account
    {
        private decimal _balance = 0;
        
        public Account(string id, string name = "John Doe")
        {
            Name = name;
            _balance = 0;
            Id = id;
        }
        
        public string Id { get; }
        
        public string Name { get; set; }

        public decimal Balance
        {
            get => Math.Round(_balance, 2);
            set => _balance = value;
        }
    }
}