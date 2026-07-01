using System;
using System.Globalization;
using System.Threading.Tasks;
using PersonalFinanceTracker.Models;
using PersonalFinanceTracker.Services;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public class ReportsPage : ContentPage
    {
        private readonly FinanceRepository _repository;
        private readonly FinanceAnalytics _analytics;
        private FinanceData _data;

        private readonly DatePicker _monthPicker;
        private readonly InteractiveBarChart _categoryChart;
        private readonly InteractiveBarChart _incomeExpenseChart;
        private readonly InteractiveBarChart _trendChart;
        private readonly Label _chartDetailLabel;

        public ReportsPage()
        {
            Title = "Reports";
            BackgroundColor = Ui.Background;
            _repository = FinanceRepository.Instance;
            _analytics = new FinanceAnalytics();
            _data = new FinanceData();

            _monthPicker = new DatePicker { Date = DateTime.Today, Format = "MMMM yyyy" };
            _monthPicker.DateSelected += (sender, args) => RefreshReports();

            _categoryChart = new InteractiveBarChart { EmptyText = "No expense categories to chart for this month." };
            _incomeExpenseChart = new InteractiveBarChart { EmptyText = "No income or expenses to compare for this month." };
            _trendChart = new InteractiveBarChart { EmptyText = "No spending trend data yet." };
            _chartDetailLabel = Ui.Body("No chart point selected.");

            _categoryChart.ItemTapped += OnChartItemTapped;
            _incomeExpenseChart.ItemTapped += OnChartItemTapped;
            _trendChart.ItemTapped += OnChartItemTapped;

            Content = new ScrollView
            {
                Content = new StackLayout
                {
                    Padding = new Thickness(18, 18, 18, 28),
                    Spacing = 8,
                    Children =
                    {
                        Ui.Title("Visual Reports"),
                        _monthPicker,
                        Ui.Panel(_chartDetailLabel),
                        Ui.SectionTitle("Spending by Category"),
                        Ui.Panel(_categoryChart),
                        Ui.SectionTitle("Monthly Income vs. Expenses"),
                        Ui.Panel(_incomeExpenseChart),
                        Ui.SectionTitle("Total Spending Trend"),
                        Ui.Panel(_trendChart)
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
                RefreshReports();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Storage error", ex.Message, "OK");
            }
        }

        private void RefreshReports()
        {
            var monthKey = BudgetEngine.ToMonthKey(_monthPicker.Date);
            _categoryChart.SetItems(_analytics.SpendingByCategory(_data, monthKey));
            _incomeExpenseChart.SetItems(_analytics.IncomeVsExpenses(_data, monthKey));
            _trendChart.SetItems(_analytics.SpendingTrend(_data, _monthPicker.Date));
            _chartDetailLabel.Text = string.Format(CultureInfo.CurrentCulture, "{0} report", _monthPicker.Date.ToString("MMMM yyyy", CultureInfo.CurrentCulture));
        }

        private void OnChartItemTapped(object sender, ChartItem item)
        {
            _chartDetailLabel.Text = item.Detail;
        }
    }
}
