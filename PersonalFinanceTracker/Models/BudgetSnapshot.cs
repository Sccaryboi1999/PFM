namespace PersonalFinanceTracker.Models
{
    public class BudgetSnapshot
    {
        public string MonthKey { get; set; }
        public string Category { get; set; }
        public decimal Limit { get; set; }
        public decimal Spent { get; set; }
        public decimal Remaining { get; set; }
        public decimal PercentUsed { get; set; }
        public BudgetHealth Health { get; set; }

        public string StatusLabel
        {
            get
            {
                if (Health == BudgetHealth.OverBudget)
                {
                    return "Over budget";
                }

                if (Health == BudgetHealth.NearBudget)
                {
                    return "Near budget";
                }

                return "Under budget";
            }
        }
    }
}
