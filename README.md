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
| 1 | **Types as Design** | Records, DUs, single-case wrappers, making illegal states unrepresentable | 90 min | ✅ Done |
| 2 | **Pipelines & Composition** | `\|>`, `>>`, `Option`, `Result`, railway-oriented error handling | 90 min | ✅ Done |
| 3 | **Pattern Matching & Recursion** | Exhaustive matching, active patterns, recursive data structures | 90 min | ✅ Done |
| 4 | **Modules & Domain Modeling** | Module design, encapsulation, a small bounded context end-to-end | 90 min | ✅ Done |
| 5 | **Collections & Pipelines** | `List.map`, `List.groupBy`, `List.collect`, pipeline composition with `\|>` and `>>` | 90 min | ✅ Done |
| 6 | **Integration & Capstone** | JSON serialization, HTTP, CLI app — tie everything together | 90 min | ⬜ |

## Part II — State Machines & Data Transformation

| # | Session | What you build | Key concept |
|---|---------|---------------|-------------|
| 7 | **Explicit State Machines** | A deployment pipeline (Pending → Building → Testing → Deployed / Failed) | DUs as states, transition functions that only accept valid moves |
| 8 | **Event Sourcing from Scratch** | An order lifecycle — rebuild current state by replaying events | Events as DUs, state = fold over event list |
| 9 | **Parsing & Transformation** | A log file parser — raw text → structured records → summary report | Active patterns, `Result` chains, pipeline composition |
| 10 | **Recursive Data Structures** | A file system tree or config hierarchy — traverse, transform, flatten | Trees as recursive DUs, catamorphisms (folds over trees) |
| 11 | **Workflow Engine** | A multi-step approval process with branching and rollback | Combining state machines + event sourcing |
| 12 | **Capstone: Infrastructure DSL** | A small DSL that describes infrastructure and validates it at compile time | Computation expressions, type-safe builders |

### Progression

- **Session 7** teaches the raw pattern — a state machine is just a function `State → Event → State`
- **Session 8** shows that storing events gives you time travel for free (replay, audit, debugging)
- **Sessions 9–10** build transformation muscles — taking messy data and shaping it
- **Session 11** combines both skills into something real
- **Session 12** ties it to systems engineering — building a tiny infrastructure DSL (think mini Pulumi/Terraform in F#)

### Where Temporal fits in

After building state machines by hand (Sessions 7–8), you'll understand what tools like [Temporal](https://temporal.io) abstract away:
- **F# state machines** = the logic (which transitions are valid, compile-time safety)
- **Temporal** = the runtime (durable execution, retries, surviving crashes across distributed services)

## Sessions

### Session 1 — Types as Design ✅

**Objective:** Encode domain rules in the type system so invalid states can't exist.

**What you learned:**
- Records for data, discriminated unions for choices
- Single-case DUs with `private` as domain primitives (`Quantity`, `OrderNumber`)
- Smart constructors — validate once at the boundary, trust the types after
- `Result<'T, 'E>` for expected failures instead of exceptions
- Units of measure for compile-time dimensional safety

→ [Full details](docs/session01.md)

### Session 2 — Pipelines & Composition ✅

**Objective:** Chain fallible operations cleanly, accumulate errors, and compose processing pipelines.

**What you learned:**
- `bind`, `map2`, `apply` — the Result combinators that replace nested matching
- Applicative validation — collect ALL errors instead of stopping at the first
- Active patterns — custom decompositions for readable `match` expressions
- Pipeline composition with `|>` from validation through pricing
- Batch processing — partition successes and failures across a list

→ [Full details](docs/session02.md)

### Session 3 — Pattern Matching & Recursion ✅

**Objective:** Exhaustive matching, recursive data structures, and generic folds.

**What you learned:**
- Recursive DU types (`Expr` — a type that references itself)
- Structural recursion — one match branch per DU case
- `evaluate` with `Result.bind` for railway-oriented recursion
- `format` for recursive pretty-printing
- `simplify` for tree transformation (algebraic rules)
- Generic `fold` (catamorphism) — extracting the common recursion pattern
- Factory worker analogy — each branch node combines finished results from below

→ [Full details](docs/session03.md)

### Session 4 — Modules & Domain Modeling ✅

**Objective:** Organize domain logic into modules with clear boundaries.

**What you learned:**
- Module design — group related functions, each module validates what it owns
- `Book` module — status transitions (Available ↔ CheckedOut)
- `Member` module — borrow limits, duplicate checks
- `Library` module — orchestration, calling Book + Member and combining results
- Record updates with `{ x with ... }` — immutable state transitions
- `Map` as in-memory storage — `tryFind`, `add`, `filter`, `values`
- Query functions — `availableBooks`, `booksBorrowedBy`
- Domain vs storage separation — domain stays pure, repository translates

→ [Full details](docs/session04.md)

### Session 5 — Collections & Pipelines ✅

**Objective:** Master F#'s collection functions and pipeline composition.

**What you learned:**
- `List.map`, `List.filter`, `List.sumBy` — basic transformations
- `List.collect` — flatMap for F#, use when your lambda returns a list
- `List.groupBy` — group items by key, returns `(key * items) list`
- `List.sortByDescending`, `List.truncate` — ranking and limiting
- `List.choose` — map + filter in one step using `Option`
- `snd` / `fst` — extract tuple elements
- `>>` composition — build reusable pipelines without data
- `|>` vs `>>` — "run now" vs "save for later"

→ [Full details](docs/session05.md)

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
docs/
  session01.md     ← Full session 1 details, exercises, concepts
  session02.md     ← Full session 2 details, exercises, concepts
  session03.md     ← Full session 3 details, exercises, concepts
  session04.md     ← Full session 4 details, exercises, concepts
  patterns.md      ← Railway-oriented programming, parse don't validate
  java21-functional.md ← F# patterns translated to Java 21+
  learning-notes.md    ← Validation boundaries, complex types, Result.sequence
tests/             ← Test projects (optional, on demand)
```

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
```

## Rules for Yourself

1. **No mutable variables** — if you reach for `mutable`, rethink
2. **No exceptions for expected failures** — use `Result`
3. **Types before functions** — define the domain types first, then the transformations
4. **Commit after each session** — track your progress

## Reference Docs

- [Patterns worth studying](docs/patterns.md) — railway-oriented programming, making illegal states unrepresentable, parse don't validate
- [F# → Java 21+ translation](docs/java21-functional.md) — map/flatMap, sealed interfaces, smart constructors, Stream pipelines
- [Learning notes](docs/learning-notes.md) — validation boundaries, complex types example, Result.sequence vs validation
