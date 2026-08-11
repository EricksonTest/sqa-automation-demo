# C# Test Style Guide and Reusable Patterns

This guide codifies the conventions used in this repository. It is the practical companion to the [risk-based test strategy](test-strategy.md) and the [contributing working agreement](../CONTRIBUTING.md).

The style is easy to learn because it is **consistent, thin at the test layer, and predictable** — every API test looks like every other API test; every UI test follows the same base-class lifecycle.

---

## Five principles

1. **Tests are specifications** — thin, named, readable without framework knowledge.
2. **Mechanics live below** — clients, page objects, builders, waits, config.
3. **Data is built, not pasted** — valid defaults, unique identifiers, override only what matters.
4. **Layers match risk** — API for business rules; UI for presentation; E2E for cross-layer proof.
5. **Failures must explain themselves** — grouped assertions, behaviour messages, artifacts on failure, safe cleanup logging.

---

## Naming conventions

| Element | Pattern | Example |
|---|---|---|
| Test method | `Action_Condition_ExpectedOutcome` | `CreateBooking_WithValidData_ReturnsCreatedBooking` |
| Test class | `{Feature}{Layer}Tests` | `BookingApiTests`, `AdminLoginTests` |
| Categories | Layer + purpose | `[Category("Api")]`, `[Category("Smoke")]` |
| Page objects | `{Screen}Page` | `AdminLoginPage`, `HomePage` |
| Builders | `{Entity}Builder` | `BookingBuilder.CreateValid()` |
| API clients | `{Resource}ApiClient` | `BookingApiClient` |

Name tests so they could be pasted into a test plan. The name should answer: *what action, under what condition, with what expected result*.

### Category model

Categories are a **risk and execution filter**, not decoration:

| Category | Meaning | When to use |
|---|---|---|
| `Api` / `Ui` / `E2E` | Layer | Every test gets exactly one layer tag |
| `Smoke` | High-impact, fast gate | PR / pre-push |
| `Regression` | Broader functional coverage | Scheduled / manual |
| `Contract` | Schema/shape validation | API response structure checks |
| `Evidence` | Passing negative UI state with an attached screenshot | Manual interview evidence run |

A test can carry both a layer tag and a purpose tag (for example `[Category("Api")]` and `[Category("Smoke")]`).

---

## Layer separation

```text
Tests (intent)          → assert behaviour, status codes, persisted state, visible UI text
API clients (mechanics)   → HTTP, auth tokens, serialization, error normalisation
Page objects (mechanics)  → locators, clicks, navigation, explicit waits
Builders (data)           → valid defaults; tests override only scenario-relevant fields
Configuration             → environment-backed settings via TestSettings.FromEnvironment()
```

**Rule:** Never put `By.CssSelector(...)` or `HttpClient` calls directly in a test. If you need a new locator or endpoint, add it to the framework first.

Before writing a test, ask *"Can API give equivalent evidence?"* If yes, write an API test. Reserve UI for rendering, routing, and interaction risks. Reserve E2E for state that must be proven across layers.

---

## Test structure

Each test follows arrange → act → assert without explicit section comments:

```csharp
var requested = BookingBuilder.CreateValid().WithDeposit(false).Build();   // arrange
var created = await CreateAndTrackAsync(requested);                        // act
var response = await _client.GetBookingAsync(created.BookingId);           // act
var retrieved = response.EnsureSuccess();                                  // act

Assert.Multiple(() => { ... });                                            // assert
```

Keep tests at the "what and why" level. If a test needs more than ~15 lines, extract a private helper (like `CreateAndTrackAsync`) or move mechanics into the framework.

---

## Patterns by layer

### API tests

Reference: [`tests/SeniorQaAutomation.Tests/Api/BookingApiTests.cs`](../tests/SeniorQaAutomation.Tests/Api/BookingApiTests.cs)

| Pattern | How it appears | Why |
|---|---|---|
| Fixture lifecycle | `[SetUp]` creates client; `[TearDown]` disposes and cleans up | One client per test; no leaked data |
| Track-and-clean | `_bookingsToDelete` + `TrackForCleanup()` + `CreateAndTrackAsync()` | Safe against shared public environment |
| NonParallelizable | On the fixture class when shared SUT state conflicts | Prevents date/conflict collisions |
| Assert.Multiple | Groups related assertions | One failure shows all differences |
| EnsureSuccess vs raw status | Happy path uses `.EnsureSuccess()`; negative tests assert `StatusCode` | Clear success vs failure intent |
| Record `with` for variations | `requested with { FirstName = string.Empty }` | Minimal duplication for small data changes |
| Contract tests | Separate category; inspect `JsonDocument` / `JsonValueKind` | Catches schema drift |
| Resilient cleanup | `catch (ApiClientException)` in TearDown, log via `TestContext.Progress` | Cleanup failure must not mask the real result |

### UI tests

Reference: [`tests/SeniorQaAutomation.Tests/Ui/UiTestBase.cs`](../tests/SeniorQaAutomation.Tests/Ui/UiTestBase.cs)

| Pattern | How it appears | Why |
|---|---|---|
| Abstract base class | `UiTestBase` owns driver start/stop | Zero browser boilerplate in each test |
| Failure-only diagnostics | Screenshot + browser log capture when `TestStatus != Passed` | Fast passes; rich evidence on failure |
| Fluent page objects | `.Open().LoginSuccessfully(...).OpenRoom(...)` | Reads as a user journey |
| Return-type navigation | Success returns next page type; failure returns current page | Compiler guides correct next step |
| Private static locators | `private static readonly By Username = By.Id("username")` | Locators in one place |
| Lazy assertions | `Assert.That(() => homePage.BrandName, Is.Not.Empty)` | Re-evaluates after waits settle |
| Explicit waits only | `Wait.UntilVisible`, `Wait.UntilUrlContains` — no `Thread.Sleep` | Stable without arbitrary delays |

### E2E tests

Reference: [`tests/SeniorQaAutomation.Tests/E2E/ApiToUiBookingTests.cs`](../tests/SeniorQaAutomation.Tests/E2E/ApiToUiBookingTests.cs)

| Pattern | How it appears | Why |
|---|---|---|
| API for setup, UI for proof | Create via API; verify in browser | Faster, more reliable setup |
| Unique correlation data | `Guid.NewGuid()` suffix on guest names | Traceable in shared environment |
| try/catch/finally lifecycle | Capture artifacts on failure; delete in `finally` | E2E owns its own setup/teardown |
| Sparse count | Few E2E tests in the suite | E2E is expensive; use only for cross-layer risk |

---

## Test data (builder pattern)

Reference: [`src/SeniorQaAutomation.Framework/TestData/BookingBuilder.cs`](../src/SeniorQaAutomation.Framework/TestData/BookingBuilder.cs)

```csharp
// Default valid entity — zero config needed
BookingBuilder.CreateValid().Build()

// Override only what the scenario cares about
BookingBuilder.CreateValid().WithDeposit(false).Build()

// Fluent chain for E2E correlation
BookingBuilder.CreateValid()
    .WithGuest($"E2E{suffix}", $"Guest{suffix}")
    .WithRoom(1)
    .Build()
```

For every new entity, create a `{Entity}Builder` with `CreateValid()` and fluent `With*` methods. Never hard-code dates, IDs, or names in test bodies.

Builder properties:

- **Valid by default** — `CreateValid()` always produces a usable entity.
- **Unique dates/names** — avoid collisions in shared environments.
- **Override only relevant fields** — tests stay focused.

Copy [`docs/templates/EntityBuilderTemplate.cs.template`](templates/EntityBuilderTemplate.cs.template) when adding a new builder.

---

## Assertions and diagnostics

1. **Group related checks** with `Assert.Multiple` — especially when comparing request vs response fields.
2. **Write assertion messages as behaviour statements** — e.g. `"Expected server-side validation feedback for an empty contact message."` not `"errors not empty"`.
3. **Separate success unwrapping from assertion** — call `.EnsureSuccess()` before asserting field values; use raw `StatusCode` for negative paths.
4. **Attach artifacts on failure** — screenshots and browser logs via `TestContext.AddTestAttachment`.
5. **Never log secrets** — credentials come from `TestSettings.FromEnvironment()`, never echoed in output.

---

## Workflow: adding a new test

Follow this order — **framework first, test second**:

1. **Identify the risk** — what regression would this catch? (See risk table in [test-strategy.md](test-strategy.md).)
2. **Pick the lowest reliable layer** — API first, UI if browser-specific, E2E only if cross-layer.
3. **Extend the framework** — add client method, page object method, or builder `With*` as needed.
4. **Name the test** — `Action_Condition_ExpectedOutcome`.
5. **Tag it** — layer category + Smoke/Regression/Contract as appropriate.
6. **Use a builder** — `CreateValid()`, override only scenario-relevant fields.
7. **Keep the test body thin** — arrange, act, assert.
8. **Own your data** — track IDs for cleanup; use unique identifiers.
9. **Handle cleanup safely** — delete in TearDown/finally; swallow cleanup errors with logging.
10. **Run the right filter** — `dotnet test --filter 'TestCategory=Smoke'` before PR.

Starter templates live in [`docs/templates/`](templates/).

---

## What not to copy blindly

These are deliberate trade-offs for a **public demo environment**, not universal rules:

- **`[NonParallelizable]` on the whole API fixture** — needed here because of shared booking dates; with a disposable local SUT, parallelise instead.
- **Minimal E2E count** — correct for this repo; products with complex cross-service flows may need more.
- **Contract tests via manual `JsonDocument` inspection** — consider JSON Schema or OpenAPI validation at scale.
- **Custom assertion extensions** — avoid proliferating one-off helpers; prefer NUnit constraints and behaviour messages.

---

## Related documentation

- [Test strategy](test-strategy.md) — risk model, scope, and execution tiers
- [Contributing](../CONTRIBUTING.md) — working agreement and review checklist
- [Templates](templates/) — copy-paste starters for API, UI, E2E, page objects, and builders
