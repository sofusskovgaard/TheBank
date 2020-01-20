using System.Collections.Generic;
using System.Linq;

namespace TheBank.Core
{
    public class Bank
    {
        private int _lastId = 0;

        public Bank()
        {
            Accounts = new List<Account>();
        }
        
        public List<Account> Accounts { get; }

        public int AccountsCount => Accounts.Count;
        
        public string BankName { get; set; }

        public Account CreateAccount(string name)
        {
            var newAccount = new Account((_lastId+1).ToString(), name);
            Accounts.Add(newAccount);
            return newAccount;
        }

        public decimal Deposit(Account account, decimal amount)
        {
            account.Balance += amount;
            return account.Balance;
        }

        public decimal Withdraw(Account account, decimal amount)
        {
            account.Balance -= amount;
            return account.Balance;
        }

        public decimal Balance(Account account)
        {
            return account.Balance;
        }

        public Account GetAccount(string id) => Accounts.FirstOrDefault(account => account.Id == id);
    }
}