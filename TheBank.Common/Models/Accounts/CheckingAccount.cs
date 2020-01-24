using System;

namespace TheBank.Common.Models.Accounts
{
    public class CheckingAccount : Account
    {
        public CheckingAccount(string name)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 6);
            Name = name;
            Type = AccountType.CheckingAccount;
        }
        
        public CheckingAccount(string id, string name)
        {
            Id = id;
            Name = name;
            Type = AccountType.CheckingAccount;
        }
        
        #region private variables

        public override decimal TransactionFee => .001M;

        public override decimal InterestRate => .005M;
        
        public override decimal NegativeCeiling => -2500M;

        #endregion
    }
}