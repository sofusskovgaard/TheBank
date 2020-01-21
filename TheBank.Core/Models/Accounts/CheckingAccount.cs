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

        #endregion

        #region public overides

        public override decimal ChargeInterest()
        {
            Balance += Balance * InterestRate;
            return Balance;
        }

        #endregion
    }
}