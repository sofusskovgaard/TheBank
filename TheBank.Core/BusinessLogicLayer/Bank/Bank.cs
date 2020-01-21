using System;
using System.Collections.Generic;
using System.Linq;

using TheBank.Core.Models.Accounts;

namespace TheBank.Core.BusinessLogicLayer.Bank
{
    public class Bank : IBank
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

        public decimal Balance(Account account) => account.Balance;

        public Account GetAccount(string id) => Accounts.FirstOrDefault(account => account.Id == id);
    }

    public interface IBank
    {
        List<Account> Accounts { get; }
        
        int AccountsCount { get; }
        
        string BankName { get; set; }

        Account CreateAccount(string name, AccountType type);

        decimal Deposit(Account account, decimal amount);

        decimal Withdraw(Account account, decimal amount);

        decimal Balance(Account account);

        Account GetAccount(string id);
    }
}