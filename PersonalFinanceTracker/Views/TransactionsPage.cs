using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public class TransactionsPage : ContentPage
    {
        private readonly FinanceRepository _repository;
        private readonly CsvImportService _csvImportService;
        private FinanceData _data;

        private readonly DatePicker _datePicker;
        private readonly Entry _amountEntry;
        private readonly Entry _descriptionEntry;
        private readonly Picker _categoryPicker;
        private readonly Picker _typePicker;
        private readonly Picker _filterCategoryPicker;
        private readonly DatePicker _fromPicker;
        private readonly DatePicker _toPicker;
        private readonly StackLayout _transactionStack;

        public TransactionsPage()
        {
            Title = "Transactions";
            BackgroundColor = Ui.Background;
            _repository = FinanceRepository.Instance;
            _csvImportService = new CsvImportService();
            _data = new FinanceData();

            _datePicker = new DatePicker { Date = DateTime.Today };
            _amountEntry = new Entry { Placeholder = "Amount", Keyboard = Keyboard.Numeric };
            _descriptionEntry = new Entry { Placeholder = "Description" };
            _categoryPicker = new Picker { Title = "Category" };
            _typePicker = new Picker { Title = "Type" };
            _filterCategoryPicker = new Picker { Title = "Filter category" };
            _fromPicker = new DatePicker { Date = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) };
            _toPicker = new DatePicker { Date = DateTime.Today };
            _transactionStack = new StackLayout { Spacing = 8 };

            FillCategoryPicker(_categoryPicker, false);
            FillCategoryPicker(_filterCategoryPicker, true);
            _categoryPicker.SelectedIndex = 0;
            _filterCategoryPicker.SelectedIndex = 0;
            _typePicker.Items.Add(TransactionType.Income.ToString());
            _typePicker.Items.Add(TransactionType.Expense.ToString());
            _typePicker.SelectedIndex = 1;

            _filterCategoryPicker.SelectedIndexChanged += (sender, args) => RefreshTransactions();
            _fromPicker.DateSelected += (sender, args) => RefreshTransactions();
            _toPicker.DateSelected += (sender, args) => RefreshTransactions();

            var addButton = Ui.PrimaryButton("Add transaction");
            addButton.Clicked += async (sender, args) => await AddTransactionAsync();

            var importButton = Ui.SecondaryButton("Import CSV");
            importButton.Clicked += async (sender, args) => await ImportCsvAsync();

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Padding = new Thickness(18, 18, 18, 28),
                    Spacing = 8,
                    Children =
                    {
                        Ui.Title("Transactions"),
                        Ui.SectionTitle("Add Transaction"),
                        Ui.Panel(new StackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                _datePicker,
                                _typePicker,
                                _categoryPicker,
                                _amountEntry,
                                _descriptionEntry,
                                addButton
                            }
                        }),
                        Ui.SectionTitle("CSV Import"),
                        Ui.Panel(new StackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                importButton
                            }
                        }),
                        Ui.SectionTitle("Filters"),
                        Ui.Panel(new StackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                _filterCategoryPicker,
                                new Label { Text = "From", TextColor = Ui.Muted, FontSize = 12 },
                                _fromPicker,
                                new Label { Text = "To", TextColor = Ui.Muted, FontSize = 12 },
                                _toPicker
                            }
                        }),
                        Ui.SectionTitle("Transaction List"),
                        _transactionStack
                    }
                }
            };

            MessagingCenter.Subscribe<App>(this, AppEvents.DataChanged, async sender => await LoadAsync());
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                _data = await _repository.LoadAsync();
                RefreshTransactions();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Storage error", ex.Message, "OK");
            }
        }

        private async Task AddTransactionAsync()
        {
            decimal amount;
            if (!TryParseAmount(_amountEntry.Text, out amount) || amount <= 0m)
            {
                await DisplayAlert("Invalid amount", "Enter an amount greater than zero.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_descriptionEntry.Text))
            {
                await DisplayAlert("Missing description", "Enter a short transaction description.", "OK");
                return;
            }

            var transaction = new TransactionRecord
            {
                Date = _datePicker.Date,
                Amount = Math.Abs(amount),
                Category = _categoryPicker.SelectedItem as string ?? FinanceCategories.Miscellaneous,
                Description = _descriptionEntry.Text.Trim(),
                Type = (TransactionType)Enum.Parse(typeof(TransactionType), _typePicker.SelectedItem.ToString()),
                SourceHash = "manual-" + Guid.NewGuid().ToString("N"),
                CreatedAtUtc = DateTime.UtcNow
            };

            _data.Transactions.Add(transaction);
            await _repository.SaveAsync(_data);
            _amountEntry.Text = string.Empty;
            _descriptionEntry.Text = string.Empty;
            RefreshTransactions();
            MessagingCenter.Send((App)Application.Current, AppEvents.DataChanged);
        }

        private async Task ImportCsvAsync()
        {
            try
            {
                var file = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select transaction CSV"
                });

                if (file == null)
                {
                    return;
                }

                string csvText;
                using (var stream = await file.OpenReadAsync())
                using (var reader = new StreamReader(stream))
                {
                    csvText = await reader.ReadToEndAsync();
                }

                var result = _csvImportService.Import(csvText, _data.Transactions);
                if (result.ImportedTransactions.Any())
                {
                    _data.Transactions.AddRange(result.ImportedTransactions);
                    await _repository.SaveAsync(_data);
                }

                RefreshTransactions();
                MessagingCenter.Send((App)Application.Current, AppEvents.DataChanged);

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    "Imported {0} transaction(s). Skipped {1} duplicate(s).",
                    result.ImportedTransactions.Count,
                    result.DuplicateCount);

                if (result.HasErrors)
                {
                    message += Environment.NewLine + Environment.NewLine + string.Join(Environment.NewLine, result.Errors.Take(5));
                }

                await DisplayAlert("CSV import complete", message, "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("CSV import failed", ex.Message, "OK");
            }
        }

        private void RefreshTransactions()
        {
            _transactionStack.Children.Clear();
            var selectedCategory = _filterCategoryPicker.SelectedItem as string;
            var filtered = _data.Transactions
                .Where(t => t.Date.Date >= _fromPicker.Date.Date && t.Date.Date <= _toPicker.Date.Date)
                .Where(t => selectedCategory == "All categories" || string.IsNullOrWhiteSpace(selectedCategory) || t.Category == selectedCategory)
                .OrderByDescending(t => t.Date)
                .ThenBy(t => t.Description)
                .ToList();

            if (!filtered.Any())
            {
                _transactionStack.Children.Add(Ui.Panel(Ui.Body("No transactions match the selected filters.")));
                return;
            }

            foreach (var transaction in filtered)
            {
                _transactionStack.Children.Add(TransactionRow(transaction));
            }
        }

        private static View TransactionRow(TransactionRecord transaction)
        {
            var amountColor = transaction.Type == TransactionType.Income ? Ui.Green : Ui.Red;
            var amountPrefix = transaction.Type == TransactionType.Income ? "+" : "-";
            return Ui.Panel(new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                },
                Children =
                {
                    { new Label { Text = transaction.Description, FontAttributes = FontAttributes.Bold, TextColor = Ui.Text }, 0, 0 },
                    { new Label { Text = amountPrefix + Ui.Money(transaction.Amount), TextColor = amountColor, FontAttributes = FontAttributes.Bold }, 1, 0 },
                    { Ui.Body(string.Format(CultureInfo.CurrentCulture, "{0:d} - {1} - {2}", transaction.Date, transaction.Category, transaction.Type)), 0, 1 }
                }
            });
        }

        private static bool TryParseAmount(string text, out decimal amount)
        {
            return decimal.TryParse(text, NumberStyles.Currency, CultureInfo.CurrentCulture, out amount) ||
                   decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
        }

        private static void FillCategoryPicker(Picker picker, bool includeAll)
        {
            if (includeAll)
            {
                picker.Items.Add("All categories");
            }

            foreach (var category in FinanceCategories.Defaults)
            {
                picker.Items.Add(category);
            }
        }
    }
}
