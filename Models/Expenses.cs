namespace WebJourney.Models
{
    public class Expenses
    {
        public int Id { get; set; } 
        public string Name { get; set; }
        public double Value { get; set; }
        public string Currency { get; set; }
        public Expenses() { }
        public Expenses(string name, double value, string currency):this()
        {
            Name = name;
            Value = value;
            Currency = currency;
        }
    }
}
