namespace PersonalFinanceTracker.Models
{
    public class MonthlySummary
    {
        public string MonthKey { get; set; }
        public decimal Income { get; set; }
        public decimal Expenses { get; set; }

        public decimal Net
        {
            get { return Income - Expenses; }
        }
    }
}
