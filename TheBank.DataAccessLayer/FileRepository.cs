using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TheBank.Common.Models.Accounts;
using TheBank.Common.Models.Exceptions;
using TheBank.Common.Models.Transaction;
using TheBank.Services;

namespace TheBank.DataAccessLayer
{
    public class FileRepository : IFileRepository
    {

        #region Private Fields
        
        private static string _directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

        private static string _accountsPath = Path.Combine(_directory, "accounts.csv");
        
        private static string _transactionsPath = Path.Combine(_directory, "transactions.csv");

        private readonly object _accountsLock = new object();
        
        private readonly object _transactionsLock = new object();

        #endregion

        public FileRepository()
        {
            _InitiateDirectories();
            _InitiateStores();
            _LoadEverything();
            
            _InitiateAutoSaving();
            
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(SaveEverything);
        }
        
        #region Data Stores
        
        public List<Account> Accounts { get; set; }
        
        public List<Transaction> Transactions { get; set; }
        
        #endregion
        
        #region Main methods

        /// <summary>
        /// Initiate data stores
        /// </summary>
        private void _InitiateStores()
        {
            Accounts = new List<Account>();
            Transactions = new List<Transaction>();
        }

        /// <summary>
        /// Check if directories and files exist (and create them)
        /// </summary>
        private void _InitiateDirectories()
        {
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            if (!File.Exists(_accountsPath))
                File.Create(_accountsPath).Dispose();
            
            if (!File.Exists(_transactionsPath))
                File.Create(_transactionsPath).Dispose();
        }

        /// <summary>
        /// Load data from files
        /// </summary>
        private void _LoadEverything()
        {
            LoadAccounts();
            LoadTransactions();
        }
        
        /// <summary>
        /// Save everything on ProcessExit event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        private void SaveEverything(object sender, EventArgs e)
        {
            SaveAccounts();
            SaveTransactions();
            LoggerService.Write("APPLICATION EXIT EVENT TRIGGERED THE SAVING OF THE DATA STORES");
        }

        #endregion

        #region Accounts

        /// <summary>
        /// Save accounts to file
        /// </summary>
        /// <returns>Task</returns>
        public Task SaveAccounts()
        {
            return Task.Run(() =>
            {
                lock (_accountsLock)
                {
                    using (StreamWriter stream = File.CreateText(_accountsPath))
                    {
                        stream.WriteLine("ID;Name;Type;Balance");

                        Accounts.ForEach(account =>
                            stream.WriteLine($"{account.Id};{account.Name};{(int) account.Type};{account.Balance}"));
                    }
                }
            });
        }

        /// <summary>
        /// Load accounts from file
        /// </summary>
        /// <returns>Task</returns>
        public Task LoadAccounts()
        {
            return Task.Run(() =>
            {
                lock (_accountsLock)
                {
                    using (StreamReader stream = new StreamReader(_accountsPath))
                    {
                        while (!stream.EndOfStream)
                        {
                            var line = stream.ReadLine();
                            if (line == "ID;Name;Type;Balance") continue;
                            var values = line.Split(";");

                            if (!decimal.TryParse(values[3], out decimal balance)) continue;
                    
                            if (values[2] == "0")
                                Accounts.Add(new ConsumerAccount(values[0], values[1]) {Balance = balance});
                            else if (values[2] == "1")
                                Accounts.Add(new CheckingAccount(values[0], values[1]) {Balance = balance});
                            else if (values[2] == "2")
                                Accounts.Add(new SavingsAccount(values[0], values[1]) {Balance = balance});
                        }
                    }
                }
            });
        }
        
        /// <summary>
        /// Create account and save to file
        /// </summary>
        /// <param name="account">Account to save</param>
        /// <returns>Task</returns>
        public Task CreateAccount(Account account)
        {
            return Task.Run(() =>
            {
                using (var stream = new StreamWriter(_accountsPath, true))
                {
                    stream.WriteLine($"{account.Id};{account.Name};{(int) account.Type};{account.Balance}");
                    lock (_accountsLock)
                    {
                        Accounts.Add(account);
                    }
                }
            });
        }

        /// <summary>
        /// Update account and edit in file
        /// </summary>
        /// <param name="account">Account to update</param>
        /// <returns>Task</returns>
        public Task UpdateAccount(Account account)
        {
            return Task.Run(() =>
            {
                lock (_accountsLock)
                {
                    var lines = new List<string>();

                    using (var stream = new StreamReader(_accountsPath))
                    {
                        while (!stream.EndOfStream)
                        {
                            var line = stream.ReadLine();
                            if (line == "ID;Name;Type;Balance") continue;
                            var values = line.Split(";");

                            if (values[0] == account.Id)
                            {
                                values[1] = account.Name;
                                values[2] = $"{(int) account.Type}";
                                values[3] = account.Balance.ToString("0.00");
                                line = string.Join(";", values);
                            }

                            lines.Add(line);
                        }
                    }

                    using (var stream = new StreamWriter(_accountsPath, false))
                    {
                        stream.WriteLine("ID;Name;Type;Balance");
                        lines.ForEach(line => stream.WriteLine(line));
                    }
                }
            });
        }

        /// <summary>
        /// Get account from file
        /// </summary>
        /// <param name="id">Account ID</param>
        /// <returns>Task&lt;Account&gt;</returns>
        /// <exception cref="MissingAccountException">Used to handle when account doesn't exist</exception>
        public Task<Account> GetAccount(string id)
        {
            return Task.Run(new Func<Account>(() =>
            {
                lock (_accountsLock)
                {
                    using (StreamReader stream = new StreamReader(_accountsPath))
                    {
                        while (!stream.EndOfStream)
                        {
                            var line = stream.ReadLine();
                            if (line == "ID;Name;Type;Balance") continue;
                            var values = line.Split(";");

                            if (values[0] == id)
                            {
                                int.TryParse(values[2], out int _type);
                                var type = (AccountType) _type;
                                decimal.TryParse(values[2], out decimal balance);
                                switch (type)
                                {
                                    case AccountType.ConsumerAccount:
                                        return new ConsumerAccount(values[0], values[1]) {Balance = balance};
                                        break;
                                    case AccountType.CheckingAccount:
                                        return new CheckingAccount(values[0], values[1]) {Balance = balance};
                                        break;
                                    case AccountType.SavingsAccount:
                                        return new SavingsAccount(values[0], values[1]) {Balance = balance};
                                        break;
                                }
                            }
                        }
                    }
                    
                    throw new MissingAccountException("MissingAccountException");
                }
            }));
        }
        
        #endregion
        
        #region Transactions

        /// <summary>
        /// Save transactions to file
        /// </summary>
        /// <returns>Task</returns>
        public Task SaveTransactions()
        {
            return Task.Run(() =>
            {
                using (StreamWriter stream = File.CreateText(_transactionsPath))
                {
                    stream.WriteLine("ID;Sender;Recipient;Amount");
            
                    lock (_transactionsLock)
                    {
                            Transactions.ForEach(transaction =>
                            {
                                if (transaction.Recipient != null && transaction.Sender != null)
                                    stream.WriteLine($"{transaction.Id};{transaction.Sender.Id};{transaction.Recipient.Id};{transaction.Amount:0.00}");
                                else if (transaction.Recipient == null && transaction.Sender != null)
                                    stream.WriteLine($"{transaction.Id};{transaction.Sender.Id};null;{transaction.Amount:0.00}");
                                else if (transaction.Recipient != null && transaction.Sender == null )
                                    stream.WriteLine($"{transaction.Id};null;{transaction.Recipient.Id};{transaction.Amount:0.00}");
                            });
                    }
                }
            });
        }

        /// <summary>
        /// Load transactions from file
        /// </summary>
        /// <returns>Task</returns>
        public async Task LoadTransactions()
        {
            using (StreamReader stream = new StreamReader(_transactionsPath))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (line == "ID;Sender;Recipient;Amount" || string.IsNullOrEmpty(line)) continue;
                    var values = line.Split(";");

                    if (!decimal.TryParse(values[3], out decimal balance)) continue;

                    var transaction = new Transaction();

                    transaction.Id = values[0];
                    transaction.Amount = balance;

                    if (values[1] != "null")
                    {
                        var sender = await GetAccount(values[1]);
                        transaction.Sender = sender;
                    }

                    if (values[2] != "null")
                    {
                        var reciever = await GetAccount(values[2]);
                        transaction.Recipient = reciever;
                    }

                    lock (_transactionsLock)
                    {
                        Transactions.Add(transaction);
                    }
                }
            }
        }

        /// <summary>
        /// Create transaction and save to file
        /// </summary>
        /// <param name="transaction">Transaction to save</param>
        /// <returns>Task</returns>
        public Task CreateTransaction(Transaction transaction)
        {
            return Task.Run(() =>
            {
                lock (_transactionsLock)
                {
                    using (var stream = new StreamWriter(_transactionsPath, true))
                    {
                        if (transaction.Recipient != null && transaction.Sender != null)
                            stream.WriteLine(
                                $"{transaction.Id};{transaction.Sender.Id};{transaction.Recipient.Id};{transaction.Amount:0.00}");
                        else if (transaction.Recipient == null && transaction.Sender != null)
                            stream.WriteLine(
                                $"{transaction.Id};{transaction.Sender.Id};null;{transaction.Amount:0.00}");
                        else if (transaction.Recipient != null && transaction.Sender == null)
                            stream.WriteLine(
                                $"{transaction.Id};null;{transaction.Recipient.Id};{transaction.Amount:0.00}");

                        Transactions.Add(transaction);
                    }
                }    
            });
        }

        /// <summary>
        /// Update transaction and edit file
        /// </summary>
        /// <param name="transaction">Transaction to update</param>
        /// <returns>Task</returns>
        public Task UpdateTransaction(Transaction transaction)
        {
            return Task.Run(() =>
            {
                lock (_transactionsLock)
                {
                    var lines = new List<string>();
            
                    using (var stream = new StreamReader(_transactionsPath))
                    {
                        while (!stream.EndOfStream)
                        {
                            var line = stream.ReadLine();
                            var values = line.Split(";");

                            if (values[0] == transaction.Id)
                            {
                                values[1] = transaction.Sender?.Id ?? "null";
                                values[2] = transaction.Recipient?.Id ?? "null";
                                values[3] = transaction.Amount.ToString("0.00");
                                line = string.Join(";", values);
                            }
                            lines.Add(line);
                        }
                    }

                    using (var stream = new StreamWriter(_transactionsPath, false))
                    {
                        stream.WriteLine("ID;Sender;Recipient;Amount");
                        lines.ForEach(line => stream.WriteLine(line));
                    }
                }
            });
        }

        /// <summary>
        /// Get transaction from file
        /// </summary>
        /// <param name="id">Transaction ID</param>
        /// <returns>Task&lt;Transaction&gt;</returns>
        /// <exception cref="MissingTransactionException">Used to handle when transaction doesn't exist</exception>
        public async Task<Transaction> GetTransaction(string id)
        {
            using (StreamReader stream = new StreamReader(_transactionsPath))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (line == "ID;Sender;Recipient;Amount") continue;
                    var values = line.Split(";");

                    if (values[0] == id)
                    {
                        decimal.TryParse(values[3], out decimal balance);

                        var _transaction = new Transaction();

                        _transaction.Id = values[0];
                        _transaction.Amount = balance;

                        if (values[1] != "null")
                        {
                            var sender = await GetAccount(values[1]);
                            _transaction.Sender = sender;
                        }

                        if (values[2] != "null")
                        {
                            var reciever = await GetAccount(values[2]);
                            _transaction.Recipient = reciever;
                        }

                        return _transaction;
                    }
                }
            }
            
            throw new MissingTransactionException("MissingTransactionException");
        }

        #endregion

        #region Autosaving

        /// <summary>
        /// Initiate task/thread to automatically save data
        /// </summary>
        private void _InitiateAutoSaving()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    await SaveAccounts();
                    LoggerService.Write($"[{Thread.CurrentThread.ManagedThreadId}] AutoSaved Accounts");
                    
                    await SaveTransactions();
                    LoggerService.Write($"[{Thread.CurrentThread.ManagedThreadId}] AutoSaved Transactions");
                    
                    await Task.Delay(5 * 60 * 1000);
                }
            });
        }

        #endregion

    }
    

    public interface IFileRepository
    {
        Task CreateAccount(Account account);

        Task CreateTransaction(Transaction transaction);

        Task UpdateAccount(Account account);

        Task UpdateTransaction(Transaction transaction);

        Task LoadAccounts();

        Task LoadTransactions();
        
        Task SaveAccounts();

        Task SaveTransactions();

        Task<Account> GetAccount(string id);

        Task<Transaction> GetTransaction(string id);
    }
}