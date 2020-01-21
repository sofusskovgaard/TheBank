using System;

namespace TheBank.Core.Models.Accounts
{
    public enum AccountType
    {
        ConsumerAccount,
        CheckingAccount,
        SavingsAccount
    }
    
    public abstract class Account
    {
        private decimal _balance = 0;
        
        protected Account(string id, string name = "John Doe")
        {
            Id = id;
            Name = name;
        }
        
        public string Id { get; set; }
        
        public string Name { get; set; }

        public AccountType Type { get; set; }

        public decimal Balance
        {
            get => Math.Round(_balance, 2);
            set => _balance = value;
        }

        // Abstract variables/methods
        public abstract decimal InterestRate { get; }

        public abstract decimal ChargeInterest();
    }
}