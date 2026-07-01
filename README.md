# Personal Finance Tracker

Xamarin.Forms personal finance tracking app for Visual Studio. The app is offline-only and stores financial data on the device through `Xamarin.Essentials.SecureStorage`.

## Resume-Matching Features

- Budgeting tools with monthly category limits, remaining budget calculation, and under/near/over budget status.
- Categorized income and expense transactions with date, amount, category, description, and type.
- Interactive Xamarin.Forms graph views for spending by category, income vs. expenses, and spending trends.
- CSV import automation that detects common columns, validates rows, and skips duplicate imported transactions.
- Secure local data storage with no online account and no internet permission.
- Alert notifications for near-budget, over-budget, and weekly/monthly spending review reminders.

## Open in Visual Studio

1. Open `PersonalFinanceTracker.sln` in Visual Studio.
2. Restore NuGet packages when prompted.
3. Select `PersonalFinanceTracker.Android` as the startup project.
4. Run on an Android emulator or connected Android device.

The app seeds a small demo dataset on first launch so the dashboard, budget alerts, and reports are visible immediately.

## CSV Import Demo

Use `samples/sample-transactions.csv` from this folder with the Transactions tab's **Import CSV** button. Importing the same file twice should skip the duplicate rows.

Accepted CSV headers include:

- `date`
- `description`, `memo`, `merchant`, or `payee`
- `amount`, or debit/credit columns
- `category`
- `type`

Default categories are Food, Transportation, Housing, Entertainment, Bills, Savings, and Miscellaneous.
