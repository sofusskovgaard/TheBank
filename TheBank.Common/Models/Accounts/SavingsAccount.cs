using System;

namespace TheBank.Common.Models.Accounts
{
    public class SavingsAccount : Account
    {
        public SavingsAccount(string name)
        {
            Id = Guid.NewGuid().ToString().Substring(0, 6);
            Name = name;
            Type = AccountType.SavingsAccount;
        }
        
        public SavingsAccount(string id, string name)
        {
            Id = id;
            Name = name;
            Type = AccountType.SavingsAccount;
        }
        
        #region private variables
        
        public override decimal TransactionFee => .00125M;

        public override decimal InterestRate => Balance < 50000 ? .01M : (Balance >= 50000 && Balance <= 100000 ? .02M : .03M);
        
        public override decimal NegativeCeiling => 0M;
        
        #endregion
    }
}