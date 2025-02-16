﻿using System;
using System.Collections.Generic;
using System.Data;

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
        
        static void Menu()
        {
            bool running = true;
            do
            {
                Console.Clear(); // Clear screen for the menu
                
                Console.WriteLine($"Welcome to {_bank.Name}");
                Console.WriteLine("----------------------------------------");

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
                        CreateAccount();
                        break;
                    case ConsoleKey.D2:
                        DepositFunds();
                        break;
                    case ConsoleKey.D3:
                        WithdrawFunds();
                        break;
                    case ConsoleKey.D4:
                        TransactFunds();
                        break;
                    case ConsoleKey.D5:
                        DisplayBalance();
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

        static void CreateAccount()
        {
            Console.Write("New account name: ");
            var accountName = Console.ReadLine();
            Console.Clear();

            if (string.IsNullOrEmpty(accountName))
            {
                return;
            }

            Console.WriteLine("New account type: (1)Consumer Account, (2)Checking Account, (3)Savings Account");
            var accountType = Console.ReadKey();
            Console.Clear();

            if (accountType.Key == ConsoleKey.D1 || accountType.Key == ConsoleKey.D2 || accountType.Key == ConsoleKey.D3)
            {
                switch (accountType.Key)
                {
                    case ConsoleKey.D1:
                        var consumerAccount = _bank.CreateAccount(accountName, AccountType.ConsumerAccount);
                        Console.WriteLine($"New consumer account for {consumerAccount.Name}, with an id of {consumerAccount.Id} and a balance of {consumerAccount.Balance} kr");
                        break;
                    case ConsoleKey.D2:
                        var checkingAccount = _bank.CreateAccount(accountName, AccountType.CheckingAccount);
                        Console.WriteLine($"New checking account for {checkingAccount.Name}, with an id of {checkingAccount.Id} and a balance of {checkingAccount.Balance} kr");
                        break;
                    case ConsoleKey.D3:
                        var savingsAccount = _bank.CreateAccount(accountName, AccountType.SavingsAccount);
                        Console.WriteLine($"New savings account for {savingsAccount.Name}, with an id of {savingsAccount.Id} and a balance of {savingsAccount.Balance} kr");
                        break;
                }
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
                Console.Clear();

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
                if (ex.Message != "CancelledAccountSearch" && !(ex is OverdraftException))
                {
                    throw ex;
                }

                if (ex is OverdraftException)
                {
                    Console.Clear();
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
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
                Console.Clear();

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
                if (ex.Message != "CancelledAccountSearch" && !(ex is OverdraftException))
                {
                    throw ex;
                }

                if (ex is OverdraftException)
                {
                    Console.Clear();
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                }
            }
        }

        static void TransactFunds()
        {
            try
            {
                Console.WriteLine("Sender account");
                var sender = _GetUser();
                
                Console.WriteLine("Reciever account");
                var reciever = _GetUser();
                
                Console.Write("Amount to transact: ");
                decimal.TryParse(Console.ReadLine(), out decimal amount);
                Console.Clear();

                if (amount == 0)
                {
                    Console.WriteLine("No withdrawal was made");
                }
                else
                {
                    _bank.Transact(sender, reciever, amount);
                    Console.WriteLine($"Balance after transaction {sender.Balance:0.00} kr");
                }

                Console.Read();
            }
            catch (Exception ex)
            {
                if (ex.Message != "CancelledAccountSearch" && !(ex is OverdraftException))
                {
                    throw ex;
                }

                if (ex is OverdraftException)
                {
                    Console.Clear();
                    Console.WriteLine(ex.Message);
                    Console.ReadKey();
                }
            }
        }

        static void ChargeInterests()
        {
            if (_bank.AccountsCount > 0)
            {
                Console.WriteLine($"Accounts charged: ({_bank.AccountsCount})");
                    
                var table = new DataTable();

                table.Columns.Add("ID", typeof(string));
                table.Columns.Add("Old Balance", typeof(string));
                table.Columns.Add("New Balance", typeof(string));
                table.Columns.Add("Differential", typeof(string));
                table.Columns.Add("Interest", typeof(string));
            
            
                _bank.Accounts.ForEach(account =>
                {
                    var oldBalance = account.Balance;
                    _bank.ChargeInterest(account);
                    var newBalance = account.Balance;
                    table.Rows.Add(account.Id, $"{oldBalance} kr", $"{newBalance} kr", $"{oldBalance - newBalance} kr", $"{account.InterestRate * 100} %");
                });

                ConsoleTableBuilder.From(table).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered accounts...");
            }
            
            Console.Read();
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
                table.Columns.Add("Balance", typeof(string));
                table.Columns.Add("Account Type", typeof(string));
            
            
                _bank.Accounts.ForEach(account =>
                {
                    table.Rows.Add(account.Id, account.Name, $"{account.Balance} kr", account.Type.ToString());
                });

                ConsoleTableBuilder.From(table).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered accounts...");
            }

            Console.Read();
        }

        static void DisplayLogs()
        {
            var logs = LoggerService.Read();
            Console.WriteLine("Logs: ");
            logs.ForEach(log => Console.WriteLine(log));
            Console.ReadKey();
        }

        static void DisplayTransactions()
        {
            Console.Clear();
            
            if (_bank.TransactionsCount > 0)
            {
                Console.WriteLine($"Registered transactions: ({_bank.TransactionsCount})");
                
                var table = new DataTable();

                table.Columns.Add("ID", typeof(string));
                table.Columns.Add("Sender", typeof(string));
                table.Columns.Add("Reciever", typeof(string));
                table.Columns.Add("Amount", typeof(string));
            
            
                _bank.Transactions.ForEach(transaction =>
                {
                    table.Rows.Add(transaction.Id.Substring(0, 12), transaction.Sender?.Id ?? "null", transaction.Reciever?.Id ?? "null", transaction.Amount.ToString("0.00"));
                });

                ConsoleTableBuilder.From(table).WithFormat(ConsoleTableBuilderFormat.Minimal).ExportAndWriteLine();
            }
            else
            {
                Console.WriteLine("There are no registered transactions...");
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

            Console.Clear();
            return account;
        }
        
        // #endregion
    }
}