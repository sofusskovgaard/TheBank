using System;
using System.Collections.Generic;
using System.Linq;
using TheBank.DataAccessLayer;
using TheBank.Common.Models.Accounts;
using TheBank.Common.Models.Transaction;
using TheBank.Services;

namespace TheBank.BusinessLogicLayer.Bank
{
    public class Bank : IBank
    {
        private FileRepository _fileRepository = new FileRepository();
        
        private int _lastId = 0;

        public List<Account> Accounts => _fileRepository.Accounts;

        public List<Transaction> Transactions => _fileRepository.Transactions;

        public int AccountsCount => Accounts.Count;

        public int TransactionsCount => Transactions.Count;
        
        public string Name { get; set; }

        public Account CreateAccount(string name, AccountType accountType, decimal balance = 0M)
        {
            _lastId += 1;

            switch (accountType)
            {
                case AccountType.ConsumerAccount:
                    var newConsumerAccount = new ConsumerAccount(name) { Balance = balance};
                    _fileRepository.AddRow(newConsumerAccount);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW CONSUMER ACCOUNT, ID = {newConsumerAccount.Id}");
                    return newConsumerAccount;
                case AccountType.CheckingAccount:
                    var newCheckingAccount = new CheckingAccount(name) { Balance = balance};
                    _fileRepository.AddRow(newCheckingAccount);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW CHECKING ACCOUNT, ID = {newCheckingAccount.Id}");
                    return newCheckingAccount;
                case AccountType.SavingsAccount:
                    var newSavingsAccount = new SavingsAccount(name) { Balance = balance};
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
            _fileRepository.AddRow(new Transaction() { Id = Guid.NewGuid().ToString(), Reciever = account, Amount = amount });
            return account.Balance;
        }

        public decimal Withdraw(Account account, decimal amount)
        {
            account.Balance -= amount;
            LoggerService.Write($"[WITHDRAWAL] WITHDRAWAL OF {amount} FROM ACCOUNT => {account.Id}");
            _fileRepository.UpdateRow(account);
            _fileRepository.AddRow(new Transaction() { Id = Guid.NewGuid().ToString(), Sender = account, Amount = amount });
            return account.Balance;
        }

        public decimal Transact(Account sender, Account reciever, decimal amount)
        {
            sender.Balance -= amount;
            reciever.Balance += amount;
            LoggerService.Write($"[TRANSACTION] TRANSACTION OF {amount} FROM {sender.Id} TO {reciever.Id}");
            _fileRepository.UpdateRow(sender);
            _fileRepository.UpdateRow(reciever);
            _fileRepository.AddRow(new Transaction() { Id = Guid.NewGuid().ToString(), Sender = sender, Reciever = reciever, Amount = amount });
            return sender.Balance;
        }

        public decimal Balance(Account account)
        {
            LoggerService.Write($"[BALANCE] SHOW BALANCE OF ACCOUNT => {account.Id}");
            return account.Balance;
        }

        public void ChargeInterest(Account account)
        {
            var interest = account.ChargeInterest();
            _fileRepository.UpdateRow(account);
            _fileRepository.AddRow(new Transaction() { Id = Guid.NewGuid().ToString(), Reciever = account, Amount = interest });
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