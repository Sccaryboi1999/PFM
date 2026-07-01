using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public class DashboardPage : ContentPage
    {
        private readonly FinanceRepository _repository;
        private readonly BudgetEngine _budgetEngine;
        private readonly FinanceAnalytics _analytics;
        private readonly AlertService _alertService;

        private FinanceData _data;
        private readonly DatePicker _monthPicker;
        private readonly Label _incomeLabel;
        private readonly Label _expenseLabel;
        private readonly Label _netLabel;
        private readonly Label _overallBudgetLabel;
        private readonly StackLayout _budgetStack;
        private readonly StackLayout _alertStack;
        private readonly Picker _reminderPicker;
        private bool _isRefreshing;
        private string _lastAlertSignature;

        public DashboardPage()
        {
            Title = "Dashboard";
            BackgroundColor = Ui.Background;

            _repository = FinanceRepository.Instance;
            _budgetEngine = new BudgetEngine();
            _analytics = new FinanceAnalytics();
            _alertService = new AlertService();
            _data = new FinanceData();

            _monthPicker = new DatePicker { Date = DateTime.Today, Format = "MMMM yyyy" };
            _monthPicker.DateSelected += (sender, args) => Refresh();

            _incomeLabel = ValueLabel();
            _expenseLabel = ValueLabel();
            _netLabel = ValueLabel();
            _overallBudgetLabel = ValueLabel();

            _budgetStack = new StackLayout { Spacing = 8 };
            _alertStack = new StackLayout { Spacing = 8 };

            _reminderPicker = new Picker { Title = "Reminder cadence" };
            _reminderPicker.Items.Add("Off");
            _reminderPicker.Items.Add("Weekly");
            _reminderPicker.Items.Add("Monthly");
            _reminderPicker.SelectedIndexChanged += async (sender, args) => await SaveReminderChoiceAsync();

            var reviewedButton = Ui.SecondaryButton("Mark spending reviewed");
            reviewedButton.Clicked += async (sender, args) => await MarkReviewedAsync();

            var summaryGrid = new Grid
            {
                RowSpacing = 10,
                ColumnSpacing = 10,
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                RowDefinitions =
                {
                    new RowDefinition { Height = GridLength.Auto },
                    new RowDefinition { Height = GridLength.Auto }
                }
            };
            summaryGrid.Children.Add(SummaryTile("Income", _incomeLabel), 0, 0);
            summaryGrid.Children.Add(SummaryTile("Expenses", _expenseLabel), 1, 0);
            summaryGrid.Children.Add(SummaryTile("Net", _netLabel), 0, 1);
            summaryGrid.Children.Add(SummaryTile("Budget", _overallBudgetLabel), 1, 1);

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Padding = new Thickness(18, 18, 18, 28),
                    Spacing = 8,
                    Children =
                    {
                        Ui.Title("Personal Finance Tracker"),
                        _monthPicker,
                        summaryGrid,
                        Ui.SectionTitle("Alerts"),
                        _alertStack,
                        Ui.SectionTitle("Budget Status"),
                        _budgetStack,
                        Ui.SectionTitle("Review Reminder"),
                        Ui.Panel(new StackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                _reminderPicker,
                                reviewedButton
                            }
                        })
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
                Refresh();
                await ShowAlertsIfNeededAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Storage error", ex.Message, "OK");
            }
        }

        private void Refresh()
        {
            _isRefreshing = true;
            var monthKey = BudgetEngine.ToMonthKey(_monthPicker.Date);
            var summary = _analytics.SummarizeMonth(_data, monthKey);
            var snapshots = _budgetEngine.BuildSnapshots(_data, monthKey);

            _incomeLabel.Text = Ui.Money(summary.Income);
            _expenseLabel.Text = Ui.Money(summary.Expenses);
            _netLabel.Text = Ui.Money(summary.Net);
            _netLabel.TextColor = summary.Net >= 0m ? Ui.Green : Ui.Red;

            if (snapshots.Any())
            {
                var totalLimit = snapshots.Sum(s => s.Limit);
                var totalSpent = snapshots.Sum(s => s.Spent);
                _overallBudgetLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0:C} left", totalLimit - totalSpent);
                _overallBudgetLabel.TextColor = totalSpent > totalLimit ? Ui.Red : Ui.Green;
            }
            else
            {
                _overallBudgetLabel.Text = "No budgets";
                _overallBudgetLabel.TextColor = Ui.Muted;
            }

            _reminderPicker.SelectedIndex = (int)_data.ReminderSettings.Interval;
            RenderBudgetStatus(snapshots);
            RenderAlerts(snapshots);
            _isRefreshing = false;
        }

        private void RenderBudgetStatus(IReadOnlyList<BudgetSnapshot> snapshots)
        {
            _budgetStack.Children.Clear();
            if (!snapshots.Any())
            {
                _budgetStack.Children.Add(Ui.Panel(Ui.Body("No monthly budgets yet.")));
                return;
            }

            foreach (var snapshot in snapshots)
            {
                var progress = Math.Max(0m, Math.Min(1m, snapshot.PercentUsed));
                var progressBar = new ProgressBar
                {
                    Progress = (double)progress,
                    ProgressColor = Ui.BudgetColor(snapshot.Health)
                };

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
                        progressBar,
                        Ui.Body(string.Format(
                            CultureInfo.CurrentCulture,
                            "{0:C} spent of {1:C}. Remaining: {2:C}.",
                            snapshot.Spent,
                            snapshot.Limit,
                            snapshot.Remaining))
                    }
                }));
            }
        }

        private void RenderAlerts(IReadOnlyList<BudgetSnapshot> snapshots)
        {
            _alertStack.Children.Clear();
            var alerts = _alertService.BuildBudgetAlerts(snapshots);
            var reminder = _alertService.BuildReminderMessage(_data.ReminderSettings, DateTime.UtcNow);
            if (!string.IsNullOrWhiteSpace(reminder))
            {
                alerts.Add(reminder);
            }

            if (!alerts.Any())
            {
                _alertStack.Children.Add(Ui.Panel(Ui.Body("No active alerts. Budgets are currently in a healthy range.")));
                return;
            }

            foreach (var alert in alerts)
            {
                _alertStack.Children.Add(Ui.Panel(new Label
                {
                    Text = alert,
                    TextColor = Ui.Text,
                    FontAttributes = FontAttributes.Bold
                }));
            }
        }

        private async Task ShowAlertsIfNeededAsync()
        {
            var monthKey = BudgetEngine.ToMonthKey(_monthPicker.Date);
            var snapshots = _budgetEngine.BuildSnapshots(_data, monthKey);
            var alerts = _alertService.BuildBudgetAlerts(snapshots);
            var reminder = _alertService.BuildReminderMessage(_data.ReminderSettings, DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(reminder))
            {
                alerts.Add(reminder);
            }

            if (!alerts.Any())
            {
                return;
            }

            var signature = string.Join("|", alerts);
            if (signature == _lastAlertSignature)
            {
                return;
            }

            _lastAlertSignature = signature;
            if (!string.IsNullOrWhiteSpace(reminder))
            {
                _data.ReminderSettings.LastReminderShownUtc = DateTime.UtcNow;
                await _repository.SaveAsync(_data);
            }

            await DisplayAlert("Finance alerts", string.Join(Environment.NewLine, alerts.Take(3)), "OK");
        }

        private async Task SaveReminderChoiceAsync()
        {
            if (_isRefreshing || _reminderPicker.SelectedIndex < 0)
            {
                return;
            }

            _data.ReminderSettings.Interval = (ReminderInterval)_reminderPicker.SelectedIndex;
            await _repository.SaveAsync(_data);
            MessagingCenter.Send((App)Application.Current, AppEvents.DataChanged);
        }

        private async Task MarkReviewedAsync()
        {
            _data.ReminderSettings.LastReviewedUtc = DateTime.UtcNow;
            _data.ReminderSettings.LastReminderShownUtc = DateTime.UtcNow;
            await _repository.SaveAsync(_data);
            Refresh();
            await DisplayAlert("Review saved", "Your reminder clock has been reset.", "OK");
        }

        private static Frame SummaryTile(string label, Label valueLabel)
        {
            return Ui.Panel(new StackLayout
            {
                Spacing = 4,
                Children =
                {
                    new Label { Text = label, TextColor = Ui.Muted, FontSize = 12 },
                    valueLabel
                }
            });
        }

        private static Label ValueLabel()
        {
            return new Label
            {
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Ui.Text
            };
        }
    }
}
