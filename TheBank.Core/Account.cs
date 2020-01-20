using System;

namespace TheBank.Core
{
    public abstract class Account
    {
        protected Account(string id, string name = "John Doe")
        {
            Name = name;
            Id = id;
        }
        
        #region private variables
        
        private decimal _balance = 0;
        
        #endregion
        
        #region public variables
        
        public AccountType Type { get; set; }
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public decimal Balance
        {
            get => Math.Round(_balance, 2);
            set => _balance = value;
        }
        
        #endregion
        
        public abstract decimal InterestRate { get; }
        
        #region abstract functions

        public abstract decimal ChargeInterest();

        #endregion
    }

    public class ConsumerAccount : Account
    {
        public ConsumerAccount(string id, string name) : base(id, name)
        {
            this.Type = AccountType.ConsumerAccount;
        }
        
        #region private variables

        public override decimal InterestRate => Balance > 0 ? .001M : .2M;
        
        #endregion

        #region public overides

        public override decimal ChargeInterest()
        {
            Balance += Balance * InterestRate;
            return Balance;
        }

        #endregion
    }

    public class CheckingAccount : Account
    {
        public CheckingAccount(string id, string name) : base(id, name)
        {
            this.Type = AccountType.CheckingAccount;
        }
        
        #region private variables

        public override decimal InterestRate => .005M;

        #endregion

        #region public overides

        public override decimal ChargeInterest()
        {
            Balance += Balance * InterestRate;
            return Balance;
        }

        #endregion
    }

    public class SavingsAccount : Account
    {
        public SavingsAccount(string id, string name) : base(id, name)
        {
            this.Type = AccountType.SavingsAccount;
        }
        
        #region private variables

        public override decimal InterestRate => Balance < 50000 ? .01M : (Balance >= 50000 && Balance <= 100000 ? .02M : .03M);
        
        #endregion

        #region public overides

        public override decimal ChargeInterest()
        {
            Balance += Balance * InterestRate;
            return Balance;
        }

        #endregion
    }

    public enum AccountType
    {
        ConsumerAccount,
        CheckingAccount,
        SavingsAccount
    }
}