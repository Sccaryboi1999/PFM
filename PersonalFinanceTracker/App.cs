using System;
using PersonalFinanceTracker.Services;
using PersonalFinanceTracker.Views;
using Xamarin.Forms;

namespace PersonalFinanceTracker
{
    public class App : Application
    {
        public App()
        {
            MainPage = new MainTabbedPage();
        }

        protected override async void OnStart()
        {
            try
            {
                await new DemoDataSeeder(FinanceRepository.Instance).EnsureSeededAsync();
                MessagingCenter.Send(this, AppEvents.DataChanged);
            }
            catch (Exception ex)
            {
                await MainPage.DisplayAlert(
                    "Secure storage unavailable",
                    "The app could not open the device secure storage area. " + ex.Message,
                    "OK");
            }
        }
    }
}
