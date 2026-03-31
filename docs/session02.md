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

## Stretch Goal (20 min)
- Add a `(|LoyalCustomer|_|)` active pattern based on email domain
- Implement `Result.map3` and `Result.map4` for combining more fields
- Refactor `processBatch` to use `List.partitionMap` (or write your own)

## Review (10 min)
- Compare your Session 1 `validate` with Session 2's pipeline — how much cleaner is it?
- Note: which pattern (fail-fast vs accumulate) would you use for an API? A CLI?
