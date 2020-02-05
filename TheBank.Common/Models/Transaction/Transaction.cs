using TheBank.Common.Models.Accounts;

namespace TheBank.Common.Models.Transaction
{
    public class Transaction : ITransaction
    {
        public string Id { get; set; }

        public Account Sender { get; set; }
        
        public Account Recipient { get; set; }
        
        public decimal Amount { get; set; }
    }

    public interface ITransaction
    {
        string Id { get; set; }

        Account Sender { get; set; }
        
        Account Recipient { get; set; }
        
        decimal Amount { get; set; }
    }
}