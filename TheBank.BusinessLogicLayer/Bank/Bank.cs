using System;
using System.Collections.Generic;
using System.Linq;
using TheBank.DataAccessLayer;
using TheBank.Common.Models.Accounts;
using TheBank.Services;

namespace TheBank.BusinessLogicLayer.Bank
{
    public class Bank : IBank
    {
        private FileRepository _fileRepository = new FileRepository();
        
        private int _lastId = 0;

        public Bank()
        {

        }
        
        public List<Account> Accounts => _fileRepository.Accounts;

        public int AccountsCount => Accounts.Count;
        
        public string Name { get; set; }

        public Account CreateAccount(string name, AccountType accountType, decimal balance = 0M)
        {
            _lastId += 1;

            switch (accountType)
            {
                case AccountType.ConsumerAccount:
                    var newConsumberAccount = new ConsumerAccount(name) { Balance = balance};
                    Accounts.Add(newConsumberAccount);
                    _fileRepository.AddRow(newConsumberAccount);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW CONSUMER ACCOUNT, ID = {newConsumberAccount.Id}");
                    return newConsumberAccount;
                case AccountType.CheckingAccount:
                    var newCheckingAccount = new CheckingAccount(name) { Balance = balance};
                    Accounts.Add(newCheckingAccount);
                    _fileRepository.AddRow(newCheckingAccount);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW CHECKING ACCOUNT, ID = {newCheckingAccount.Id}");
                    return newCheckingAccount;
                case AccountType.SavingsAccount:
                    var newSavingsAccount = new SavingsAccount(name) { Balance = balance};
                    Accounts.Add(newSavingsAccount);
                    _fileRepository.AddRow(newSavingsAccount);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW SAVINGS ACCOUNT, ID = {newSavingsAccount.Id}");
                    return newSavingsAccount;
                default:
                    throw new Exception("MissingAccountType");
                    break;
            }
        }

        public decimal Deposit(Account account, decimal amount)
        {
            account.Balance += amount;
            LoggerService.Write($"[DEPOSIT] DEPOSIT OF {amount} INTO ACCOUNT => {account.Id}");
            _fileRepository.UpdateRow(account);
            return account.Balance;
        }

        public decimal Withdraw(Account account, decimal amount)
        {
            account.Balance -= amount;
            LoggerService.Write($"[WITHDRAWAL] WITHDRAWAL OF {amount} FROM ACCOUNT => {account.Id}");
            _fileRepository.UpdateRow(account);
            return account.Balance;
        }

        public decimal Balance(Account account)
        {
            LoggerService.Write($"[BALANCE] SHOW BALANCE OF ACCOUNT => {account.Id}");
            return account.Balance;
        }

        public void ChargeInterest(Account account)
        {
            account.ChargeInterest();
            _fileRepository.UpdateRow(account);
        }

        public Account GetAccount(string id)
        {
            var account = Accounts.FirstOrDefault(a => a.Id == id);
            
            if (account != null)
            {
                LoggerService.Write($"[ACCOUNT] GET ACCOUNT => {account.Id}");
            }

            return account;
        }
    }

    public interface IBank
    {
        List<Account> Accounts { get; }
        
        int AccountsCount { get; }
        
        string Name { get; set; }

        Account CreateAccount(string name, AccountType type, decimal balance);

        decimal Deposit(Account account, decimal amount);

        decimal Withdraw(Account account, decimal amount);

        decimal Balance(Account account);

        Account GetAccount(string id);
    }
}