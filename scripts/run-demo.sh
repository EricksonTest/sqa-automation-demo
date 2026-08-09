#!/usr/bin/env bash
set -euo pipefail

export TEST_BASE_URL="${TEST_BASE_URL:-https://automationintesting.online}"
export HEADLESS="${HEADLESS:-false}"
export BROWSER="${BROWSER:-chrome}"

dotnet restore --locked-mode
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build \
  --filter "TestCategory=E2E" \
  --logger "console;verbosity=normal" \
  --logger "trx;LogFileName=e2e-demo.trx" \
  --results-directory TestResults/demo
