using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using TheBank.Common.Models.Accounts;
using TheBank.Services;

namespace TheBank.DataAccessLayer
{
    public class FileRepository : IFileRepository
    {
        private static string _directory = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");

        private static string _accountsPath = Path.Combine(_directory, "accounts.csv");

        public FileRepository()
        {
            _InitiateDirectories();
            _InitiateStores();
            _LoadEverything();
            
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(SaveEverything);
        }
        
        #region Data Stores
        
        public List<Account> Accounts { get; set; }
        
        #endregion
        
        #region Main methods

        private void _InitiateStores()
        {
            Accounts = new List<Account>();
        }

        private void _InitiateDirectories()
        {
            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            if (!File.Exists(_accountsPath))
                File.Create(_accountsPath).Dispose();
        }

        private void _LoadEverything()
        {
            LoadAccounts();
        }
        
        private void SaveEverything(object sender, EventArgs e)
        {
            SaveAccounts();
            LoggerService.Write("APPLICATION EXIT EVENT TRIGGERED THE SAVING OF THE DATA STORES");
        }

        #endregion

        #region Accounts

        public void SaveAccounts()
        {
            using (StreamWriter stream = File.CreateText(_accountsPath))
            {
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
    }
    

    public interface IFileRepository
    {
        void AddRow(Account account);
        
        void UpdateRow(Account account);

        void LoadAccounts();
        
        void SaveAccounts();

        void GetRow(string id, out Account account);
    }
}