namespace TheBank.Core
{
    public class Bank
    {
        public Account Account { get; set; }
        
        public string BankName { get; set; }

        public Account CreateAccount(string name)
        {
            Account = new Account(name);
            return Account;
        }

        public decimal Deposit(decimal amount)
        {
            Account.Balance += amount;
            return Account.Balance;
        }

        public decimal Withdraw(decimal amount)
        {
            Account.Balance -= amount;
            return Account.Balance;
        }

        public decimal Balance()
        {
            return Account.Balance;
        }
    }
}