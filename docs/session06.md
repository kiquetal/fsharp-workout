# Session 6 — Integration & Capstone

**Goal:** Tie together everything from Sessions 1–5 into one cohesive CLI application. No new concepts — just applying what you know.

## The Domain: Weather Station Dashboard

You receive weather readings from multiple stations. Build a CLI tool that:
1. Parses raw readings (strings → typed data)
2. Validates them (reject bad data with errors)
3. Aggregates them (averages, extremes, by station, by date)
4. Displays a formatted report

## Skills Used

| Session | Skill | Where it appears |
|---------|-------|-----------------|
| 1 | Types as design | `StationId`, `Temperature`, `Reading` |
| 2 | Result pipelines | Parsing and validation chains |
| 3 | Pattern matching | Matching on validation errors, DU cases |
| 4 | Modules | `Parsing`, `Validation`, `Analytics`, `Report` |
| 5 | Collections & pipelines | Grouping, aggregating, formatting |

## Step 1: Define the types (10 min)

Domain types:
- `StationId` — single-case DU wrapper
- `Temperature` — single-case DU with validation (reject < -100 or > 100)
- `Humidity` — single-case DU with validation (reject < 0 or > 100)
- `Reading` — station, temperature, humidity, timestamp
- `ParseError` — what can go wrong when parsing raw input

Raw input format (one reading per line):
```
station-01,23.5,65,2024-03-15T10:30:00
station-02,bad,70,2024-03-15T11:00:00
station-01,18.2,45,2024-03-15T12:00:00
```

## Step 2: Parsing module (15 min)

Parse a raw string line into a `Reading`:
- Split by comma
- Parse each field
- Return `Result<Reading, ParseError>`

```fsharp
module Parsing =
    val parseLine : string -> Result<Reading, ParseError>
    val parseAll : string list -> Reading list * ParseError list
```

`parseAll` uses `List.partition` or `List.choose` to separate successes from failures.

## Step 3: Validation module (10 min)

Additional business rules beyond parsing:
- Temperature must be between -50 and 60 (realistic range)
- Humidity must be between 0 and 100
- Timestamp must not be in the future

```fsharp
module Validation =
    val validateReading : Reading -> Result<Reading, ValidationError>
```

## Step 4: Analytics module (20 min)

Aggregate validated readings — reuse Session 5 patterns:

```fsharp
module Analytics =
    val averageTemperature : Reading list -> float
    val averageHumidity : Reading list -> float
    val hottestReading : Reading list -> Reading option
    val coldestReading : Reading list -> Reading option
    val readingsByStation : Reading list -> (StationId * Reading list) list
    val dailyAverages : Reading list -> (System.DateTime * float * float) list  // date, avgTemp, avgHumidity
```

## Step 5: Report module (15 min)

Format the analytics into a readable report:

```fsharp
module Report =
    val stationSummary : StationId -> Reading list -> string
    val fullReport : Reading list -> ParseError list -> string
```

The report should show:
- How many readings parsed, how many failed
- Overall averages
- Per-station breakdown
- Hottest and coldest readings

## Step 6: Main — wire it all up (10 min)

```fsharp
rawLines
|> Parsing.parseAll        // string list → (Reading list * ParseError list)
|> fun (readings, errors) ->
    let validated = readings |> List.choose (Validation.validateReading >> Result.toOption)
    Report.fullReport validated errors
|> printfn "%s"
```

## Stretch Goals

- Read from a file instead of hardcoded data
- Add `--station` flag to filter by station
- Add `--format json` flag to output JSON instead of text
- Use `System.Text.Json` for JSON serialization

## Review (10 min)
- Count your modules — each one has a single responsibility
- Every function is pure except `main` (which does IO)
- The pipeline reads like a sentence: parse → validate → analyze → report
- You used every skill from Sessions 1–5 without being told which one to use
