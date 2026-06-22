# Package Validation

Unity version: `6000.3.5f1`

Expected local file references:

```json
"com.deucarian.gameplay-foundation": "file:C:/Repositories/Deucarian/Gameplay-Foundation",
"com.deucarian.persistence": "file:C:/Repositories/Deucarian/Persistence",
"com.deucarian.progression": "file:C:/Repositories/Deucarian/Progression",
"com.deucarian.combat": "file:C:/Repositories/Deucarian/Combat"
```

Direct `-runTests` may no-op after import on this machine; use the batch Test Runner API harness if needed.

IL2CPP, Burst, mobile runtime, and Entities compatibility are unverified until separately tested.

## Recorded Results

- Clean project: `C:/Repositories/Deucarian/Combat-TestProject`
- Import command: `Unity.exe -batchmode -quit -projectPath C:/Repositories/Deucarian/Combat-TestProject -logFile C:/Repositories/Deucarian/Combat-TestProject-import.log`
- Import result: return code `0`; no compiler errors found.
- Direct command: `Unity.exe -batchmode -quit -projectPath C:/Repositories/Deucarian/Combat-TestProject -runTests -testPlatform EditMode -testResults C:/Repositories/Deucarian/Combat-TestProject-direct-tests.xml -logFile C:/Repositories/Deucarian/Combat-TestProject-direct-tests.log`
- Direct result: return code `0`, but no XML was produced.
- Batch command: `Unity.exe -batchmode -projectPath C:/Repositories/Deucarian/Combat-TestProject -executeMethod BatchEditModeTestRunner.Run -batchTestResults C:/Repositories/Deucarian/Combat-TestProject-batch-5.txt -logFile C:/Repositories/Deucarian/Combat-TestProject-batch-5.log`
- Batch result: `result=Passed; passCount=14; failCount=0; skipCount=0; duration=0,335`
- Repeat command: `Unity.exe -batchmode -projectPath C:/Repositories/Deucarian/Combat-TestProject -executeMethod BatchEditModeTestRunner.Run -batchTestResults C:/Repositories/Deucarian/Combat-TestProject-batch-repeat.txt -logFile C:/Repositories/Deucarian/Combat-TestProject-batch-repeat.log`
- Repeat result: `result=Passed; passCount=14; failCount=0; skipCount=0; duration=0,298`

The suite includes a repeatable microbenchmark/allocation harness over health query, status lookup, simple damage resolution, multi-component damage resolution, zero-tick status advancement, and target selection. The durable output path is:

`C:/Repositories/Deucarian/Combat-TestProject/Logs/combat-microbenchmark-results.json`

The Phase 1D requirement-to-test matrix is maintained in the Phase 1E preflight documentation so the accepted Combat package report remains concise.
