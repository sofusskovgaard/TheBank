using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheBank.Common.Models.Accounts;
using TheBank.Common.Models.Transaction;
using TheBank.Services;

namespace TheBank.DataAccessLayer
{
    public class FileRepository : IFileRepository
    {
        private static string _directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

        private static string _accountsPath = Path.Combine(_directory, "accounts.csv");
        
        private static string _transactionsPath = Path.Combine(_directory, "transactions.csv");

        public FileRepository()
        {
            _InitiateDirectories();
            _InitiateStores();
            _LoadEverything();
            
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(SaveEverything);
        }
        
        #region Data Stores
        
        public List<Account> Accounts { get; set; }
        
        public List<Transaction> Transactions { get; set; }
        
        #endregion
        
        #region Main methods

        private void _InitiateStores()
        {
            Accounts = new List<Account>();
            Transactions = new List<Transaction>();
        }

        private void _InitiateDirectories()
        {
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            if (!File.Exists(_accountsPath))
                File.Create(_accountsPath).Dispose();
            
            if (!File.Exists(_transactionsPath))
                File.Create(_transactionsPath).Dispose();
        }

        private void _LoadEverything()
        {
            LoadAccounts();
            LoadTransactions();
        }
        
        private void SaveEverything(object sender, EventArgs e)
        {
            SaveAccounts();
            SaveTransactions();
            LoggerService.Write("APPLICATION EXIT EVENT TRIGGERED THE SAVING OF THE DATA STORES");
        }

        #endregion

        #region Accounts

        public void SaveAccounts()
        {
            using (StreamWriter stream = File.CreateText(_accountsPath))
            {
                stream.WriteLine("ID;Name;Type;Balance");
                
                Accounts.ForEach(account => stream.WriteLine($"{account.Id};{account.Name};{(int)account.Type};{account.Balance}"));
            }
        }

        public void LoadAccounts()
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
        
        public void AddRow(Account account)
        {
            using (var stream = new StreamWriter(_accountsPath, true))
            {
                stream.WriteLine($"{account.Id};{account.Name};{(int)account.Type};{account.Balance}");
                Accounts.Add(account);
            }
        }

        public void UpdateRow(Account account)
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
                        values[2] = $"{(int)account.Type}";
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

        public void GetRow(string id, out Account account)
        {
            bool foundAccount = false;
            account = null;
            
            using (StreamReader stream = new StreamReader(_accountsPath))
            {
                while (!stream.EndOfStream && !foundAccount)
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
                                account = new ConsumerAccount(values[0], values[1]) { Balance = balance };
                                break;
                            case AccountType.CheckingAccount:
                                account = new CheckingAccount(values[0], values[1]) { Balance = balance };
                                break;
                            case AccountType.SavingsAccount:
                                account = new SavingsAccount(values[0], values[1]) { Balance = balance };
                                break;
                            default:
                                break;
                        }

                        foundAccount = true;
                    }
                }
            }
        }
        
        #endregion
        
        #region Transactions

        public void SaveTransactions()
        {
            using (StreamWriter stream = File.CreateText(_transactionsPath))
            {
                stream.WriteLine("ID;Sender;Reciever;Amount");
                
                Transactions.ForEach(transaction =>
                {
                    if (transaction.Reciever != null && transaction.Sender != null)
                        stream.WriteLine($"{transaction.Id};{transaction.Sender.Id};{transaction.Reciever.Id};{transaction.Amount:0.00}");
                    else if (transaction.Reciever == null && transaction.Sender != null)
                        stream.WriteLine($"{transaction.Id};{transaction.Sender.Id};null;{transaction.Amount:0.00}");
                    else if (transaction.Reciever != null && transaction.Sender == null )
                        stream.WriteLine($"{transaction.Id};null;{transaction.Reciever.Id};{transaction.Amount:0.00}");
                });
            }
        }

        public void LoadTransactions()
        {
            using (StreamReader stream = new StreamReader(_transactionsPath))
            {
                while (!stream.EndOfStream)
                {
                    var line = stream.ReadLine();
                    if (line == "ID;Sender;Reciever;Amount") continue;
                    var values = line.Split(";");

                    if (!decimal.TryParse(values[3], out decimal balance)) continue;

                    var transaction = new Transaction();

                    transaction.Id = values[0];
                    transaction.Amount = balance;

                    if (values[1] != "null")
                    {
                        GetRow(values[1], out Account sender);
                        transaction.Sender = sender;
                    }

                    if (values[2] != "null")
                    {
                        GetRow(values[2], out Account reciever);
                        transaction.Reciever = reciever;
                    }

                    Transactions.Add(transaction);
                }
            }
        }

        public void AddRow(Transaction transaction)
        {
            using (var stream = new StreamWriter(_transactionsPath, true))
            {
                if (transaction.Reciever != null && transaction.Sender != null)
                    stream.WriteLine($"{transaction.Id};{transaction.Sender.Id};{transaction.Reciever.Id};{transaction.Amount:0.00}");
                else if (transaction.Reciever == null && transaction.Sender != null)
                    stream.WriteLine($"{transaction.Id};{transaction.Sender.Id};null;{transaction.Amount:0.00}");
                else if (transaction.Reciever != null && transaction.Sender == null )
                    stream.WriteLine($"{transaction.Id};null;{transaction.Reciever.Id};{transaction.Amount:0.00}");
                
                Transactions.Add(transaction);
            }
        }

        public void UpdateRow(Transaction transaction)
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
                        values[2] = transaction.Reciever?.Id ?? "null";
                        values[3] = transaction.Amount.ToString("0.00");
                        line = string.Join(";", values);
                    }
                    lines.Add(line);
                }
            }

            using (var stream = new StreamWriter(_transactionsPath, false))
            {
                stream.WriteLine("ID;Sender;Reciever;Amount");
                lines.ForEach(line => stream.WriteLine(line));
            }
        }
        
        public void GetRow(string id, out Transaction transaction)
        {
            bool foundTransaction = false;
            transaction = null;
            
            using (StreamReader stream = new StreamReader(_transactionsPath))
            {
                while (!stream.EndOfStream && !foundTransaction)
                {
                    var line = stream.ReadLine();
                    if (line == "ID;Sender;Reciever;Amount") continue;
                    var values = line.Split(";");

                    if (values[0] == id)
                    {
                        decimal.TryParse(values[3], out decimal balance);
                        
                        transaction = new Transaction();

                        transaction.Id = values[0];
                        transaction.Amount = balance;

                        if (values[1] != "null")
                        {
                            GetRow(values[1], out Account sender);
                            transaction.Sender = sender;
                        }

                        if (values[2] != "null")
                        {
                            GetRow(values[2], out Account reciever);
                            transaction.Reciever = reciever;
                        }

                        foundTransaction = true;
                    }
                }
            }
        }

        #endregion
    }
    

    public interface IFileRepository
    {
        void AddRow(Account account);

        void AddRow(Transaction transaction);

        void UpdateRow(Account account);

        void UpdateRow(Transaction transaction);

        void LoadAccounts();

        void LoadTransactions();
        
        void SaveAccounts();

        void SaveTransactions();

        void GetRow(string id, out Account account);

        void GetRow(string id, out Transaction transaction);
    }
}