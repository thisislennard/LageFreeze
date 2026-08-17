# Repository guidance

## Product

LageFreeze is a small, stable Windows desktop application for freezing the
visible content of one selected monitor. Read `docs/product-requirements.md`,
`docs/architecture.md`, and `docs/roadmap.md` before making architectural or
scope decisions.

## Priorities

In descending order: stability, maintainability, simple development, clean
Windows integration, and few dependencies. Prefer a simple and explicit
implementation over a clever abstraction. Do not turn this into a general
incident-management platform.

## Development rules

- Keep code, identifiers, and filenames in English; user-facing text is German.
- Target Windows 10/11 x64 and support per-monitor DPI awareness.
- Freeze only through a normal topmost borderless window. Never alter display
  drivers, display configuration, DWM, or registry display settings.
- The application must restore the live desktop by closing its overlay window.
- Treat negative monitor coordinates, mixed DPI, portrait displays, and monitor
  disconnects as normal operating conditions.
- Do not add cloud services, telemetry, analytics, tracking, or required network
  access.
- Add dependencies only when their value clearly exceeds their maintenance cost.
- Keep the MVP boundary in `docs/roadmap.md`; do not silently pull later-phase
  features into it.
- Record material architectural decisions in `docs/architecture.md`.
- Do not create a license until the repository owner has selected one.

## Verification

Build and run focused automated tests for pure logic. Hardware-dependent display,
DPI, fullscreen, refresh, and disconnect behavior also requires the manual test
matrix described in `docs/product-requirements.md`.
