using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class AlertService
    {
        public List<string> BuildBudgetAlerts(IEnumerable<BudgetSnapshot> snapshots)
        {
            return snapshots
                .Where(s => s.Health == BudgetHealth.NearBudget || s.Health == BudgetHealth.OverBudget)
                .Select(snapshot =>
                {
                    if (snapshot.Health == BudgetHealth.OverBudget)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture,
                            "{0} is over budget by {1:C}.",
                            snapshot.Category,
                            Math.Abs(snapshot.Remaining));
                    }

                    return string.Format(
                        CultureInfo.CurrentCulture,
                        "{0} is close to its budget limit ({1:P0} used).",
                        snapshot.Category,
                        snapshot.PercentUsed);
                })
                .ToList();
        }

        public string BuildReminderMessage(ReminderSettings settings, DateTime utcNow)
        {
            if (settings == null || settings.Interval == ReminderInterval.Off)
            {
                return null;
            }

            var reviewAge = utcNow - settings.LastReviewedUtc;
            var lastShownAge = utcNow - settings.LastReminderShownUtc;
            var requiredAge = settings.Interval == ReminderInterval.Weekly
                ? TimeSpan.FromDays(7)
                : TimeSpan.FromDays(30);

            if (reviewAge >= requiredAge && lastShownAge >= TimeSpan.FromHours(12))
            {
                return settings.Interval == ReminderInterval.Weekly
                    ? "Weekly reminder: review your recent spending and update any budget limits."
                    : "Monthly reminder: review this month's spending before setting next month's budgets.";
            }

            return null;
        }
    }
}
