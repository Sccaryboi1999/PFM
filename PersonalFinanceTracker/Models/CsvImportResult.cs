using System.Collections.Generic;

namespace PersonalFinanceTracker.Models
{
    public class CsvImportResult
    {
        public List<TransactionRecord> ImportedTransactions { get; set; }
        public List<string> Errors { get; set; }
        public int DuplicateCount { get; set; }
        public int ParsedRowCount { get; set; }

        public CsvImportResult()
        {
            ImportedTransactions = new List<TransactionRecord>();
            Errors = new List<string>();
        }

        public bool HasErrors
        {
            get { return Errors.Count > 0; }
        }
    }
}
