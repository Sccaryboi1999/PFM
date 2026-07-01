using System;
using System.Linq;
using System.Threading.Tasks;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class DemoDataSeeder
    {
        private readonly FinanceRepository _repository;

        public DemoDataSeeder(FinanceRepository repository)
        {
            _repository = repository;
        }

        public async Task EnsureSeededAsync()
        {
            var data = await _repository.LoadAsync().ConfigureAwait(false);
            if (data.Transactions.Any() || data.Budgets.Any())
            {
                return;
            }

            var today = DateTime.Today;
            var monthKey = BudgetEngine.ToMonthKey(today);

            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Food, Limit = 500m });
            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Transportation, Limit = 220m });
            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Housing, Limit = 1400m });
            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Entertainment, Limit = 180m });
            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Bills, Limit = 320m });
            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Savings, Limit = 600m });
            data.Budgets.Add(new CategoryBudget { MonthKey = monthKey, Category = FinanceCategories.Miscellaneous, Limit = 150m });

            Add(data, today.AddDays(-14), 3900m, FinanceCategories.Miscellaneous, "Paycheck", TransactionType.Income);
            Add(data, today.AddDays(-12), 1400m, FinanceCategories.Housing, "Rent", TransactionType.Expense);
            Add(data, today.AddDays(-10), 86.42m, FinanceCategories.Food, "Grocery run", TransactionType.Expense);
            Add(data, today.AddDays(-9), 48.20m, FinanceCategories.Transportation, "Fuel", TransactionType.Expense);
            Add(data, today.AddDays(-8), 172.33m, FinanceCategories.Bills, "Electric bill", TransactionType.Expense);
            Add(data, today.AddDays(-6), 63.18m, FinanceCategories.Food, "Meal prep supplies", TransactionType.Expense);
            Add(data, today.AddDays(-5), 54.00m, FinanceCategories.Entertainment, "Movie night", TransactionType.Expense);
            Add(data, today.AddDays(-3), 600m, FinanceCategories.Savings, "Emergency fund transfer", TransactionType.Expense);
            Add(data, today.AddDays(-2), 279.50m, FinanceCategories.Food, "Warehouse groceries", TransactionType.Expense);
            Add(data, today.AddDays(-1), 38.75m, FinanceCategories.Miscellaneous, "Household items", TransactionType.Expense);

            data.DemoSeededUtc = DateTime.UtcNow;
            await _repository.SaveAsync(data).ConfigureAwait(false);
        }

        private static void Add(FinanceData data, DateTime date, decimal amount, string category, string description, TransactionType type)
        {
            data.Transactions.Add(new TransactionRecord
            {
                Date = date.Date,
                Amount = amount,
                Category = category,
                Description = description,
                Type = type,
                SourceHash = "seed-" + date.ToString("yyyyMMdd") + "-" + description.Replace(" ", string.Empty).ToLowerInvariant(),
                CreatedAtUtc = DateTime.UtcNow
            });
        }
    }
}
