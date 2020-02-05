using System;

namespace TheBank.Common.Models.Accounts
{
    public class ConsumerAccount : Account
    {
        
        public ConsumerAccount(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Type = AccountType.ConsumerAccount;
            Balance += 500;
        }
        public ConsumerAccount(string id, string name)
        {
            Id = id;
            Name = name;
            Type = AccountType.ConsumerAccount;
            Balance += 500;
        }

        #region private variables
        
        public override decimal TransactionFee => 0;

        public override decimal InterestRate => Balance > 0 ? .001M : .2M;

        public override decimal NegativeCeiling => -25000M;

        #endregion
    }
}