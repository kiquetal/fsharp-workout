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
# Create solution and first project
dotnet new sln -n FSharpWorkout
dotnet new console -lang F# -o src/Session01
dotnet sln add src/Session01
```

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
