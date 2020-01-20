namespace TheBank.Core
{
    public class Account
    {
        public Account(string name = "John Doe")
        {
            Name = name;
            Balance = 0;
        }
        
        public string Name { get; set; }
        
        public decimal Balance { get; set; }
    }
}