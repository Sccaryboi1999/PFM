# Architecture

Personal Finance Tracker is a local-first Xamarin.Forms application with a shared .NET Standard library and an Android host project. It uses programmatic C# views, service classes for finance rules, and a single secure-storage document for persistence.

## Runtime Overview

```text
Android MainActivity
        |
        v
Xamarin.Forms App -> MainTabbedPage
        |                |
        |                +-> DashboardPage
        |                +-> TransactionsPage
        |                +-> BudgetsPage
        |                +-> ReportsPage
        |
        +-> DemoDataSeeder on first launch

Views -> Services -> Models -> FinanceRepository -> Xamarin.Essentials SecureStorage
```

The application has no remote API, identity provider, analytics service, or server-side component.

## Projects

### `PersonalFinanceTracker`

The shared .NET Standard 2.0 project contains all models, finance services, application startup, and Xamarin.Forms views.

### `PersonalFinanceTracker.Android`

The Android host initializes Xamarin.Essentials and Xamarin.Forms, applies platform resources, and loads the shared `App`. It targets Android API 33 and declares API 21 as the minimum supported level.

## Application Startup

1. Android starts `MainActivity`.
2. `MainActivity` initializes Xamarin.Essentials and Xamarin.Forms.
3. `App` creates `MainTabbedPage`.
4. `App.OnStart` asks `DemoDataSeeder` to populate the store if both transactions and budgets are empty.
5. A `MessagingCenter` event tells active pages to refresh after data changes.

Demo seeding is idempotent for a normal populated store. If both collections are cleared, the next launch will treat the store as empty and seed again.

## UI Composition

`MainTabbedPage` owns four navigation tabs:

- `DashboardPage` summarizes the selected month, renders budget health, displays alerts, and manages review reminders.
- `TransactionsPage` creates transactions, imports CSV files, and filters history.
- `BudgetsPage` creates or updates one category limit per month.
- `ReportsPage` supplies calculated data to lightweight interactive bar charts.

`Ui` centralizes colors, typography helpers, buttons, panels, money formatting, and budget-state colors. `InteractiveBarChart` renders normalized horizontal bars without an external chart package.

## Domain Model

`FinanceData` is the persistence root and contains:

- `Transactions`: income and expense records.
- `Budgets`: category limits keyed by `yyyy-MM` month values.
- `ReminderSettings`: cadence and last-review timestamps.
- `DemoSeededUtc`: the last demo initialization time.

Amounts are stored as positive decimals. `TransactionType` determines whether a record contributes to income or expenses. `SignedAmount` exposes a calculated positive or negative representation.

## Services

### `FinanceRepository`

Serializes `FinanceData` with Newtonsoft.Json and stores it under `personal-finance-tracker.secure-data.v1` using Xamarin.Essentials `SecureStorage`. Loads repair null collections to protect against incomplete older payloads.

### `BudgetEngine`

Aggregates monthly expense totals by category and compares them with saved limits:

- Under budget: less than 85% used.
- Near budget: 85% through 100% used.
- Over budget: more than 100% used.

### `FinanceAnalytics`

Builds monthly summaries and chart items for category spending, income versus expenses, and the six-month expense trend ending with the selected month.

### `AlertService`

Creates user-facing messages for near/over-budget categories and weekly or monthly review reminders. Alerts are displayed while the app is active; no background scheduler is used.

### `CsvImportService`

Normalizes common header variants, validates required values, infers income or expense type, and hashes normalized transaction fields to detect duplicates. Valid rows can be imported even when other rows contain errors.

## Data Flow

### Manual Transaction

```text
User input -> TransactionsPage validation -> TransactionRecord
           -> FinanceData.Transactions -> FinanceRepository.SaveAsync
           -> AppEvents.DataChanged -> page refresh
```

### CSV Import

```text
FilePicker -> local text read -> CsvImportService
           -> validation + normalization + duplicate check
           -> accepted TransactionRecords -> secure storage -> page refresh
```

### Budget Reporting

```text
FinanceData -> BudgetEngine / FinanceAnalytics
            -> snapshots and chart items -> Xamarin.Forms views
```

## Trust Boundaries

- CSV files are untrusted user input and must be size-limited or streamed in a future production version.
- Secure storage is delegated to the platform and depends on device integrity and lock configuration.
- UI strings derived from transaction descriptions are displayed locally and are not executed.
- SHA-256 fingerprints prevent routine duplicate imports but do not authenticate records.
- All data currently moves through one JSON document, so very large histories can increase memory use and write cost.

## Modernization Path

Microsoft ended Xamarin support on May 1, 2024. A production modernization should:

1. Create a supported .NET MAUI single-project app.
2. Port models and pure finance services first.
3. Replace Xamarin.Forms namespaces and platform initialization with MAUI equivalents.
4. Replace `MessagingCenter` with a supported messaging or state-management approach.
5. Validate secure-storage compatibility and define a versioned data-migration path.
6. Revisit Android file permissions and use current platform file-picker behavior.
7. Add unit tests before changing budget, CSV, and analytics behavior.
8. Add integration/UI tests for persistence, imports, navigation, and first-run seeding.

The existing service boundaries make the calculation and parsing code the safest place to begin that migration.

