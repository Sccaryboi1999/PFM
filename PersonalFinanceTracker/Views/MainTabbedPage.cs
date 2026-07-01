using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public class MainTabbedPage : TabbedPage
    {
        public MainTabbedPage()
        {
            Title = "Finance Tracker";
            BarBackgroundColor = Color.White;
            BarTextColor = Ui.Green;
            BackgroundColor = Ui.Background;

            Children.Add(new NavigationPage(new DashboardPage())
            {
                Title = "Dashboard",
                BarBackgroundColor = Color.White,
                BarTextColor = Ui.Text
            });
            Children.Add(new NavigationPage(new TransactionsPage())
            {
                Title = "Transactions",
                BarBackgroundColor = Color.White,
                BarTextColor = Ui.Text
            });
            Children.Add(new NavigationPage(new BudgetsPage())
            {
                Title = "Budgets",
                BarBackgroundColor = Color.White,
                BarTextColor = Ui.Text
            });
            Children.Add(new NavigationPage(new ReportsPage())
            {
                Title = "Reports",
                BarBackgroundColor = Color.White,
                BarTextColor = Ui.Text
            });
        }
    }
}
