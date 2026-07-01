using System;

namespace PersonalFinanceTracker.Models
{
    public class TransactionRecord
    {
        public string Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public TransactionType Type { get; set; }
        public string SourceHash { get; set; }
        public DateTime CreatedAtUtc { get; set; }

        public TransactionRecord()
        {
            Id = Guid.NewGuid().ToString("N");
            Category = FinanceCategories.Miscellaneous;
            Description = string.Empty;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public decimal SignedAmount
        {
            get
            {
                var absolute = Math.Abs(Amount);
                return Type == TransactionType.Income ? absolute : -absolute;
            }
        }
    }
}
