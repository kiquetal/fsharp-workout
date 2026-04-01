# Learning Notes

## Where Does Validation Happen?

`RawOrder` is your JSON, your HTTP request body, your CSV row, your user input.
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
    | d -> Error (sprintf "Unknown drink: %s" d)
```

The `d` and `m` are just variable names that capture whatever string didn't match any case above.
If someone sends `"mocha"`, then `d = "mocha"` and you return `Error "Unknown drink: mocha"`.

## Complex Types — Transcription Job Example

A real-world example: modeling a podcast transcription API. Shows how DUs carry different data per state,
how lists work inside types, and how you "update" without mutation.

```fsharp
type TranscriptionId = TranscriptionId of string

type AudioFormat = Mp3 | Wav | Flac

type AudioFile = {
    FileName: string
    Format: AudioFormat
    DurationSeconds: float
}

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

type TranscriptionState =
    | Queued
    | Processing of progress: float
    | Completed of segments: Segment list
    | Failed of TranscriptionError

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

let job = {
    Id = TranscriptionId "abc"
    Audio = { FileName = "ep01.mp3"; Format = Mp3; DurationSeconds = 1800.0 }
    State = Queued
}

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

## Result.sequence vs Validation (Collect All Errors)

### The Problem

When validating a list of items, you get back a `Result list`. You need to turn it into a `Result` of a list.
This is the "traverse/sequence" pattern — and there are two strategies.

### Strategy 1: Fail Fast (`Result.sequence`)

Stop at the first error. Good when errors are fatal and there's no point continuing.

```fsharp
module Result =
    let sequence (results: Result<'a, 'e> list) : Result<'a list, 'e> =
        (Ok [], results)
        ||> List.fold (fun acc cur ->
            match acc, cur with
            | Ok cs, Ok c -> Ok (c :: cs)
            | Error e, _ -> Error e
            | _, Error e -> Error e)
        |> Result.map List.rev
```

Once `acc` is `Error`, the `| Error e, _ ->` branch keeps it there — all subsequent items are ignored.

### Strategy 2: Collect All Errors (Validation)

Gather every error so you can report them all at once. Good for user-facing validation (forms, API input).

```fsharp
let validateAll (results: Result<'a, 'e> list) : Result<'a list, 'e list> =
    let oks, errs =
        results |> List.fold (fun (oks, errs) cur ->
            match cur with
            | Ok v -> (v :: oks, errs)
            | Error e -> (oks, e :: errs)) ([], [])
    match errs with
    | [] -> Ok (List.rev oks)
    | _ -> Error (List.rev errs)
```

### Trace through: `[Ok A; Error "x"; Error "y"]`

| Step | oks | errs | cur |
|------|-----|------|-----|
| 0 | `[]` | `[]` | `Ok A` |
| 1 | `[A]` | `[]` | `Error "x"` |
| 2 | `[A]` | `["x"]` | `Error "y"` |

Final: `errs = ["x"; "y"]` → `Error ["x"; "y"]`

### When to use which

| Strategy | Return type | Use when |
|----------|------------|----------|
| Fail fast | `Result<'a list, 'e>` | Errors are fatal, no point collecting more |
| Collect all | `Result<'a list, 'e list>` | User-facing validation, show all problems at once |

## DU Case Data — The `*` Syntax Is Not a Tuple

When you see `*` in a discriminated union case, it means "this case carries multiple pieces of data." It's not a tuple — it's just F#'s syntax for "and."

```fsharp
// One piece of data
| Latte of Milk

// Three pieces of data
| Operation of Operator * Expr * Expr
```

Construct by passing all values:

```fsharp
Operation(Add, Number 2.0, Number 3.0)
//        ^^^  ^^^^^^^^^^  ^^^^^^^^^^
//        op   left expr   right expr
```

Decompose by pattern matching:

```fsharp
match expr with
| Number n -> ...                      // pull out 1 value
| Operation(op, left, right) -> ...    // pull out 3 values
```

The `*` is just how F# says "and" in DU definitions. Think: "an Operator *and* an Expr *and* an Expr."
