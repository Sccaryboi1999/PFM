# Contributing to Personal Finance Tracker

Thank you for helping improve Personal Finance Tracker. This document describes the expected workflow for code, documentation, and migration contributions.

## Before You Start

- Search existing issues before opening a new one.
- Use a feature request for proposed behavior and a bug report for reproducible defects.
- Keep financial examples fictional and free of personal or account information.
- For security concerns, follow [SECURITY.md](SECURITY.md) instead of filing a public issue.

## Development Setup

The current codebase uses Xamarin.Forms 5 and Xamarin.Android. Microsoft ended Xamarin support on May 1, 2024, so contributors need an existing Xamarin-capable Windows/Visual Studio toolchain or should work on a clearly scoped .NET MAUI migration branch.

1. Fork or clone the repository.
2. Create a branch from `main`.
3. Open `PersonalFinanceTracker.sln` in Visual Studio.
4. Restore NuGet packages.
5. Set `PersonalFinanceTracker.Android` as the startup project.
6. Run on an Android emulator or connected device.

## Design Expectations

- Preserve offline-first behavior unless a proposal explicitly introduces an optional service.
- Never log transaction contents, secure-storage payloads, credentials, or file contents.
- Keep finance calculations deterministic and independent from view rendering.
- Validate imported data before persistence and make partial-import behavior clear to the user.
- Treat user-selected files as untrusted input.
- Preserve duplicate protection when changing CSV normalization or transaction models.
- Prefer small, reviewable changes with a single clear purpose.

## Code Organization

- `Models/` contains persisted records and calculated data shapes.
- `Services/` contains storage, import, budgeting, alert, and analytics logic.
- `Views/` contains Xamarin.Forms pages and reusable UI components.
- Platform-specific Android configuration belongs in `PersonalFinanceTracker.Android/`.

Keep business rules out of UI event handlers when a reusable service is a better fit.

## Manual Validation

Automated tests are not yet included. At minimum, verify the following before opening a pull request:

- Clean install seeds demo data only once.
- Transactions persist across app restarts.
- Category and date filters return the expected rows.
- Budget state is under 85%, near budget from 85% through 100%, and over budget above 100%.
- The sample CSV imports and a repeated import is detected as duplicate data.
- Invalid CSV rows produce useful feedback without discarding valid rows.
- Reports refresh when transactions change.
- Secure-storage failure is handled without an unhandled exception.

For a migration change, also document the target .NET version, Android API levels, dependency replacements, and any behavior that intentionally differs from the Xamarin baseline.

## Pull Requests

Include:

- A short problem statement and solution summary.
- The screens, services, or models affected.
- Manual verification performed and device/emulator details.
- Screenshots for visible UI changes.
- Privacy, persistence, or migration implications.
- Follow-up work that is intentionally out of scope.

Do not include generated build output, IDE state, signing keys, keystores, local secrets, or real financial data.

