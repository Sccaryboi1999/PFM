using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PersonalFinanceTracker.Models;
using Xamarin.Essentials;

namespace PersonalFinanceTracker.Services
{
    public class FinanceRepository
    {
        private const string StorageKey = "personal-finance-tracker.secure-data.v1";
        private static readonly Lazy<FinanceRepository> LazyInstance =
            new Lazy<FinanceRepository>(() => new FinanceRepository());

        public static FinanceRepository Instance
        {
            get { return LazyInstance.Value; }
        }

        private FinanceRepository()
        {
        }

        public async Task<FinanceData> LoadAsync()
        {
            var json = await SecureStorage.GetAsync(StorageKey).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new FinanceData();
            }

            var data = JsonConvert.DeserializeObject<FinanceData>(json) ?? new FinanceData();
            data.Transactions = data.Transactions ?? new System.Collections.Generic.List<TransactionRecord>();
            data.Budgets = data.Budgets ?? new System.Collections.Generic.List<CategoryBudget>();
            data.ReminderSettings = data.ReminderSettings ?? new ReminderSettings();
            return data;
        }

        public Task SaveAsync(FinanceData data)
        {
            var json = JsonConvert.SerializeObject(data, Formatting.None);
            return SecureStorage.SetAsync(StorageKey, json);
        }

        public void Clear()
        {
            SecureStorage.Remove(StorageKey);
        }
    }
}
