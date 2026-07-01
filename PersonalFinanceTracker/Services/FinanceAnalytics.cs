using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PersonalFinanceTracker.Models;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Services
{
    public class FinanceAnalytics
    {
        private readonly Color[] _palette =
        {
            Color.FromHex("#2F855A"),
            Color.FromHex("#2B6CB0"),
            Color.FromHex("#B7791F"),
            Color.FromHex("#805AD5"),
            Color.FromHex("#C53030"),
            Color.FromHex("#319795"),
            Color.FromHex("#4A5568")
        };

        public List<ChartItem> SpendingByCategory(FinanceData data, string monthKey)
        {
            return data.Transactions
                .Where(t => t.Type == TransactionType.Expense && BudgetEngine.ToMonthKey(t.Date) == monthKey)
                .GroupBy(t => t.Category)
                .OrderByDescending(g => g.Sum(t => Math.Abs(t.Amount)))
                .Select((g, index) => new ChartItem
                {
                    Label = g.Key,
                    Value = g.Sum(t => Math.Abs(t.Amount)),
                    Detail = string.Format(CultureInfo.CurrentCulture, "{0:C} spent on {1}", g.Sum(t => Math.Abs(t.Amount)), g.Key),
                    Color = _palette[index % _palette.Length]
                })
                .ToList();
        }

        public List<ChartItem> IncomeVsExpenses(FinanceData data, string monthKey)
        {
            var summary = SummarizeMonth(data, monthKey);
            return new List<ChartItem>
            {
                new ChartItem
                {
                    Label = "Income",
                    Value = summary.Income,
                    Detail = string.Format(CultureInfo.CurrentCulture, "{0:C} income for {1}", summary.Income, monthKey),
                    Color = Color.FromHex("#2F855A")
                },
                new ChartItem
                {
                    Label = "Expenses",
                    Value = summary.Expenses,
                    Detail = string.Format(CultureInfo.CurrentCulture, "{0:C} expenses for {1}", summary.Expenses, monthKey),
                    Color = Color.FromHex("#C53030")
                }
            };
        }

        public List<ChartItem> SpendingTrend(FinanceData data, DateTime month)
        {
            var first = new DateTime(month.Year, month.Month, 1).AddMonths(-5);
            var months = Enumerable.Range(0, 6)
                .Select(offset => first.AddMonths(offset))
                .ToList();

            return months.Select((date, index) =>
            {
                var key = BudgetEngine.ToMonthKey(date);
                var expenses = data.Transactions
                    .Where(t => t.Type == TransactionType.Expense && BudgetEngine.ToMonthKey(t.Date) == key)
                    .Sum(t => Math.Abs(t.Amount));

                return new ChartItem
                {
                    Label = date.ToString("MMM", CultureInfo.CurrentCulture),
                    Value = expenses,
                    Detail = string.Format(CultureInfo.CurrentCulture, "{0:C} total spending in {1}", expenses, date.ToString("MMMM yyyy", CultureInfo.CurrentCulture)),
                    Color = _palette[index % _palette.Length]
                };
            }).ToList();
        }

        public MonthlySummary SummarizeMonth(FinanceData data, string monthKey)
        {
            var transactions = data.Transactions
                .Where(t => BudgetEngine.ToMonthKey(t.Date) == monthKey)
                .ToList();

            return new MonthlySummary
            {
                MonthKey = monthKey,
                Income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => Math.Abs(t.Amount)),
                Expenses = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => Math.Abs(t.Amount))
            };
        }
    }
}
