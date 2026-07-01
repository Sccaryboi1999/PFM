using System.Globalization;
using PersonalFinanceTracker.Models;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public static class Ui
    {
        public static readonly Color Background = Color.FromHex("#F7FAFC");
        public static readonly Color Surface = Color.White;
        public static readonly Color Border = Color.FromHex("#CBD5E0");
        public static readonly Color Text = Color.FromHex("#1A202C");
        public static readonly Color Muted = Color.FromHex("#718096");
        public static readonly Color Green = Color.FromHex("#2F855A");
        public static readonly Color Amber = Color.FromHex("#B7791F");
        public static readonly Color Red = Color.FromHex("#C53030");
        public static readonly Color Blue = Color.FromHex("#2B6CB0");

        public static Label Title(string text)
        {
            return new Label
            {
                Text = text,
                FontSize = 24,
                FontAttributes = FontAttributes.Bold,
                TextColor = Text,
                Margin = new Thickness(0, 0, 0, 4)
            };
        }

        public static Label SectionTitle(string text)
        {
            return new Label
            {
                Text = text,
                FontSize = 18,
                FontAttributes = FontAttributes.Bold,
                TextColor = Text,
                Margin = new Thickness(0, 16, 0, 4)
            };
        }

        public static Label Body(string text)
        {
            return new Label
            {
                Text = text,
                FontSize = 14,
                TextColor = Muted
            };
        }

        public static Button PrimaryButton(string text)
        {
            return new Button
            {
                Text = text,
                BackgroundColor = Green,
                TextColor = Color.White,
                CornerRadius = 6,
                FontAttributes = FontAttributes.Bold
            };
        }

        public static Button SecondaryButton(string text)
        {
            return new Button
            {
                Text = text,
                BackgroundColor = Color.FromHex("#EDF2F7"),
                TextColor = Text,
                CornerRadius = 6
            };
        }

        public static Frame Panel(View content)
        {
            return new Frame
            {
                Content = content,
                Padding = 14,
                Margin = new Thickness(0, 6),
                BackgroundColor = Surface,
                BorderColor = Border,
                CornerRadius = 8,
                HasShadow = false
            };
        }

        public static string Money(decimal amount)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0:C}", amount);
        }

        public static Color BudgetColor(BudgetHealth health)
        {
            if (health == BudgetHealth.OverBudget)
            {
                return Red;
            }

            if (health == BudgetHealth.NearBudget)
            {
                return Amber;
            }

            return Green;
        }
    }
}
