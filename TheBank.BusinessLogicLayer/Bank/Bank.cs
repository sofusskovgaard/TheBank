using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TheBank.DataAccessLayer;
using TheBank.Common.Models.Accounts;
using TheBank.Common.Models.Exceptions;
using TheBank.Common.Models.Transaction;
using TheBank.Services;

namespace TheBank.BusinessLogicLayer.Bank
{
    public class Bank : IBank
    {
        #region Private Fields

        private FileRepository _fileRepository = new FileRepository();

        #endregion

        #region Public Fields
        
        public List<Account> Accounts => _fileRepository.Accounts;
        
        public int AccountsCount => Accounts.Count;

        public List<Transaction> Transactions => _fileRepository.Transactions;

        public int TransactionsCount => Transactions.Count;

        public string Name { get; set; }

        #endregion
        
        public Bank()
        {
            
        }

        /// <summary>
        /// Create a new account
        /// </summary>
        /// <param name="name">Account Name</param>
        /// <param name="accountType">Account Type</param>
        /// <param name="balance">Starting Balance</param>
        /// <returns>Task&lt;Account&gt;</returns>
        /// <exception cref="MissingAccountTypeException">Used to make sure the Account Type exists</exception>
        public async Task<Account> CreateAccount(string name, AccountType accountType, decimal balance = 0M)
        {
            switch (accountType)
            {
                case AccountType.ConsumerAccount:
                    var consumerAccount = await _createConsumerAccount(name, balance);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW CONSUMER ACCOUNT, ID = {consumerAccount.Id}");

                    return consumerAccount;
                
                case AccountType.CheckingAccount:
                    var checkingAccount = await _createCheckingAccount(name, balance);
                    LoggerService.Write($"[NEW_ACCOUNT] NEW CHECKING ACCOUNT, ID = {checkingAccount.Id}");

                    return checkingAccount;
                
                case AccountType.SavingsAccount:
                    var savingsAccount = await _createSavingsAccount(name, balance);

                    return savingsAccount;
                
                default: throw new MissingAccountTypeException("MissingAccountType");
            }
        }
        
        /// <summary>
        /// Deposit funds into account
        /// </summary>
        /// <param name="account">Recipient</param>
        /// <param name="amount">Amount to deposit</param>
        /// <returns>Task&lt;decimal&gt;</returns>
        /// <exception cref="InvalidAmountException">Used to handle bad amount input</exception>
        public async Task<decimal> Deposit(Account account, decimal amount)
        {
            if (amount <= 0) throw new InvalidAmountException("InvalidInput");
            
            account.Balance += amount;
            
            await _fileRepository.UpdateAccount(account);
            await _fileRepository.CreateTransaction(new Transaction() { Id = Guid.NewGuid().ToString(), Recipient = account, Amount = amount }).ContinueWith(
                t =>
                {
                    LoggerService.Write($"[DEPOSIT] DEPOSIT OF {amount} INTO ACCOUNT => {account.Id}");
                }, TaskContinuationOptions.OnlyOnRanToCompletion);

            return account.Balance;
        }

        /// <summary>
        /// Withdraw funds from account
        /// </summary>
        /// <param name="account">Sender</param>
        /// <param name="amount">Amount to withdraw</param>
        /// <returns>Task&lt;decimal&gt;</returns>
        /// <exception cref="InvalidAmountException">Used to handle bad amount input</exception>
        public async Task<decimal> Withdraw(Account account, decimal amount)
        {
            if (amount <= 0) throw new InvalidAmountException("InvalidInput");
            
            account.Balance -= amount;

            await _fileRepository.UpdateAccount(account);
            await _fileRepository.CreateTransaction(new Transaction() { Id = Guid.NewGuid().ToString(), Sender = account, Amount = amount }).ContinueWith(
                t =>
                {
                    LoggerService.Write($"[WITHDRAWAL] WITHDRAWAL OF {amount} FROM ACCOUNT => {account.Id}");
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            
            return account.Balance;
        }

        /// <summary>
        /// Transact funds between accounts
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="recipient">Recipient</param>
        /// <param name="amount">Amount to transact</param>
        /// <returns>Task&lt;decimal&gt;</returns>
        /// <exception cref="InvalidAmountException">Used to handle bad amount input</exception>
        public async Task<decimal> Transact(Account sender, Account recipient, decimal amount)
        {
            if (amount <= 0) throw new InvalidAmountException("InvalidInput");
            
            sender.Balance -= amount + (amount * sender.TransactionFee);
            recipient.Balance += amount;

            await _fileRepository.UpdateAccount(sender);
            await _fileRepository.UpdateAccount(recipient);
            await _fileRepository.CreateTransaction(new Transaction() { Id = Guid.NewGuid().ToString(), Sender = sender, Recipient = recipient, Amount = amount }).ContinueWith(
                t =>
                {
                    LoggerService.Write($"[TRANSACTION] TRANSACTION OF {amount:0.00}, WITH A FEE OF {(amount * sender.TransactionFee):0.00} FROM {sender.Id} TO {recipient.Id}");
                }, TaskContinuationOptions.OnlyOnRanToCompletion);
            
            return sender.Balance;
        }

        /// <summary>
        /// Get account balance
        /// </summary>
        /// <param name="account">Account to inspect</param>
        /// <returns>decimal</returns>
        public decimal Balance(Account account)
        {
            LoggerService.Write($"[BALANCE] SHOW BALANCE OF ACCOUNT => {account.Id}");
            return account.Balance;
        }

        /// <summary>
        /// Charge interest on account
        /// </summary>
        /// <param name="account">Account to charge</param>
        /// <returns>Task</returns>
        public async Task ChargeInterest(Account account)
        {
            var interest = account.ChargeInterest();
            await _fileRepository.UpdateAccount(account);
            await _fileRepository.CreateTransaction(new Transaction() {Id = Guid.NewGuid().ToString(), Recipient = account, Amount = interest}).ContinueWith(
                t =>
                {
                    LoggerService.Write($"[INTEREST] CHARGED INTEREST ON => {account.Id}");
                });
        }

        /// <summary>
        /// Get account by id
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Task&lt;Account&gt;</returns>
        /// <exception cref="Exception"></exception>
        public async Task<Account> GetAccount(string id)
        {
            try
            {
                var account = Accounts.FirstOrDefault(a => a.Id.Substring(0, 8) == id) ?? await _fileRepository.GetAccount(id);

                LoggerService.Write($"[ACCOUNT] FOUND USER => {id}");
                
                return account;
            }
            // TODO: Change exception type 
            catch (Exception e)
            {
                LoggerService.Write($"[ACCOUNT] COULDN'T FIND USER => {id}");
                throw e;
            }
        }

        #region Private Methods

        /// <summary>
        /// Create new account of type ConsumerAccount
        /// </summary>
        /// <param name="name">Account Name</param>
        /// <param name="balance">Starting Balance</param>
        /// <returns>Task&lt;Account&gt;</returns>
        private async Task<Account> _createConsumerAccount(string name, decimal balance)
        {
            var account = new ConsumerAccount(name) {Balance = balance};
            await _fileRepository.CreateAccount(account);
            return account;
        }
        
        /// <summary>
        /// Create new account of type CheckingAccount
        /// </summary>
        /// <param name="name">Account Name</param>
        /// <param name="balance">Starting Balance</param>
        /// <returns>Task&lt;Account&gt;</returns>
        private async Task<Account> _createCheckingAccount(string name, decimal balance)
        {
            var account = new CheckingAccount(name) {Balance = balance};
            await _fileRepository.CreateAccount(account);
            return account;
        }
        
        /// <summary>
        /// Create new account of type SavingsAccount
        /// </summary>
        /// <param name="name">Account Name</param>
        /// <param name="balance">Starting Balance</param>
        /// <returns>Task&lt;Account&gt;</returns>
        private async Task<Account> _createSavingsAccount(string name, decimal balance)
        {
            var account = new SavingsAccount(name) {Balance = balance};
            await _fileRepository.CreateAccount(account);
            return account;
        }

        #endregion
    }

    public interface IBank
    {
        List<Account> Accounts { get; }

        List<Transaction> Transactions { get; }

        Task<Account> CreateAccount(string name, AccountType type, decimal balance);

        Task<decimal> Deposit(Account account, decimal amount);

        Task<decimal> Withdraw(Account account, decimal amount);

        Task ChargeInterest(Account account);

        decimal Balance(Account account);

        Task<Account> GetAccount(string id);
    }
}