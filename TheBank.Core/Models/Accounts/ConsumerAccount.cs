using System;

namespace TheBank.Core.Models.Accounts
{
    public class ConsumerAccount : Account
    {
        
        public ConsumerAccount(string name)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 6);
            Name = name;
            Type = AccountType.ConsumerAccount;
        }
        public ConsumerAccount(string id, string name)
        {
            Id = id;
            Name = name;
            Type = AccountType.ConsumerAccount;
        }

        #region private variables

        public override decimal InterestRate => Balance > 0 ? .001M : .2M;

        public override decimal NegativeCeiling => -25000M;

        #endregion
    }
}