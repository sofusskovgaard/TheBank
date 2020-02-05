using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ConsoleTableExt;

using TheBank.Common.Models.Accounts;
using TheBank.BusinessLogicLayer.Bank;
using TheBank.Common.Models.Exceptions;
using TheBank.Services;

namespace TheBank.ConsoleGUI
{
    class Program
    {
        private static Bank _bank;

        static void Main(string[] args)
        {
            _bank = new Bank() { Name = "Bank2000"};

            Menu();
        }
        
        /// <summary>
        /// Initiate user interactive menu.
        /// </summary>
        static void Menu()
        {
            bool running = true;
            do
            {
                Console.Clear(); // Clear screen for the menu
                
                Console.WriteLine($"Welcome to {_bank.Name}");
                Console.WriteLine("-----------------------");

                Console.WriteLine($"1) Create account");
                Console.WriteLine($"2) Deposit funds");
                Console.WriteLine($"3) Withdraw funds");
                Console.WriteLine($"4) Transact funds");
                Console.WriteLine($"5) Display balance");
                Console.WriteLine($"6) Display accounts");
                Console.WriteLine($"7) Display transactions");
                Console.WriteLine($"8) Display logs");
                Console.WriteLine($"9) Charge interests");
                Console.WriteLine($"0) Exit");

                var userInput = Console.ReadKey();
                Console.Clear();
                
                switch (userInput.Key)
                {
                    case ConsoleKey.D1:
                        CreateAccount().Wait();
                        break;
                    case ConsoleKey.D2:
                        DepositFunds().Wait();
                        break;
                    case ConsoleKey.D3:
                        WithdrawFunds().Wait();
                        break;
                    case ConsoleKey.D4:
                        TransactFunds().Wait();
                        break;
                    case ConsoleKey.D5:
                        DisplayBalance().Wait();
                        break;
                    case ConsoleKey.D6:
                        DisplayAccounts();
                        break;
                    case ConsoleKey.D7:
                        DisplayTransactions();
                        break;
                    case ConsoleKey.D8:
                        DisplayLogs();
                        break;
                    case ConsoleKey.D9:
                        ChargeInterests();
                        break;
                    case ConsoleKey.D0:
                        running = false;
                        break;
                    default:
                        break;
                }
            } while (running);
        }

        /// <summary>
        /// Initiate account creation process.
        /// </summary>
        static async Task CreateAccount()
        {
            Console.Write("New account name: ");
            var accountName = Console.ReadLine();
            Console.Clear();

            if (string.IsNullOrEmpty(accountName)) return;

            Console.WriteLine("1) Consumer Account");
            Console.WriteLine("2) Checking Account");
            Console.WriteLine("3) Saving Account");
            Console.Write("New account type:");
            var accountType = Console.ReadKey();
            
            Console.Clear();

            switch (accountType.Key)
            {
                case ConsoleKey.D1:
                    var consumerAccount = await _bank.CreateAccount(accountName, AccountType.ConsumerAccount);

                    Console.WriteLine("New consumer account:");
                    Console.WriteLine("---------------------");
                    Console.WriteLine($"Account ID:\t{consumerAccount.Id}");
                    Console.WriteLine($"Account Name:\t{consumerAccount.Name}");
                    break;
                
                case ConsoleKey.D2:
                    var checkingAccount = await _bank.CreateAccount(accountName, AccountType.CheckingAccount);

                    Console.WriteLine("New checking account:");
                    Console.WriteLine("---------------------");
                    Console.WriteLine($"Account ID:\t{checkingAccount.Id}");
                    Console.WriteLine($"Account Name:\t{checkingAccount.Name}");
                    break;
                
                case ConsoleKey.D3:
                    var savingsAccount = await _bank.CreateAccount(accountName, AccountType.SavingsAccount);

                    Console.WriteLine("New savings account:");
                    Console.WriteLine("---------------------");
                    Console.WriteLine($"Account ID:\t{savingsAccount.Id}");
                    Console.WriteLine($"Account Name:\t{savingsAccount.Name}");
                    break;
                
                default: return;
            }

            Console.ReadKey();
        }

        /// <summary>
        /// Initiate deposit of funds
        /// </summary>
        /// <returns>Task</returns>
        static async Task DepositFunds()
        {
            try
            {
                var account = await _GetUser();

                Console.Clear();

                Console.Write("Amount to deposit: ");
                decimal.TryParse(Console.ReadLine(), out decimal amount);

                Console.Clear();

                if (amount == 0)
                {
                    Console.WriteLine("No deposit was made");
                }
                else
                {
                    var deposit = _bank.Deposit(account, amount);

                    _loading(deposit);

                    Console.Clear();

                    Console.WriteLine(deposit.IsFaulted
                        ? "Can't do that here bud"
                        : $"Balance after deposit {account.Balance} kr");
                }

                Console.Read();
            }
            catch (OverdraftException ex)
            {
                Console.Clear();
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            catch (CancelledAccountSearchException ex)
            {
                return;
            }
        }

        /// <summary>
        /// Initiate withdrawal of funds
        /// </summary>
        /// <returns>Task</returns>
        static async Task WithdrawFunds()
        {
            try
            {
                var account = await _GetUser();
                
                Console.Write("Amount to withdraw: ");
                decimal.TryParse(Console.ReadLine(), out decimal amount);

                Console.Clear();

                if (amount == 0)
                {
                    Console.WriteLine("No withdrawal was made");
                }
                else
                {
                    var withdraw = _bank.Withdraw(account, amount);

                    _loading(withdraw);
                    
                    Console.Clear();
                    
                    Console.WriteLine(withdraw.IsFaulted
                        ? "Can't do that here bud"
                        :$"Balance after withdrawal {account.Balance} kr");
                }

                Console.Read();
            }
            catch (OverdraftException ex)
            {
                Console.Clear();
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            catch (CancelledAccountSearchException ex)
            {
                return;
            }
        }

        
        /// <summary>
        /// Initiate transaction of funds
        /// </summary>
        /// <returns>Task</returns>
        static async Task TransactFunds()
        {
            try
            {
                Console.WriteLine("Sender account");
                var sender = await _GetUser();

                Console.WriteLine("Recipient account");
                var recipient = await _GetUser();

                Console.Write("Amount to transact: ");
                decimal.TryParse(Console.ReadLine(), out decimal amount);

                Console.Clear();

                if (amount == 0)
                {
                    Console.WriteLine("No withdrawal was made");
                }
                else
                {
                    var oldBalance = sender.Balance;
                    
                    var transaction = _bank.Transact(sender, recipient, amount);

                    _loading(transaction);

                    Console.Clear();

                    if (transaction.IsFaulted)
                    {
                        Console.WriteLine("Can't do that here bud");
                    }
                    else
                    {
                        Console.WriteLine("New Transaction");
                        Console.WriteLine("---------------");
                        Console.WriteLine($"Old Balance: \t{oldBalance:0.00}");
                        Console.WriteLine($"New Balance: \t{sender.Balance:0.00}");
                        Console.WriteLine($"Process Fee: \t{(amount * sender.TransactionFee):0.00}");
                    }
                }

                Console.Read();
            }
            catch (OverdraftException ex)
            {
                Console.Clear();
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
            catch (CancelledAccountSearchException ex)
            {
                return;
            }
        }

        /// <summary>
        /// Initiate interest charging sequence and view changes
        /// </summary>
        static void ChargeInterests()
        {
            if (_bank.AccountsCount > 0)
            {
                Console.WriteLine($"Accounts charged: ({_bank.AccountsCount})");
                    
                var table = new DataTable();

                table.Columns.Add("ID", typeof(string));
                table.Columns.Add("ThreadID", typeof(string));
                table.Columns.Add("Old Balance", typeof(string));
                table.Columns.Add("New Balance", typeof(string));
                table.Columns.Add("Differential", typeof(string));
                table.Columns.Add("Interest", typeof(string));
                
                var tasks = new List<Task>();

                _bank.Accounts.OrderBy(x => x.Id).ToList().ForEach(account =>
                {
                    var task = new Task(() =>
                    {
                        var oldBalance = account.Balance;

                        _bank.ChargeInterest(account);

                        var newBalance = account.Balance;

                        table.Rows.Add(account.Id.Substring(0, 8), Thread.CurrentThread.ManagedThreadId, $"{oldBalance} DKK", $"{newBalance} DKK", $"{oldBalance - newBalance} DKK", $"{account.InterestRate * 100} %");
                    });
                    
                    task.Start();
                    
                    tasks.Add(task);
                });

                Task.WaitAll(tasks.ToArray());

                ConsoleTableBuilder.From(table).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered accounts...");
            }
            
            Console.Read();
        }

        /// <summary>
        /// Initiate balance view
        /// </summary>
        static async Task DisplayBalance()
        {
            try
            {
                var account = await _GetUser();
                
                Console.WriteLine($"Balance: {account.Balance} kr");
                Console.Read();
            } 
            catch (CancelledAccountSearchException ex)
            {
                return;
            }
        }

        /// <summary>
        /// Initiate overview of accounts
        /// </summary>
        static void DisplayAccounts()
        {
            Console.Clear();
            
            if (_bank.AccountsCount > 0)
            {
                Console.WriteLine($"Registered accounts: ({_bank.AccountsCount})");
                
                var table = new DataTable();

                table.Columns.Add("ID", typeof(string));
                table.Columns.Add("Name", typeof(string));
                table.Columns.Add("Balance", typeof(string));
                table.Columns.Add("Account Type", typeof(string));
            
            
                _bank.Accounts.ForEach(account =>
                {
                    table.Rows.Add(account.Id.Substring(0, 8), account.Name, $"{account.Balance} kr", account.Type.ToString());
                });

                ConsoleTableBuilder.From(table).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered accounts...");
            }

            Console.Read();
        }

        /// <summary>
        /// Initiate overview of logs
        /// </summary>
        static void DisplayLogs()
        {
            var logs = LoggerService.Read();
            Console.WriteLine("Logs: ");
            logs.ForEach(log => Console.WriteLine(log));
            Console.ReadKey();
        }

        /// <summary>
        /// Initiate overview of transactions
        /// </summary>
        static void DisplayTransactions()
        {
            Console.Clear();
            
            if (_bank.TransactionsCount > 0)
            {
                Console.WriteLine($"Registered transactions: ({_bank.TransactionsCount})");
                
                var table = new DataTable();

                table.Columns.Add("ID", typeof(string));
                table.Columns.Add("Sender", typeof(string));
                table.Columns.Add("Recipient", typeof(string));
                table.Columns.Add("Amount", typeof(string));
            
            
                _bank.Transactions.ForEach(transaction =>
                {
                    table.Rows.Add(transaction.Id.Substring(0, 8), transaction.Sender?.Id.Substring(0, 8) ?? "null", transaction.Recipient?.Id.Substring(0, 8) ?? "null", transaction.Amount.ToString("0.00"));
                });

                ConsoleTableBuilder.From(table).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered transactions...");
            }

            Console.Read();
        }
        
        #region private methods

        /// <summary>
        /// Initiate account search sequence
        /// </summary>
        /// <returns>Account</returns>
        /// <exception cref="CancelledAccountSearchException">Used to handle cancellation of account search</exception>
        private static async Task<Account> _GetUser()
        {
            Console.Write("AccountID: ");
            var userInput = Console.ReadLine();
        
            if (string.IsNullOrEmpty(userInput))
            {
                throw new CancelledAccountSearchException("CancelledAccountSearch");    
            }

            try
            {
                var account = await _bank.GetAccount(userInput);
                Console.Clear();
                return account;
            }
            catch
            {
                Console.Clear();
                Console.WriteLine("That ID doesn't exists");
                var account = await _GetUser();
                return account;
            }
        }

        /// <summary>
        /// Initiate loading sequence
        /// </summary>
        /// <param name="task">Task to wait for</param>
        private static void _loading(Task task)
        {
            try
            {
                int i = 5;
                bool completed = false;
                
                while (!completed)
                {
                    Console.SetCursorPosition(0, 0);
                    Console.Write($"{Math.Abs(i / (Console.WindowWidth - 1))}%");
                
                    Console.SetCursorPosition(4, 0);
                    Console.Write("[");
                
                    Console.SetCursorPosition(Console.WindowWidth, 0);
                    Console.Write("]");

                    Console.SetCursorPosition(i, 0);
                    Console.Write("█");
                    Thread.Sleep(10);

                    i++;

                    if (task.IsCompleted)
                    {
                        Thread.Sleep(new Random().Next(1, Math.Abs(500 / i)));
                        if (i >= Console.WindowWidth) completed = true;
                    }
                    else
                    {
                        Thread.Sleep(new Random().Next(175, 500));
                    }
                }
            }
            catch
            {
                return;
            }
        }
        
        #endregion
    }
}