# F# Hyperfocus Training

A structured, hands-on F# training program designed for deep-focus sessions. Each session is **90 minutes max** — short enough to stay in flow, long enough to build something real.

## Philosophy

- **Type-first thinking** — design with types before writing logic
- **Learn by building** — every session produces working code
- **Progressive complexity** — each session builds on the previous one
- **No fluff** — pure practice, no slides

## Schedule

| # | Session | Focus | Duration | Status |
|---|---------|-------|----------|--------|
| 1 | **Types as Design** | Records, DUs, single-case wrappers, making illegal states unrepresentable | 90 min | 🔵 Today |
| 2 | **Pipelines & Composition** | `\|>`, `>>`, `Option`, `Result`, railway-oriented error handling | 90 min | ⬜ |
| 3 | **Pattern Matching & Recursion** | Exhaustive matching, active patterns, recursive data structures | 90 min | ⬜ |
| 4 | **Modules & Domain Modeling** | Module design, encapsulation, a small bounded context end-to-end | 90 min | ⬜ |
| 5 | **Async & Concurrency** | `task {}`, `and!`, cancellation, channels, `MailboxProcessor` | 90 min | ⬜ |
| 6 | **Integration & Capstone** | JSON serialization, HTTP, CLI app — tie everything together | 90 min | ⬜ |

## Session Format

Each 90-minute session follows this rhythm:

```
[0:00 - 0:10]  Concept intro — read the brief, understand the types
[0:10 - 0:60]  Build — implement the exercise, iterate
[0:60 - 0:80]  Stretch goal — extend or refactor
[0:80 - 0:90]  Review — reflect, commit, note what clicked
```

## Project Structure

```
src/
  Session01/       ← Types as Design
  Session02/       ← Pipelines & Composition
  Session03/       ← Pattern Matching & Recursion
  Session04/       ← Modules & Domain Modeling
  Session05/       ← Async & Concurrency
  Session06/       ← Integration & Capstone
tests/             ← Test projects (optional, on demand)
```

## Session 1 — Types as Design (Today)

**Goal:** Learn to encode domain rules in the type system so invalid states can't exist.

### Warm-up (10 min)
- Create the project structure
- Write your first record type and discriminated union
- Understand the difference: records = data, DUs = choices

### Main Exercise (50 min)
Model a **coffee order system** using only types and pure functions:

1. Define a `CupSize` (Small, Medium, Large)
2. Define a `Milk` option (Whole, Oat, None)
3. Define a `Drink` (Espresso, Latte needs milk, Drip)
4. Create an `Order` record with drink, size, and quantity
5. Write a `validate` function: `RawOrder -> Result<Order, OrderError>`
6. Write a `price` function: `Order -> decimal`

**Key insight:** A `Latte` without milk should be *impossible to construct*, not just validated at runtime.

### Stretch Goal (20 min)
- Add a `Customization` type (extra shot, syrup flavor)
- Make `price` handle customizations
- Try making the `quantity` a constrained type (must be 1–10)

### Review (10 min)
- Commit your work
- Note: which invalid states did the types prevent?

## Getting Started

```bash
# Build all projects
dotnet build

# Run a specific session
dotnet run --project src/Session01

# Run with watch (auto-reload on save)
dotnet watch run --project src/Session01

# Build and run the full solution
dotnet build FSharpWorkout.sln
dotnet run --project src/Session01
```

## Learning: Where Does Validation Happen?

Yes — `RawOrder` is your JSON, your HTTP request body, your CSV row, your user input.
It's the messy outside world. You validate it **once** at the boundary, then work with clean types.

```
  ┌─────────────────────────────────────────────────────────┐
  │                    OUTSIDE WORLD                         │
  │         (JSON, HTTP request, user input, CSV)            │
  │                                                          │
  │   { "drink": "latte", "milk": "oat", "size": "large" }  │
  └──────────────────────┬──────────────────────────────────┘
                         │
                         ▼
  ┌──────────────────────────────────────────────────────────┐
  │              VALIDATION BOUNDARY (parse here)             │
  │                                                           │
  │   RawOrder ──► parseDrink ──► Result<Drink, string>       │
  │                                                           │
  │   Strings go in, domain types or errors come out.         │
  │   This is the ONLY place you validate.                    │
  └──────────────────────┬──────────────────────────────────-─┘
                         │
                         ▼
  ┌───────────────────────────────────────────────────────────┐
  │                CLEAN DOMAIN (safe zone)                    │
  │                                                            │
  │   Drink, Milk, Order — types guarantee correctness.        │
  │   No validation needed. If it compiles, it's valid.        │
  └───────────────────────────────────────────────────────────┘
```

### Example

```fsharp
type Milk = Whole | Oat | Almond
type Drink = Espresso | Latte of Milk

type RawOrder = { DrinkName: string; MilkName: string }

let parseDrink (raw: RawOrder) : Result<Drink, string> =
    match raw.DrinkName with
    | "espresso" -> Ok Espresso
    | "latte" ->
        match raw.MilkName with
        | "whole" -> Ok (Latte Whole)
        | "oat" -> Ok (Latte Oat)
        | "almond" -> Ok (Latte Almond)
        | m -> Error (sprintf "Unknown milk: %s" m)
        //  ^ this is the milk string that didn't match
    | d -> Error (sprintf "Unknown drink: %s" d)
    //  ^ this is the drink string that didn't match
```

The `d` and `m` are just variable names that capture whatever string didn't match any case above.
If someone sends `"mocha"`, then `d = "mocha"` and you return `Error "Unknown drink: mocha"`.

## Learning: Complex Types — Transcription Job Example

A real-world example: modeling a podcast transcription API. Shows how DUs carry different data per state,
how lists work inside types, and how you "update" without mutation.

```fsharp
// Single-case DU — wraps a string so you can't mix up IDs
type TranscriptionId = TranscriptionId of string

type AudioFormat = Mp3 | Wav | Flac

type AudioFile = {
    FileName: string
    Format: AudioFormat
    DurationSeconds: float
}

// A transcription is made of segments — each with timing, text, and confidence
type Segment = {
    Start: float
    End: float
    Text: string
    Confidence: float
}

type TranscriptionError =
    | UnsupportedFormat of string
    | FileTooLarge
    | ApiFailure of string

// Each state carries ONLY the data that makes sense for it
type TranscriptionState =
    | Queued                              // no extra data
    | Processing of progress: float       // how far along
    | Completed of segments: Segment list // the result — a LIST of segments
    | Failed of TranscriptionError        // what went wrong

type TranscriptionJob = {
    Id: TranscriptionId
    Audio: AudioFile
    State: TranscriptionState
}
```

### Creating and "updating" a job (no mutation)

```fsharp
let updateState (job: TranscriptionJob) (newState: TranscriptionState) : TranscriptionJob =
    { job with State = newState }

// Start with a queued job
let job = {
    Id = TranscriptionId "abc"
    Audio = { FileName = "ep01.mp3"; Format = Mp3; DurationSeconds = 1800.0 }
    State = Queued
}

// Move through states — each returns a NEW job
let job2 = updateState job (Processing 0.5)

let job3 = updateState job2 (Completed [
    { Start = 0.0; End = 5.2; Text = "Welcome to the show"; Confidence = 0.95 }
    { Start = 5.2; End = 12.1; Text = "Today we talk about F#"; Confidence = 0.91 }
])

let job4 = updateState job (Failed (ApiFailure "timeout"))
```

### Key takeaways

- `Segment list` — that's how you put a list inside a type. `list` is a built-in F# type, `Segment list` means "a list of Segments"
- `{ job with State = ... }` — copy the job, change one field. No mutation. The old `job` still exists unchanged
- Each state carries only its data — you can't have segments on a `Failed` job or progress on a `Completed` one
- In a loop (polling an API), pass the updated job to the next iteration instead of mutating

## Patterns Worth Studying

| Pattern | What it is | When to use it |
|---------|-----------|----------------|
| **Railway-Oriented Programming** | Chain `Result` values with `bind`/`map`. Two tracks: success and error. First error derails the pipeline — no need for try/catch. | Validation, parsing, any multi-step operation that can fail |
| **Making Illegal States Unrepresentable** | Use DUs so invalid combinations can't be constructed. E.g., `Latte` carries `Milk` — you can't build a Latte without it. | Domain modeling, anywhere you'd otherwise write runtime checks |
| **Parse, Don't Validate** | Convert raw untyped data (strings, ints) into rich domain types at the boundary. Once parsed, the types guarantee correctness. | Input handling, API boundaries, deserialization |

## Rules for Yourself

1. **No mutable variables** — if you reach for `mutable`, rethink
2. **No exceptions for expected failures** — use `Result`
3. **Types before functions** — define the domain types first, then the transformations
4. **Commit after each session** — track your progress
