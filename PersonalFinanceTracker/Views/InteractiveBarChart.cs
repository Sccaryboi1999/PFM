using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using PersonalFinanceTracker.Models;
using Xamarin.Forms;

namespace PersonalFinanceTracker.Views
{
    public class InteractiveBarChart : ContentView
    {
        private readonly StackLayout _stack;

        public event EventHandler<ChartItem> ItemTapped;
        public string EmptyText { get; set; }

        public InteractiveBarChart()
        {
            EmptyText = "No chart data yet.";
            _stack = new StackLayout { Spacing = 10 };
            Content = _stack;
        }

        public void SetItems(IEnumerable<ChartItem> items)
        {
            _stack.Children.Clear();
            var list = items == null ? new List<ChartItem>() : items.ToList();
            if (!list.Any())
            {
                _stack.Children.Add(Ui.Body(EmptyText));
                return;
            }

            var max = list.Max(item => item.Value);
            if (max <= 0m)
            {
                max = 1m;
            }

            foreach (var sourceItem in list)
            {
                var item = sourceItem;
                var percent = Math.Max(0.02, Math.Min(1d, (double)(item.Value / max)));
                var bar = new Grid
                {
                    HeightRequest = 22,
                    BackgroundColor = Color.FromHex("#E2E8F0"),
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(percent * 100, GridUnitType.Star) },
                        new ColumnDefinition { Width = new GridLength((1d - percent) * 100, GridUnitType.Star) }
                    }
                };
                bar.Children.Add(new BoxView { Color = item.Color }, 0, 0);

                var row = new Grid
                {
                    ColumnSpacing = 10,
                    ColumnDefinitions =
                    {
                        new ColumnDefinition { Width = new GridLength(112) },
                        new ColumnDefinition { Width = GridLength.Star },
                        new ColumnDefinition { Width = new GridLength(88) }
                    }
                };

                row.Children.Add(new Label
                {
                    Text = item.Label,
                    FontSize = 13,
                    TextColor = Ui.Text,
                    VerticalTextAlignment = TextAlignment.Center,
                    LineBreakMode = LineBreakMode.TailTruncation
                }, 0, 0);
                row.Children.Add(bar, 1, 0);
                row.Children.Add(new Label
                {
                    Text = string.Format(CultureInfo.CurrentCulture, "{0:C0}", item.Value),
                    FontSize = 13,
                    TextColor = Ui.Text,
                    HorizontalTextAlignment = TextAlignment.End,
                    VerticalTextAlignment = TextAlignment.Center
                }, 2, 0);

                row.GestureRecognizers.Add(new TapGestureRecognizer
                {
                    Command = new Command(() => ItemTapped?.Invoke(this, item))
                });

                _stack.Children.Add(row);
            }
        }
    }
}
