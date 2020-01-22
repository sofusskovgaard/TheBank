using System;
using TheBank.Common.Models.Exceptions;
using TheBank.Services;

namespace TheBank.Common.Models.Accounts
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
        
        protected Account(string id = null, string name = "John Doe")
        {
            Id = id ?? Guid.NewGuid().ToString().Substring(0, 6);
            Name = name;
        }
        
        public string Id { get; set; }
        
        public string Name { get; set; }

        public AccountType Type { get; set; }

        public decimal Balance
        {
            get => Math.Round(_balance, 2);
            set
            {
                if (value < 0 && _balance > 0 && value > NegativeCeiling)
                {
                    _balance = value;
                    throw new OverdraftException($"You've overdrafted and you're now in a debt of {Math.Round(value, 2):0.00} kr");
                }
                
                if (value < 0 && _balance < 0 && value < _balance && value > NegativeCeiling)
                {
                    _balance = value;
                    throw new OverdraftException($"You've overdrafted again and you're now in a debt of {Math.Round(value, 2):0.00} kr");
                } 
                
                if (value < 0 && _balance < 0 && value > _balance && value > NegativeCeiling)
                {
                    _balance = value;
                    throw new OverdraftException($"You're paying off your debt, but you're still in a debt of {Math.Round(value, 2):0.00} kr");
                }

                if (value < NegativeCeiling)
                {
                    if (_balance > 0)
                    {
                        throw new OverdraftException($"You've reached your overdraft ceiling of {NegativeCeiling:0.00} kr. Your balance is {Math.Round(_balance, 2):0.00} kr");
                    }
                    throw new OverdraftException($"You've reached your overdraft ceiling of {NegativeCeiling:0.00} kr. You're currently in a debt of {Math.Round(_balance, 2):0.00} kr");
                }

                _balance = value;
            }
        }

        // Abstract variables/methods
        public abstract decimal InterestRate { get; }
        
        public abstract decimal NegativeCeiling { get; }

        public decimal ChargeInterest()
        {
            var oldBalance = _balance;
            _balance += _balance * InterestRate;
            LoggerService.Write($"[INTERESTS][BALANCE: {oldBalance} => {_balance}] CHARGE INTEREST FROM ACCOUNT => {Id}");
            return _balance;
        }
    }
}