namespace TheBank.Core.Models.Accounts
{
    public class SavingsAccount : Account
    {
        public SavingsAccount(string id, string name) : base(id, name)
        {
            this.Type = AccountType.SavingsAccount;
        }
        
        #region private variables

        public override decimal InterestRate => Balance < 50000 ? .01M : (Balance >= 50000 && Balance <= 100000 ? .02M : .03M);
        
        public override decimal NegativeCeiling => 0M;
        
        #endregion
    }
}