using System;
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

        public Account CreateAccount(string name, AccountType accountType)
        {
            _lastId += 1;

            switch (accountType)
            {
                case AccountType.ConsumerAccount:
                    var newConsumberAccount = new ConsumerAccount(_lastId.ToString(), name);
                    Accounts.Add(newConsumberAccount);
                    return newConsumberAccount;
                case AccountType.CheckingAccount:
                    var newCheckingAccount = new CheckingAccount(_lastId.ToString(), name);
                    Accounts.Add(newCheckingAccount);
                    return newCheckingAccount;
                case AccountType.SavingsAccount:
                    var newSavingsAccount = new SavingsAccount(_lastId.ToString(), name);
                    Accounts.Add(newSavingsAccount);
                    return newSavingsAccount;
                default:
                    throw new Exception("MissingAccountType");
                    break;
            }
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