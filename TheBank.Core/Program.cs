using System;

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
                Console.WriteLine($"New account for {account.Name}, with a balance of {account.Balance} kr");
            }
            
            Console.Read();
        }

        static void DepositFunds()
        {
            Console.Write("Amount to deposit: ");
            decimal.TryParse(Console.ReadLine(), out decimal amount);

            if (amount == 0)
            {
                Console.WriteLine("No deposit was made");
            }
            else
            {
                _bank.Deposit(amount);
                Console.WriteLine($"Balance after deposit {_bank.Account.Balance} kr");
            }
            
            Console.Read();
        }

        static void WithdrawFunds()
        {
            Console.Write("Amount to withdraw: ");
            decimal.TryParse(Console.ReadLine(), out decimal amount);
            
            if (amount == 0)
            {
                Console.WriteLine("No withdrawal was made");
            }
            else
            {
                _bank.Withdraw(amount);
                Console.WriteLine($"Balance after withdrawal {_bank.Account.Balance} kr");
            }
            
            Console.Read();
        }

        static void DisplayBalance()
        {
            Console.WriteLine($"Balance: {_bank.Account.Balance} kr");
            Console.Read();
        }
    }
}