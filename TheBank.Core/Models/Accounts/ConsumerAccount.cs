namespace TheBank.Core.Models.Accounts
{
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
}