using System;

namespace PersonalFinanceTracker.Models
{
    public class CategoryBudget
    {
        public string Id { get; set; }
        public string MonthKey { get; set; }
        public string Category { get; set; }
        public decimal Limit { get; set; }

        public CategoryBudget()
        {
            Id = Guid.NewGuid().ToString("N");
            MonthKey = DateTime.Today.ToString("yyyy-MM");
            Category = FinanceCategories.Miscellaneous;
        }
    }
}
