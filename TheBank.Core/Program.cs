using System;
using System.Data;
using System.Linq;
using ConsoleTableExt;

namespace TheBank.Core
{
    class Program
    {
        private static Bank _bank;
        
        static void Main(string[] args)
        {
            _bank = new Bank() { BankName = "Skovgaard's Bank'"};
            
            Menu();
        }
        
        static void Menu()
        {
            bool running = true;

            do
            {
                Console.Clear(); // Clear screen for the menu
                
                Console.WriteLine($"Velkommen til {_bank.BankName} - Bank 2");
                Console.WriteLine("----------------------------------------");

                Console.WriteLine($"01) Create account");
                Console.WriteLine($"02) Deposit funds");
                Console.WriteLine($"03) Withdraw funds");
                Console.WriteLine($"04) Display balance");
                Console.WriteLine($"05) Display accounts");
                Console.WriteLine($"00) Exit bank");

                var userInput = Console.ReadLine();
                
                switch (userInput)
                {
                    case "01":
                        CreateAccount();
                        break;
                    case "02":
                        DepositFunds();
                        break;
                    case "03":
                        WithdrawFunds();
                        break;
                    case "04":
                        DisplayBalance();
                        break;
                    case "05":
                        DisplayAccounts();
                        break;
                    case "00":
                        running = false;
                        break;
                    default:
                        break;
                }
            } while (running);
        }

        static void CreateAccount()
        {
            Console.Write("New account name: ");
            var accountName = Console.ReadLine();

            if (string.IsNullOrEmpty(accountName))
            {
                Console.WriteLine("I need a name idiot...");    
            }
            else
            {
                var account = _bank.CreateAccount(accountName);
                Console.WriteLine($"New account for {account.Name}, with an id of {account.Id} and a balance of {account.Balance} kr");
            }
            
            Console.Read();
        }

        static void DepositFunds()
        {
            try
            {
                var account = _GetUser();
                
                Console.Write("Amount to deposit: ");
                decimal.TryParse(Console.ReadLine(), out decimal amount);

                if (amount == 0)
                {
                    Console.WriteLine("No deposit was made");
                }
                else
                {
                    _bank.Deposit(account, amount);
                    Console.WriteLine($"Balance after deposit {account.Balance} kr");
                }
            
                Console.Read();
            }
            catch (Exception ex)
            {
                if (ex.Message != "CancelledAccountSearch")
                {
                    throw ex;
                }
            }
        }

        static void WithdrawFunds()
        {
            try
            {
                var account = _GetUser();
                
                Console.Write("Amount to withdraw: ");
                decimal.TryParse(Console.ReadLine(), out decimal amount);

                if (amount == 0)
                {
                    Console.WriteLine("No withdrawal was made");
                }
                else
                {
                    _bank.Withdraw(account, amount);
                    Console.WriteLine($"Balance after withdrawal {account.Balance} kr");
                }

                Console.Read();
            }
            catch (Exception ex)
            {
                if (ex.Message != "CancelledAccountSearch")
                {
                    throw ex;
                }
            }
        }

        static void DisplayBalance()
        {
            try
            {
                var account = _GetUser();
            
                Console.WriteLine($"Balance: {account.Balance} kr");
                Console.Read();
            }
            catch (Exception ex)
            {
                if (ex.Message != "CancelledAccountSearch")
                {
                    throw ex;
                }
            }
        }

        static void DisplayAccounts()
        {
            Console.Clear();
            
            if (_bank.AccountsCount > 0)
            {
                Console.WriteLine($"Registered accounts: ({_bank.AccountsCount})");
                
                var table = new DataTable();

                table.Columns.Add("ID", typeof(string));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Balance", typeof(decimal));
            
            
                _bank.Accounts.ForEach(account =>
                {
                    table.Rows.Add(account.Id, account.Name, account.Balance);
                });

                ConsoleTableBuilder.From(table).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered accounts...");
            }

            Console.Read();
        }
        
        //#region private methods

        static Account _GetUser()
        {
            Console.Write("AccountID: ");
            var userInput = Console.ReadLine();
            
            if (string.IsNullOrEmpty(userInput))
            {
                throw new Exception("CancelledAccountSearch");    
            }
            
            var account = _bank.GetAccount(userInput);

            if (account == null)
            {
                Console.Clear();
                Console.WriteLine("That ID doesn't exists");
                return _GetUser();
            }

            return account;
        }
        
        // #endregion
    }
}