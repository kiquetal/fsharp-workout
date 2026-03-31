# Session 2 — Railway-Oriented Programming

**Goal:** Chain fallible operations cleanly using `Result`, accumulate errors, and compose a full processing pipeline.

## Warm-up (10 min)
- Review your Session 1 validation — notice how nested `match` expressions get unwieldy
- Understand the two-track metaphor: every operation either stays on the success track or switches to the error track

## Main Exercise (50 min)
Build an **order processing pipeline** that validates, prices, and discounts orders:

1. **Result helpers** — implement `bind`, `map2`, and `apply`. These are the combinators that eliminate nested matching
2. **Smart constructors** — `Email`, `CupSize`, `Quantity`, `Drink` parsers, each returning `Result<'T, OrderError>`
3. **Error accumulation** — instead of stopping at the first error, collect ALL validation errors using an applicative `map2` that concatenates error lists
4. **Active patterns** — define `(|SmallOrder|MediumOrder|LargeOrder|)` and `(|BulkOrder|_|)` to make discount rules read like English
5. **Discount engine** — combine active patterns: Large + Bulk → 15%, Bulk → 10%, Large → 5%
6. **Full pipeline** — compose validate → price → discount → final price using `|>`
7. **Batch processing** — validate a list of raw orders, partition into successes/failures, price the valid ones

## Key Concepts

| Concept | What it means | Why it matters |
|---------|--------------|----------------|
| `bind` | Chain two `Result`-returning functions | Replaces nested `match` with a flat pipeline |
| `map2` | Combine two `Result` values with a function | Build records from independently validated fields |
| Applicative validation | `map2` that accumulates errors in a list | Show users ALL problems at once, not one at a time |
| Active patterns | Custom decompositions for `match` expressions | Domain-readable pattern matching without exposing internals |

## Active Patterns — Theory

Active patterns let you define custom decompositions for `match` expressions. Instead of matching on raw data, you match on domain concepts.

### The core insight

An active pattern is just a function with a special name. You extract classification logic into it, so `match` reads like business intent instead of implementation details:

```fsharp
// Without: HOW — implementation leaked into every match
match x.Size with
| Large -> ...

// With: WHAT — business concept, logic lives in one place
match x with
| LargeOrder -> ...
```

The `(| |)` banana clip syntax tells F# "this is a pattern you can use in `match`." When `match` sees a pattern name like `LargeOrder`, it finds the active pattern function, calls it with the matched value, and branches on the result. You never see the call — F# does it for you:

```fsharp
// What you write:
match x with
| LargeOrder -> 0.05M

// What F# actually does:
let __result = (|SmallOrder|MediumOrder|LargeOrder|) x
match __result with
| LargeOrder -> 0.05M
```

The connection is by type, not by name — `x` can be called anything, F# just needs it to be an `Order` because that's what the active pattern function accepts.

### Two kinds

**Total active pattern** — always matches one of the cases (like a regular DU):

```fsharp
let (|SmallOrder|MediumOrder|LargeOrder|) (order: Order) =
    match order.Size with
    | Small -> SmallOrder
    | Medium -> MediumOrder
    | Large -> LargeOrder
```

The compiler knows every input maps to exactly one case — exhaustive, no wildcard needed.

**Partial active pattern** — might match, might not (returns `Option`):

```fsharp
let (|BulkOrder|_|) (order: Order) =
    if Quantity.value order.Quantity >= 10 then Some order
    else None
```

The `|_|` in the name means "this can fail to match." Returns `Some` to match, `None` to skip. You need a wildcard `_` or other cases to cover the non-match.

### Using them in `match`

Active patterns make business rules read like English:

```fsharp
let discount (order: Order) =
    match order with
    | LargeOrder & BulkOrder -> 0.15M    // both match — 15%
    | BulkOrder _ -> 0.10M               // bulk only — 10%
    | LargeOrder -> 0.05M                // large only — 5%
    | _ -> 0.0M                          // no discount
```

Compare without active patterns:

```fsharp
let discount (order: Order) =
    if order.Size = Large && Quantity.value order.Quantity >= 10 then 0.15M
    elif Quantity.value order.Quantity >= 10 then 0.10M
    elif order.Size = Large then 0.05M
    else 0.0M
```

Same logic, but the active pattern version hides the "how" (field access, threshold checks) and shows the "what" (business concepts).

### Why this matters

- The `match` expression becomes a **policy table** — readable by non-developers
- The threshold logic lives in one place — change "bulk = 10" once, not everywhere
- You can combine total + partial patterns with `&` (AND) for compound rules
- Active patterns are just functions — they compose, they're testable

### Parameterized active patterns

Active patterns can take extra arguments. The last parameter is always the matched value (passed by `match` automatically). Everything before it is yours:

```fsharp
let (|GreaterThan|_|) threshold n =
//                    ^^^^^^^^^  ^
//                    you pass   match passes automatically
    if n > threshold then Some n else None

match quantity with
| GreaterThan 10 -> "bulk"       // threshold=10, n=quantity
| GreaterThan 5  -> "medium"     // threshold=5,  n=quantity
| _ -> "small"
```

This makes active patterns reusable — same pattern, different thresholds.

### `&` (AND) pattern

`&` means "match both patterns on the same value":

```fsharp
| LargeOrder & BulkOrder -> ...   // order must be BOTH large AND bulk
```

This only works because `LargeOrder` is a case from a total pattern and `BulkOrder` is a partial pattern. F# checks both against the same input.

## Stretch Goal (20 min)
- Add a `(|LoyalCustomer|_|)` active pattern based on email domain
- Implement `Result.map3` and `Result.map4` for combining more fields
- Refactor `processBatch` to use `List.partitionMap` (or write your own)

## Review (10 min)
- Compare your Session 1 `validate` with Session 2's pipeline — how much cleaner is it?
- Note: which pattern (fail-fast vs accumulate) would you use for an API? A CLI?
