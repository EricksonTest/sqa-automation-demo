# Risk-Based Test Strategy

## 1. Purpose

This strategy describes how the **Senior QA Automation Demo** provides proportionate evidence for a public Restful Booker Platform using C#/.NET, NUnit, `HttpClient`, Selenium and GitHub Actions. It is a portfolio strategy, not a certification of the public service or a claim about any former employer’s system.

The strategy is designed to demonstrate the capabilities emphasised in the supplied Jacobs Senior QA Engineer description: maintainable automated/manual test thinking, acceptance criteria, functional/regression/integration/non-functional awareness, CI/CD, clean code, problem solving, collaboration and mentoring.

## 2. Quality objectives

1. Detect important regressions in booking and administration behaviour at the lowest reliable layer.
2. Prove selected API, persistence and browser interactions work together.
3. Give developers fast, repeatable pull-request feedback.
4. Make failures diagnosable from test output and retained artifacts.
5. Keep tests independent, readable and safe to run against a shared public environment.
6. Make scope, assumptions and residual risk explicit.

## 3. System context and assumptions

- The system under test is the public Restful Booker Platform at the configured `TEST_BASE_URL`; the repository default is `https://automationintesting.online`.
- The project does not own the application, hosting, data reset, network path or release schedule.
- Tests use public demo behaviour and non-sensitive generated data only.
- Environment configuration is supplied through variables such as `TEST_BASE_URL`, `BROWSER`, `HEADLESS` and timeout/authentication settings.
- The automated test repository can prove observed behaviour at execution time; it cannot prove all internal service paths or production readiness.

## 4. Risk model

Priority combines **impact**, **likelihood**, **detectability** and **feedback cost**. “High” means the demo should provide an automated gate or explicit evidence. “Medium” means targeted automated or exploratory coverage is proportionate. “Low/Deferred” means the risk is acknowledged but requires a different scope or environment.

| ID | Risk | Priority | Primary evidence | Residual risk |
|---|---|---:|---|---|
| R1 | A valid resource cannot be created or its returned data is wrong | High | API functional test: status, typed payload, generated identifier | Public service/data availability |
| R2 | Created/updated state cannot be retrieved consistently | High | API integration/read-after-write test with bounded assertion | Unknown internal persistence topology |
| R3 | Critical browser booking/admin behaviour is broken | High | Focused Selenium smoke tests | Browsers/devices outside configured matrix |
| R4 | API state and UI-visible state disagree | High | Small cross-layer E2E scenario using the same unique data | Only the selected journey/fields are proven |
| R5 | Authentication accepts invalid access or blocks valid demo access | High | Positive/negative API or UI checks where implemented | Not a security assessment |
| R6 | Validation fails at required-field/date/identifier boundaries | Medium | API boundary and negative cases; UI only where presentation matters | Unmodelled combinations |
| R7 | Tests collide through shared data or depend on order | High | Unique builders, independent fixtures and safe ownership/cleanup | Shared public users can still interfere |
| R8 | Timing/DOM/network noise causes false failures | High | Explicit waits, bounded timeouts, diagnostics and reliability review | External network/platform variability remains |
| R9 | A CI failure lacks enough evidence to act | High | NUnit/TRX output, stack trace, safe request context, screenshot/log artifacts | Artifact retention/configuration may vary |
| R10 | Slow regression delays delivery feedback | Medium | Category-based PR/scheduled execution and duration monitoring | No service-level execution SLO yet |
| R11 | Accessibility, performance, resilience or security is inadequate | Deferred for automation demo; potentially High in product | Documented plan and specialist follow-up | Not tested by this repository |

## 5. Scope

### Automated in scope

- API functional behaviour using typed C# models and `HttpClient`-based clients.
- Representative positive, negative and boundary cases implemented in the test suite.
- Read-after-write or equivalent integration checks where the public API supports them.
- Focused Selenium checks for critical user/admin behaviour implemented by the project.
- A small cross-layer scenario joining efficient API setup/verification to browser-visible evidence where supported.
- Configuration, browser lifecycle, explicit waits and failure diagnostics.
- NUnit categorisation: `Api`, `Ui`, `Smoke`, `Regression` and `E2E` as applicable to each test.
- GitHub Actions restore, build, selected test execution and results/artifact publication.

### Manual/exploratory in scope

- Short exploratory review of usability and unexpected behaviour around the automated happy path.
- Visual review of browser behaviour that is expensive or brittle to assert in this portfolio.
- Review of acceptance examples, automation suitability and residual risk before adding a test.
- Investigation and classification of any intermittent result; a rerun alone is not analysis.

### Explicitly out of scope

- Load, stress, soak, scalability or capacity certification.
- Penetration testing, vulnerability assessment or security accreditation.
- Formal accessibility conformance audit.
- Disaster recovery, failover or operational resilience testing.
- Native mobile applications and an exhaustive browser/device matrix.
- Exhaustive field combinations, localisation, payments or third-party integrations not exposed by the demo.
- Validation of internal microservice implementation or database contents not observable through supported interfaces.
- Production monitoring, SLO compliance or release approval for the public platform.

Out-of-scope does not mean unimportant. For a critical system, these areas require explicit requirements, representative environments/data, specialised tooling and named ownership.

## 6. Risk-based test pyramid

```text
                    Few E2E
        API-created state → browser evidence
                Focused Selenium UI
        Critical interaction and presentation risk
          Broader API functional/integration
    Validation, boundaries, auth and persisted state
       Build/static and focused component checks
```

The pyramid is a decision model rather than a fixed ratio:

- Prefer API checks when they provide equivalent behavioural evidence faster and with clearer isolation.
- Use Selenium for risks that genuinely require a browser: routing, rendering, user interaction and UI integration.
- Use E2E sparingly because each extra boundary increases runtime and ambiguous failure modes.
- Add tests in response to risk, change history and escaped defects—not to meet a test-count target.

## 7. Test design principles

### Independence and data

- Every test can run alone and must not depend on ordering.
- Builders provide valid defaults; tests change only the fields relevant to the behaviour.
- Generated data is unique enough for the shared public environment and carries no personal/confidential information.
- Server-generated IDs are captured in safe output for traceability.
- Cleanup deletes only data created and positively identified by the current test, where supported. Otherwise, use fresh unique data.

### API

- Assert the expected status and meaningful response state; do not treat any 2xx as sufficient.
- Keep transport/serialization mechanics in clients and behavioural intent in tests.
- Use negative and boundary partitions deliberately rather than duplicating every permutation.
- Retry only a documented eventually consistent condition, with a bound and a useful terminal failure.

### UI

- Page/component objects expose behaviour and centralise locators.
- Prefer stable identifiers or accessibility-aligned selectors supplied by the application.
- Synchronise with observable conditions through explicit waits; avoid fixed sleeps.
- Use headed execution for local demonstration and headless execution in CI.
- Capture screenshot and relevant browser context on failure; never capture secrets.

### Assertions and diagnostics

- Failure messages identify expected behaviour, actual observation and safe test-data correlation.
- Related assertions can be grouped when seeing all differences improves diagnosis.
- Logs exclude passwords, tokens, cookies, authorization headers and unnecessary payload data.

## 8. Execution model

The implemented entry points are:

```bash
./scripts/run-smoke.sh
./scripts/run-demo.sh
```

For targeted investigation:

```bash
dotnet test tests/SeniorQaAutomation.Tests/SeniorQaAutomation.Tests.csproj \
  --filter 'TestCategory=Api'

dotnet test tests/SeniorQaAutomation.Tests/SeniorQaAutomation.Tests.csproj \
  --filter 'TestCategory=Ui'

dotnet test tests/SeniorQaAutomation.Tests/SeniorQaAutomation.Tests.csproj \
  --filter 'TestCategory=E2E'
```

Filters must be confirmed with `dotnet test --list-tests` whenever categories or adapters change.

| Trigger | Intended selection | Objective | Gate expectation |
|---|---|---|---|
| Local development | Relevant test or category | Fast implementation feedback | Developer resolves failures before push |
| Pull request/push | Build plus `Smoke`/fast selected checks as defined in workflow | Protect mainline with timely signal | Required checks pass; no unexpected skip |
| Scheduled/manual | `Regression`, broader `Api`/`Ui`, and `E2E` as defined | Detect wider or environment-dependent regressions | Failure investigated with retained evidence |
| Pre-interview demo | `./scripts/run-demo.sh` | One rehearsed, visible scenario | Hard stop/fallback in `demo-runbook.md` |

The source of truth for CI behaviour is `.github/workflows/qa-pipeline.yml`; documentation must not promise triggers or jobs absent from that file.

## 9. Quality gates

### Pull-request gate

1. Dependency restore succeeds.
2. Release build succeeds with repository compiler/analyzer settings; warnings configured as errors remain errors.
3. Selected fast tests report zero failures.
4. No selected test is unexpectedly skipped or undiscovered.
5. Machine-readable results are published even on failure.
6. Expected diagnostics/artifacts are published on relevant failures.

Branch protection must require the workflow check for this to become an enforced merge gate; workflow success alone does not configure repository governance.

### Scheduled/regression gate

- All selected regression/E2E tests complete with zero unexplained failures.
- Any intermittent result is recorded and classified; automatic rerun success does not erase the initial failure.
- Quarantine, if unavoidable, has an owner, reason, tracking item and expiry and remains visible in reporting.

### Release decision principle

Automation informs release decisions; it does not make them alone. A risk owner considers failed/untested scope, change impact, known defects, environment relevance and non-functional evidence.

## 10. Failure triage

1. Preserve the initial result, stack trace, timestamp, safe data ID, screenshot/log and configuration metadata.
2. Classify the failure layer: test code, data, environment, API/service, browser/UI or CI infrastructure.
3. Reproduce the single test using CI-equivalent settings where practical.
4. Decide whether it is a product defect, test defect, environment incident or unresolved intermittent failure.
5. Record ownership and corrective action. Do not weaken assertions or add blanket retries to recover a green build.

Useful suite-health measures include duration percentile, first-run pass rate, rerun rate, failure cause, time to diagnose, quarantine age and defects found by layer. No baseline improvement is claimed until measurements exist.

## 11. Traceability to the supplied Jacobs role

| Jacobs requirement/responsibility | Strategy/project evidence | Boundary of claim |
|---|---|---|
| Lead design, development and maintenance of automated/manual test solutions | Risk model, layered scope, framework boundaries, exploratory scope and maintenance rules | Portfolio demonstrates an approach, not organisational leadership by itself |
| Define testing requirements and acceptance criteria collaboratively | Risk-to-evidence table and Given/When/Then-style acceptance framing in the deck | Actual stakeholder collaboration requires a truthful professional example |
| Functional testing | API and UI positive/negative/boundary scope | Only implemented behaviours are covered |
| Regression testing | `Smoke`/`Regression` categories and tiered execution | Breadth/cadence follows the workflow and current suite |
| Integration testing | Read-after-write and selected API-to-UI scenario | Does not prove every internal integration |
| Non-functional testing | Explicit risk recognition, non-scope and proposed specialist approach | No performance/security/accessibility certification is claimed |
| Selenium/Cypress/Cucumber experience | Selenium WebDriver, page objects, waits and diagnostics | Project demonstrates Selenium, not Cypress/Cucumber |
| Clean, maintainable Java/JavaScript/C# | C#, nullable types, builders/models, API clients and page objects | Code quality remains open to review and evolution |
| CI/CD tooling | `.github/workflows/qa-pipeline.yml`, category selection, results/artifacts | Branch protection and enterprise deployment are outside this repo |
| Defect tracking | Failure evidence and triage model; tracking item required for quarantine | No specific tracker integration is claimed unless added |
| Problem solving | Test-data isolation, synchronisation, layer selection and fallback strategy | Outcomes are limited to observable repository evidence |
| Collaboration/mentoring/knowledge sharing | Readable docs, conventions and reviewable framework structure | Do not claim people were mentored through this portfolio unless true |

## 12. Entry and exit criteria

### Entry

- Supported .NET SDK and browser are available.
- Configured base URL is reachable and expected public demo behaviour is present.
- Required environment configuration is supplied without exposing secrets.
- Build is clean and intended tests are discoverable.
- Shared environment has no known incident that invalidates results.

### Exit for a test run

- Intended selection executed and zero unexplained failures remain.
- Results and applicable diagnostics are retained.
- Unexpected skips, infrastructure errors and intermittent results are treated as incomplete evidence, not passes.
- Residual risks and unexecuted scope are visible to the decision maker.

## 13. Limitations and next improvements

### Current limitations

- The public SUT and network are uncontrolled; tests may fail because the service, data or UI changed.
- Shared state limits deterministic reset, isolation and safe parallelism.
- The browser matrix and functional scenarios are intentionally narrow.
- Cross-layer tests can locate a failing boundary but cannot prove internal root cause.
- CI evidence is only as strong as retention, permissions and branch-protection configuration.
- Non-functional quality attributes are not tested by this portfolio.
- A green run shows sampled behaviours at one time, not absence of defects.

### Prioritised improvement options

1. Provide a version-pinned, disposable local/containerised SUT and deterministic data reset.
2. Measure duration, first-run reliability and failure causes before tuning cadence or parallelism.
3. Add contract/schema checks where they improve interface-change detection.
4. Improve redacted HTTP/browser diagnostics and artifact indexing.
5. Add accessibility checks plus manual audit scope against agreed criteria.
6. Add performance, resilience and security work only with representative environments, requirements and specialist ownership.

The next change should respond to observed risk. More tooling is not automatically more assurance.
