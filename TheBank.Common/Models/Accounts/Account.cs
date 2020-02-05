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
            Id = id ?? Guid.NewGuid().ToString();
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
                    // If balance goes negative after being positive
                    _balance = value;
                    throw new OverdraftException($"You've overdrafted and you're now in a debt of {Math.Round(value, 2):0.00} kr");
                }
                
                if (value < 0 && _balance < 0 && value < _balance && value > NegativeCeiling)
                {
                    // If balance goes more negative
                    _balance = value;
                    throw new OverdraftException($"You've overdrafted again and you're now in a debt of {Math.Round(value, 2):0.00} kr");
                } 
                
                if (value < 0 && _balance < 0 && value > _balance && value > NegativeCeiling)
                {
                    // If balance goes less negative, but is still negative
                    _balance = value;
                    throw new OverdraftException($"You're paying off your debt, but you're still in a debt of {Math.Round(value, 2):0.00} kr");
                }

                if (value < NegativeCeiling)
                {
                    // If future balance is more negative than the specified ceiling
                    if (_balance > 0)
                    {
                        // If balance tries to go over the ceiling and is positive
                        throw new OverdraftException($"You've reached your overdraft ceiling of {NegativeCeiling:0.00} kr. Your balance is {Math.Round(_balance, 2):0.00} kr");
                    }
                    // If balance tries to go over the ceiling and is negative
                    throw new OverdraftException($"You've reached your overdraft ceiling of {NegativeCeiling:0.00} kr. You're currently in a debt of {Math.Round(_balance, 2):0.00} kr");
                }
                
                _balance = value;
            }
        }
        
        /// <summary>
        /// Charge interest on this account
        /// </summary>
        /// <returns>decimal</returns>
        public decimal ChargeInterest()
        {
            var oldBalance = _balance;
            var interest = _balance * InterestRate;
            
            _balance += interest;
            LoggerService.Write($"[INTERESTS][BALANCE: {oldBalance:0.00} => {_balance:0.00}] CHARGE INTEREST FROM ACCOUNT => {Id}");
            return interest;
        }

        #region Abstract Variables

        public abstract decimal TransactionFee { get; }
        
        public abstract decimal InterestRate { get; }
        
        public abstract decimal NegativeCeiling { get; }
        
        #endregion
    }
}