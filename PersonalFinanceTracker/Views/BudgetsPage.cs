using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public class BudgetsPage : ContentPage
    {
        private readonly FinanceRepository _repository;
        private readonly BudgetEngine _budgetEngine;
        private FinanceData _data;

        private readonly DatePicker _monthPicker;
        private readonly Picker _categoryPicker;
        private readonly Entry _limitEntry;
        private readonly StackLayout _budgetStack;

        public BudgetsPage()
        {
            Title = "Budgets";
            BackgroundColor = Ui.Background;
            _repository = FinanceRepository.Instance;
            _budgetEngine = new BudgetEngine();
            _data = new FinanceData();

            _monthPicker = new DatePicker { Date = DateTime.Today, Format = "MMMM yyyy" };
            _categoryPicker = new Picker { Title = "Category" };
            _limitEntry = new Entry { Placeholder = "Monthly limit", Keyboard = Keyboard.Numeric };
            _budgetStack = new StackLayout { Spacing = 8 };

            foreach (var category in FinanceCategories.Defaults)
            {
                _categoryPicker.Items.Add(category);
            }

            _categoryPicker.SelectedIndex = 0;
            _monthPicker.DateSelected += (sender, args) =>
            {
                PrefillSelectedBudget();
                RefreshBudgets();
            };
            _categoryPicker.SelectedIndexChanged += (sender, args) => PrefillSelectedBudget();

            var saveButton = Ui.PrimaryButton("Save category limit");
            saveButton.Clicked += async (sender, args) => await SaveBudgetAsync();

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Padding = new Thickness(18, 18, 18, 28),
                    Spacing = 8,
                    Children =
                    {
                        Ui.Title("Monthly Budgets"),
                        Ui.SectionTitle("Budget Editor"),
                        Ui.Panel(new StackLayout
                        {
                            Spacing = 8,
                            Children =
                            {
                                _monthPicker,
                                _categoryPicker,
                                _limitEntry,
                                saveButton
                            }
                        }),
                        Ui.SectionTitle("Current Month Status"),
                        _budgetStack
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
                PrefillSelectedBudget();
                RefreshBudgets();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Storage error", ex.Message, "OK");
            }
        }

        private async Task SaveBudgetAsync()
        {
            decimal limit;
            if (!decimal.TryParse(_limitEntry.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out limit) &&
                !decimal.TryParse(_limitEntry.Text, NumberStyles.Number, CultureInfo.InvariantCulture, out limit))
            {
                await DisplayAlert("Invalid limit", "Enter a monthly budget limit.", "OK");
                return;
            }

            if (limit <= 0m)
            {
                await DisplayAlert("Invalid limit", "Budget limits must be greater than zero.", "OK");
                return;
            }

            var monthKey = BudgetEngine.ToMonthKey(_monthPicker.Date);
            var category = _categoryPicker.SelectedItem as string ?? FinanceCategories.Miscellaneous;
            var existing = _data.Budgets.FirstOrDefault(b => b.MonthKey == monthKey && b.Category == category);
            if (existing == null)
            {
                _data.Budgets.Add(new CategoryBudget
                {
                    MonthKey = monthKey,
                    Category = category,
                    Limit = limit
                });
            }
            else
            {
                existing.Limit = limit;
            }

            await _repository.SaveAsync(_data);
            RefreshBudgets();
            MessagingCenter.Send((App)Application.Current, AppEvents.DataChanged);
            await DisplayAlert("Budget saved", string.Format(CultureInfo.CurrentCulture, "{0} limit is now {1:C}.", category, limit), "OK");
        }

        private void PrefillSelectedBudget()
        {
            var monthKey = BudgetEngine.ToMonthKey(_monthPicker.Date);
            var category = _categoryPicker.SelectedItem as string;
            var budget = _data.Budgets.FirstOrDefault(b => b.MonthKey == monthKey && b.Category == category);
            _limitEntry.Text = budget == null ? string.Empty : budget.Limit.ToString("0.##", CultureInfo.CurrentCulture);
        }

        private void RefreshBudgets()
        {
            _budgetStack.Children.Clear();
            var monthKey = BudgetEngine.ToMonthKey(_monthPicker.Date);
            var snapshots = _budgetEngine.BuildSnapshots(_data, monthKey);
            if (!snapshots.Any())
            {
                _budgetStack.Children.Add(Ui.Panel(Ui.Body("No budgets are saved for this month yet.")));
                return;
            }

            foreach (var snapshot in snapshots)
            {
                _budgetStack.Children.Add(Ui.Panel(new StackLayout
                {
                    Spacing = 6,
                    Children =
                    {
                        new Grid
                        {
                            ColumnDefinitions =
                            {
                                new ColumnDefinition { Width = GridLength.Star },
                                new ColumnDefinition { Width = GridLength.Auto }
                            },
                            Children =
                            {
                                { new Label { Text = snapshot.Category, FontAttributes = FontAttributes.Bold, TextColor = Ui.Text }, 0, 0 },
                                { new Label { Text = snapshot.StatusLabel, TextColor = Ui.BudgetColor(snapshot.Health), FontAttributes = FontAttributes.Bold }, 1, 0 }
                            }
                        },
                        new ProgressBar
                        {
                            Progress = (double)Math.Max(0m, Math.Min(1m, snapshot.PercentUsed)),
                            ProgressColor = Ui.BudgetColor(snapshot.Health)
                        },
                        Ui.Body(string.Format(
                            CultureInfo.CurrentCulture,
                            "Limit {0:C}; spent {1:C}; remaining {2:C}.",
                            snapshot.Limit,
                            snapshot.Spent,
                            snapshot.Remaining))
                    }
                }));
            }
        }
    }
}
