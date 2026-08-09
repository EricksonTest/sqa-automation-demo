#!/usr/bin/env bash
set -euo pipefail

export TEST_BASE_URL="${TEST_BASE_URL:-https://automationintesting.online}"
export HEADLESS="${HEADLESS:-true}"

dotnet test --configuration Release \
  --filter "TestCategory=Smoke" \
  --logger "console;verbosity=normal" \
  --logger "trx;LogFileName=smoke.trx" \
  --results-directory TestResults/smoke
