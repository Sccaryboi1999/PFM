# Security Policy

Personal Finance Tracker handles sensitive financial records locally. Security reports are appreciated and should avoid exposing user data, credentials, or exploit details in public issues.

## Supported Versions

| Version | Supported |
| --- | --- |
| `main` | Best-effort maintenance |
| Older commits or unofficial builds | No |

The app is a portfolio baseline built on Xamarin.Forms, which reached end of support on May 1, 2024. The repository should not be treated as a currently supported production finance platform until it is migrated to a supported .NET stack and receives a dedicated security review.

## Reporting a Vulnerability

Use GitHub's private vulnerability-reporting or security-advisory flow for this repository if it is available. If private reporting is unavailable, open a public issue containing only a high-level notice and request a private contact channel. Do not publish proof-of-concept code, transaction data, storage payloads, secrets, or device identifiers.

Please include:

- Affected commit or version.
- Device and Android version.
- Reproduction prerequisites and high-level steps.
- Expected and observed impact.
- Whether financial data, secure storage, file import, or device permissions are involved.
- A suggested mitigation, if known.

## Security Model

- Finance data is serialized to JSON and stored through `Xamarin.Essentials.SecureStorage`.
- The app has no account system, remote backend, telemetry, or internet permission.
- CSV imports are user-selected and parsed locally.
- `android:allowBackup` is disabled in the Android manifest.
- Imported transaction fingerprints use SHA-256 for duplicate detection, not for encryption or authentication.

## Known Security Limitations

- Xamarin.Forms and Xamarin.Android are no longer supported by Microsoft.
- The app has not received an independent security audit.
- The local JSON payload is an all-or-nothing store with no built-in backup or recovery.
- The app does not provide biometric locking, an application PIN, or explicit session timeout.
- Device compromise, unlocked-device access, rooted environments, malicious keyboards, screenshots, and operating-system vulnerabilities are outside the app's current defenses.
- CSV parsing is intentionally lightweight and should be treated as an untrusted-input boundary.

Do not commit signing material, keystores, exported financial records, or production secrets to this repository.

