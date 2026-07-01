using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PersonalFinanceTracker.Models;

namespace PersonalFinanceTracker.Services
{
    public class CsvImportService
    {
        public CsvImportResult Import(string csvText, IEnumerable<TransactionRecord> existingTransactions)
        {
            var result = new CsvImportResult();
            var rows = ParseCsv(csvText);
            if (rows.Count < 2)
            {
                result.Errors.Add("CSV must include a header row and at least one transaction row.");
                return result;
            }

            var header = rows[0].Select(NormalizeHeader).ToList();
            var dateIndex = FindIndex(header, "date", "transactiondate", "posteddate", "postingdate");
            var descriptionIndex = FindIndex(header, "description", "memo", "name", "merchant", "payee");
            var amountIndex = FindIndex(header, "amount", "transactionamount", "value");
            var debitIndex = FindIndex(header, "debit", "withdrawal", "charge");
            var creditIndex = FindIndex(header, "credit", "deposit", "payment");
            var categoryIndex = FindIndex(header, "category", "spendingcategory");
            var typeIndex = FindIndex(header, "type", "transactiontype");

            if (dateIndex < 0)
            {
                result.Errors.Add("Missing a date column. Accepted headers include date, transaction date, or posted date.");
            }

            if (descriptionIndex < 0)
            {
                result.Errors.Add("Missing a description column. Accepted headers include description, memo, merchant, or payee.");
            }

            if (amountIndex < 0 && debitIndex < 0 && creditIndex < 0)
            {
                result.Errors.Add("Missing an amount column. Accepted headers include amount, debit, withdrawal, credit, or deposit.");
            }

            if (result.HasErrors)
            {
                return result;
            }

            var knownHashes = new HashSet<string>(
                existingTransactions
                    .Where(t => !string.IsNullOrWhiteSpace(t.SourceHash))
                    .Select(t => t.SourceHash),
                StringComparer.OrdinalIgnoreCase);

            for (var i = 1; i < rows.Count; i++)
            {
                var rowNumber = i + 1;
                var row = rows[i];
                if (row.All(string.IsNullOrWhiteSpace))
                {
                    continue;
                }

                result.ParsedRowCount++;
                var transaction = BuildTransaction(row, rowNumber, dateIndex, descriptionIndex, amountIndex, debitIndex, creditIndex, categoryIndex, typeIndex, result.Errors);
                if (transaction == null)
                {
                    continue;
                }

                if (knownHashes.Contains(transaction.SourceHash))
                {
                    result.DuplicateCount++;
                    continue;
                }

                knownHashes.Add(transaction.SourceHash);
                result.ImportedTransactions.Add(transaction);
            }

            return result;
        }

        private TransactionRecord BuildTransaction(
            List<string> row,
            int rowNumber,
            int dateIndex,
            int descriptionIndex,
            int amountIndex,
            int debitIndex,
            int creditIndex,
            int categoryIndex,
            int typeIndex,
            List<string> errors)
        {
            DateTime date;
            var dateText = GetCell(row, dateIndex);
            if (!DateTime.TryParse(dateText, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date) &&
                !DateTime.TryParse(dateText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out date))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, "Row {0}: date '{1}' is not valid.", rowNumber, dateText));
                return null;
            }

            var description = GetCell(row, descriptionIndex).Trim();
            if (string.IsNullOrWhiteSpace(description))
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, "Row {0}: description is required.", rowNumber));
                return null;
            }

            decimal rawAmount;
            var amountType = TryGetAmount(row, amountIndex, debitIndex, creditIndex, out rawAmount);
            if (amountType == null)
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, "Row {0}: amount is not valid.", rowNumber));
                return null;
            }

            var transactionType = ResolveType(GetCell(row, typeIndex), rawAmount, amountType.Value);
            var category = FinanceCategories.Normalize(GetCell(row, categoryIndex));
            var absoluteAmount = Math.Abs(rawAmount);

            if (absoluteAmount <= 0m)
            {
                errors.Add(string.Format(CultureInfo.CurrentCulture, "Row {0}: amount must be greater than zero.", rowNumber));
                return null;
            }

            var sourceHash = BuildHash(date, description, absoluteAmount, category, transactionType);
            return new TransactionRecord
            {
                Date = date.Date,
                Description = description,
                Amount = absoluteAmount,
                Category = category,
                Type = transactionType,
                SourceHash = sourceHash,
                CreatedAtUtc = DateTime.UtcNow
            };
        }

        private static TransactionType? TryGetAmount(
            List<string> row,
            int amountIndex,
            int debitIndex,
            int creditIndex,
            out decimal rawAmount)
        {
            rawAmount = 0m;

            if (amountIndex >= 0)
            {
                if (TryParseMoney(GetCell(row, amountIndex), out rawAmount))
                {
                    return rawAmount < 0m ? TransactionType.Expense : TransactionType.Income;
                }

                return null;
            }

            decimal debit;
            if (debitIndex >= 0 && TryParseMoney(GetCell(row, debitIndex), out debit) && debit != 0m)
            {
                rawAmount = -Math.Abs(debit);
                return TransactionType.Expense;
            }

            decimal credit;
            if (creditIndex >= 0 && TryParseMoney(GetCell(row, creditIndex), out credit) && credit != 0m)
            {
                rawAmount = Math.Abs(credit);
                return TransactionType.Income;
            }

            return null;
        }

        private static TransactionType ResolveType(string typeText, decimal rawAmount, TransactionType inferred)
        {
            if (!string.IsNullOrWhiteSpace(typeText))
            {
                var normalized = typeText.Trim().ToLowerInvariant();
                if (normalized.Contains("expense") || normalized.Contains("debit") || normalized.Contains("withdraw"))
                {
                    return TransactionType.Expense;
                }

                if (normalized.Contains("income") || normalized.Contains("credit") || normalized.Contains("deposit"))
                {
                    return TransactionType.Income;
                }
            }

            if (rawAmount < 0m)
            {
                return TransactionType.Expense;
            }

            return inferred;
        }

        private static bool TryParseMoney(string value, out decimal amount)
        {
            amount = 0m;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            var cleaned = value.Trim()
                .Replace("$", string.Empty)
                .Replace(",", string.Empty)
                .Replace("(", "-")
                .Replace(")", string.Empty);

            return decimal.TryParse(cleaned, NumberStyles.Number | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out amount) ||
                   decimal.TryParse(cleaned, NumberStyles.Currency, CultureInfo.CurrentCulture, out amount);
        }

        private static string BuildHash(DateTime date, string description, decimal amount, string category, TransactionType type)
        {
            var normalized = string.Format(
                CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd}|{1}|{2:0.00}|{3}|{4}",
                date.Date,
                description.Trim().ToUpperInvariant(),
                amount,
                category.Trim().ToUpperInvariant(),
                type);

            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(normalized));
                return BitConverter.ToString(bytes).Replace("-", string.Empty);
            }
        }

        private static int FindIndex(IReadOnlyList<string> header, params string[] names)
        {
            for (var i = 0; i < header.Count; i++)
            {
                if (names.Contains(header[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private static string NormalizeHeader(string value)
        {
            return new string((value ?? string.Empty)
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        private static string GetCell(List<string> row, int index)
        {
            if (index < 0 || index >= row.Count)
            {
                return string.Empty;
            }

            return row[index] ?? string.Empty;
        }

        private static List<List<string>> ParseCsv(string csvText)
        {
            var rows = new List<List<string>>();
            if (string.IsNullOrWhiteSpace(csvText))
            {
                return rows;
            }

            using (var reader = new StringReader(csvText))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    rows.Add(ParseLine(line));
                }
            }

            return rows;
        }

        private static List<string> ParseLine(string line)
        {
            var cells = new List<string>();
            var current = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];
                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (ch == ',' && !inQuotes)
                {
                    cells.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(ch);
                }
            }

            cells.Add(current.ToString());
            return cells;
        }
    }
}
