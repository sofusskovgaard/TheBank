using System;

namespace TheBank.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            Bank bank = new Bank() { BankName = "Skovgaard's Bank'"};

            Console.WriteLine($"Velkommen til {bank.BankName} - Bank 1");
            OpretKonto(bank);
            Indsæt(bank);
            Hæv(bank);
        }

        static void OpretKonto(Bank bank)
        {
            var account = bank.CreateAccount("Sofus Skovgaard");
            Console.WriteLine($"Ny konto oprettet til {account.Name} med saldoen {account.Balance} kr");
        }

        static void Indsæt(Bank bank)
        {
            bank.Deposit(500M);
            Console.WriteLine($"Kontoens saldo efter indsæt {bank.Account.Balance} kr");
        }

        static void Hæv(Bank bank)
        {
            bank.Withdraw(200M);
            Console.WriteLine($"Kontoens saldo efter hæv {bank.Account.Balance} kr");
        }
    }
}