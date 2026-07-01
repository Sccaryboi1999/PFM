using System;
using System.Collections.Generic;

namespace PersonalFinanceTracker.Models
{
    public class FinanceData
    {
        public List<TransactionRecord> Transactions { get; set; }
        public List<CategoryBudget> Budgets { get; set; }
        public ReminderSettings ReminderSettings { get; set; }
        public DateTime DemoSeededUtc { get; set; }

        public FinanceData()
        {
            Transactions = new List<TransactionRecord>();
            Budgets = new List<CategoryBudget>();
            ReminderSettings = new ReminderSettings();
            DemoSeededUtc = DateTime.MinValue;
        }
    }
}
