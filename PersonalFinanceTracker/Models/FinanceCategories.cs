using System;
using System.Collections.Generic;
using System.Linq;

namespace PersonalFinanceTracker.Models
{
    public static class FinanceCategories
    {
        public const string Food = "Food";
        public const string Transportation = "Transportation";
        public const string Housing = "Housing";
        public const string Entertainment = "Entertainment";
        public const string Bills = "Bills";
        public const string Savings = "Savings";
        public const string Miscellaneous = "Miscellaneous";

        public static readonly IReadOnlyList<string> Defaults = new[]
        {
            Food,
            Transportation,
            Housing,
            Entertainment,
            Bills,
            Savings,
            Miscellaneous
        };

        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Miscellaneous;
            }

            var trimmed = value.Trim();
            var match = Defaults.FirstOrDefault(category =>
                string.Equals(category, trimmed, StringComparison.OrdinalIgnoreCase));

            return match ?? Miscellaneous;
        }
    }
}
