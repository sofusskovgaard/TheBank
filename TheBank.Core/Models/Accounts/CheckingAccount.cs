namespace TheBank.Core.Models.Accounts
{
    public class CheckingAccount : Account
    {
        public CheckingAccount(string id, string name) : base(id, name)
        {
            this.Type = AccountType.CheckingAccount;
        }
        
        #region private variables

        public override decimal InterestRate => .005M;
        
        public override decimal NegativeCeiling => -2500M;

        #endregion
    }
}