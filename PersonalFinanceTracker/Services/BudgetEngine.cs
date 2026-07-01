using System;
using System.Collections.Generic;
using System.Linq;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class BudgetEngine
    {
        public const decimal NearBudgetThreshold = 0.85m;

        public List<BudgetSnapshot> BuildSnapshots(FinanceData data, string monthKey)
        {
            var expenses = data.Transactions
                .Where(t => t.Type == TransactionType.Expense && ToMonthKey(t.Date) == monthKey)
                .GroupBy(t => t.Category)
                .ToDictionary(g => g.Key, g => g.Sum(t => Math.Abs(t.Amount)));

            return data.Budgets
                .Where(b => b.MonthKey == monthKey)
                .OrderBy(b => b.Category)
                .Select(b =>
                {
                    var spent = expenses.ContainsKey(b.Category) ? expenses[b.Category] : 0m;
                    var percent = b.Limit <= 0m ? 0m : spent / b.Limit;
                    return new BudgetSnapshot
                    {
                        MonthKey = monthKey,
                        Category = b.Category,
                        Limit = b.Limit,
                        Spent = spent,
                        Remaining = b.Limit - spent,
                        PercentUsed = percent,
                        Health = GetHealth(percent)
                    };
                })
                .ToList();
        }

        public BudgetHealth GetHealth(decimal percentUsed)
        {
            if (percentUsed > 1m)
            {
                return BudgetHealth.OverBudget;
            }

            if (percentUsed >= NearBudgetThreshold)
            {
                return BudgetHealth.NearBudget;
            }

            return BudgetHealth.UnderBudget;
        }

        public static string ToMonthKey(DateTime date)
        {
            return date.ToString("yyyy-MM");
        }
    }
}
