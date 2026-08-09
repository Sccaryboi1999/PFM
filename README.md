# Personal Finance Tracker

Personal Finance Tracker is a local-first Android budgeting app built with Xamarin.Forms and C#. It brings transactions, monthly category budgets, spending alerts, CSV imports, and visual reports into one offline mobile experience.

The app does not require an account or backend. Financial records are serialized as JSON and stored on the device through `Xamarin.Essentials.SecureStorage`.

## Problem It Solves

Personal finance data is often split between bank exports, notes, and spreadsheets. Personal Finance Tracker provides a focused mobile workflow for recording activity, monitoring category limits, and reviewing trends without sending financial data to a remote service.

## Features

- Monthly dashboard with income, expenses, net cash flow, and remaining budget totals.
- Manual income and expense entry with date, amount, description, type, and category.
- Transaction filtering by category and date range.
- Monthly category budgets with under-budget, near-budget, and over-budget states.
- Near-budget warning at 85% usage and over-budget alerts above 100%.
- Weekly, monthly, or disabled spending-review reminders.
- CSV import with flexible bank-style column matching and row-level validation.
- Duplicate import protection using a normalized SHA-256 transaction fingerprint.
- Interactive reports for spending by category, income versus expenses, and six-month spending trends.
- Seeded demo data on first launch so every primary screen can be evaluated immediately.
- Offline storage with no account, analytics SDK, advertising SDK, or internet permission.

## App Sections

| Section | Purpose |
| --- | --- |
| Dashboard | Review monthly totals, budget health, alerts, and reminder settings. |
| Transactions | Add transactions, import CSV data, and filter transaction history. |
| Budgets | Set category limits and track current-month progress. |
| Reports | Explore category spending, income versus expenses, and a six-month trend. |

## Tech Stack

- C# and .NET Standard 2.0 shared application code
- Xamarin.Forms 5.0.0.2662 user interface
- Xamarin.Essentials 1.8.1 for secure storage and file picking
- Newtonsoft.Json 13.0.3 for local persistence
- Xamarin.Android host targeting Android API 33 with a minimum API level of 21
- Programmatic Xamarin.Forms UI; no XAML layer or remote backend

## Requirements

This repository is a legacy Xamarin.Forms project and requires a Windows development environment with a Xamarin-capable Visual Studio installation, the Android SDK, and either an Android emulator or physical Android device.

Microsoft ended support for Xamarin and Xamarin.Forms on May 1, 2024. The existing project remains useful as a portfolio and learning app, but new production development should migrate it to .NET MAUI. See [Project Status](#project-status) and [Architecture](docs/ARCHITECTURE.md) for the modernization path.

## Run Locally

1. Clone the repository.

   ```powershell
   git clone https://github.com/Sccaryboi1999/PFM.git
   cd PFM
   ```

2. Open `PersonalFinanceTracker.sln` in a Xamarin-capable Visual Studio installation.
3. Restore NuGet packages.
4. Confirm that the required Android SDK platform is installed.
5. Set `PersonalFinanceTracker.Android` as the startup project.
6. Select an Android emulator or connected device and run the app.

On first launch, the app creates sample budgets and transactions for the current month. This makes the dashboard, alerts, transaction list, and reports immediately visible.

## CSV Import

Use `samples/sample-transactions.csv` with the **Import CSV** button on the Transactions tab. Re-importing the same rows skips duplicates.

Recognized headers include:

| Field | Accepted headers |
| --- | --- |
| Date | `date`, `transaction date`, `posted date`, `posting date` |
| Description | `description`, `memo`, `name`, `merchant`, `payee` |
| Amount | `amount`, `transaction amount`, `value` |
| Debit | `debit`, `withdrawal`, `charge` |
| Credit | `credit`, `deposit`, `payment` |
| Category | `category`, `spending category` |
| Type | `type`, `transaction type` |

The date, description, and either amount or debit/credit data are required. Unknown or missing categories fall back to `Miscellaneous`. Quoted values, escaped quotes, currency symbols, commas, parentheses, negative values, and separate debit/credit columns are supported.

Default categories are Food, Transportation, Housing, Entertainment, Bills, Savings, and Miscellaneous.

## Budget and Alert Rules

- A category is **Under budget** below 85% of its monthly limit.
- A category is **Near budget** from 85% through 100% of its monthly limit.
- A category is **Over budget** above 100% of its monthly limit.
- Review reminders can be off, weekly, or monthly.
- Budget and reminder alerts are shown inside the app; the project does not schedule Android background notifications.

## Data Storage and Privacy

- Transactions, budgets, and reminder settings are stored as one JSON document under the secure-storage key `personal-finance-tracker.secure-data.v1`.
- The application has no server, account system, telemetry, or internet permission.
- CSV files are read only after the user selects one through the platform file picker.
- Clearing app data, uninstalling the app, or resetting secure storage can permanently remove local records.
- The current version does not provide backup, sync, encryption export, or data recovery.

Do not use seeded demo values as real financial advice or records. Review [SECURITY.md](SECURITY.md) before reporting a vulnerability.

## Project Structure

```text
PersonalFinanceTracker/
  Models/                 Finance records and calculated view models
  Services/               Storage, CSV import, budgets, alerts, and analytics
  Views/                  Dashboard, transactions, budgets, reports, and UI helpers
  App.cs                  App startup and demo-data initialization
PersonalFinanceTracker.Android/
  MainActivity.cs         Android application host
  Properties/             Android manifest
  Resources/              Android theme, toolbar, and tab resources
samples/
  sample-transactions.csv Example import file
docs/
  ARCHITECTURE.md         Data flow, boundaries, and modernization notes
```

See [Architecture](docs/ARCHITECTURE.md) for the component responsibilities and data flow.

## Validation

The repository does not currently include an automated test project. Before accepting a change, manually verify:

1. A clean first launch seeds demo data once.
2. A manual income and expense appear in the selected date range.
3. Budget health changes at the 85% and 100% thresholds.
4. The sample CSV imports successfully and a second import skips duplicates.
5. Category, income/expense, and trend charts refresh after data changes.
6. Reminder settings and finance data persist after an app restart.
7. The app handles unavailable secure storage and invalid CSV rows without crashing.

## Known Limitations

- Xamarin.Forms and Xamarin.Android are out of support.
- Only an Android host project is included.
- Data is stored as a single secure JSON payload rather than a queryable database.
- Transactions and budgets can be added or updated, but the current UI does not expose every possible edit, delete, backup, or restore workflow.
- Alerts are in-app prompts rather than scheduled operating-system notifications.
- The custom charts are lightweight bar views rather than a full charting library.
- The CSV parser handles common single-line records but not embedded newlines inside quoted fields.
- No automated unit, integration, or UI tests are included yet.

## Future Improvements

- Migrate from Xamarin.Forms to a supported .NET MAUI single-project application.
- Add automated tests for budgeting, CSV parsing, duplicate detection, analytics, and persistence.
- Add transaction editing and deletion with confirmation and undo.
- Add encrypted backup and restore with explicit user control.
- Add optional biometric access and secure app locking.
- Add accessibility audits, localization, and richer chart interactions.
- Add release automation and signed Android build documentation.

## Contributing

Read [CONTRIBUTING.md](CONTRIBUTING.md) for development expectations, validation steps, and the pull-request checklist.

## Project Status

Personal Finance Tracker is a functional Android portfolio baseline. Its core finance workflow is implemented, but the Xamarin runtime is no longer supported. The recommended next engineering milestone is a .NET MAUI migration followed by automated test coverage and an explicit backup strategy.

