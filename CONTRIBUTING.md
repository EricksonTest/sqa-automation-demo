# Contributing

This is a compact demonstration framework, so each change should make its test
intent clearer, its feedback faster, or its failure evidence more useful.

## Working agreement

1. Create a small branch from `main` and describe the risk or behaviour under test.
2. Keep assertions in tests; keep HTTP and browser mechanics in clients/page objects.
3. Prefer API coverage to UI coverage unless the risk exists specifically in the UI.
4. Use builders with valid defaults and vary only data relevant to the scenario.
5. Do not use fixed sleeps. Use explicit conditions with bounded timeouts.
6. Tag tests with their layer (`Api`, `Ui`, or `E2E`) and purpose (`Smoke` or
   `Regression`).
7. Include useful failure context without logging credentials or personal data.
8. Run `dotnet build --configuration Release` and `./scripts/run-smoke.sh` before a PR.

## Review checklist

- Is the scenario traceable to a risk or acceptance criterion?
- Can a failure be diagnosed from the test output and attached artifacts?
- Is test data isolated and cleaned up where the public system permits it?
- Could the same confidence be gained at a faster/lower layer?
- Does the PR avoid private employer code, URLs, data, and terminology?

The default target is a shared public training service. Keep execution modest; do
not add performance or security scans against it.
