# Patterns Worth Studying

| Pattern | What it is | When to use it |
|---------|-----------|----------------|
| **Railway-Oriented Programming** | Chain `Result` values with `bind`/`map`. Two tracks: success and error. First error derails the pipeline — no need for try/catch. | Validation, parsing, any multi-step operation that can fail |
| **Making Illegal States Unrepresentable** | Use DUs so invalid combinations can't be constructed. E.g., `Latte` carries `Milk` — you can't build a Latte without it. | Domain modeling, anywhere you'd otherwise write runtime checks |
| **Parse, Don't Validate** | Convert raw untyped data (strings, ints) into rich domain types at the boundary. Once parsed, the types guarantee correctness. | Input handling, API boundaries, deserialization |

## Active Patterns — The Three Kinds

Active patterns are functions with special names that F# can call inside `match`. There are three kinds, and they differ in what they return and how you call them.

### 1. Complete (Total) — returns a label, no data

```fsharp
let (|SmallOrder|MediumOrder|LargeOrder|) (order: Order) =
    match order.Size with
    | Small  -> SmallOrder
    | Medium -> MediumOrder
    | Large  -> LargeOrder

match order with
| LargeOrder  -> "bulk discount"   // no binding — just a label
| MediumOrder -> "standard"
| SmallOrder  -> "economy"
```

- Every input matches exactly one case — exhaustive, no `_` needed
- Returns a **label** (which bucket), not data
- Think of it as a classifier/switch

### 2. Partial — returns `Some data` or `None`

```fsharp
let (|BulkOrder|_|) (order: Order) =
    if Quantity.value order.Quantity >= 10 then Some order
    else None

match order with
| BulkOrder o -> printfn "Bulk: %A" o   // o = the order (from Some)
| _           -> printfn "Normal"        // None → didn't match
```

- `_|` in the name = "might not match"
- Returns `Some value` → match succeeds, you can bind the value
- Returns `None` → match skips to next case
- Always needs a wildcard `_` or other cases as fallback

### 3. Parameterized Partial — you pass args, match passes the last one

```fsharp
let (|MoreShotsThan|_|) threshold coffee =
//                      ^^^^^^^^^  ^^^^^^
//                      you pass   match passes
    if coffee.Shots > threshold then Some coffee.Shots else None

// In match — you provide threshold, match provides coffee:
match redeye with
| MoreShotsThan 3 shots -> printfn "Intense! %d shots" shots
| MoreShotsThan 1 shots -> printfn "Strong, %d shots" shots
| _                     -> printfn "Normal"

// Direct call — you provide BOTH:
(|MoreShotsThan|_|) 3 redeye   // Some 4
(|MoreShotsThan|_|) 3 drip     // None
```

- Last parameter is always the match target (F# fills it in)
- Everything before it is yours to supply in the pattern
- Same pattern, different thresholds → reusable

### Summary Table

| Kind | Syntax | Returns | Needs `_`? | Use case |
|------|--------|---------|------------|----------|
| Complete | `(\|A\|B\|C\|)` | Label (which case) | No — exhaustive | Classifying into buckets |
| Partial | `(\|A\|_\|)` | `Some data` or `None` | Yes | Filtering + extracting |
| Parameterized | `(\|A\|_\|) param value` | `Some data` or `None` | Yes | Reusable filters with thresholds |

### The one rule

In a `match`, F# always passes the matched value as the **last argument**. You never write it — it's implicit. Everything else you provide explicitly in the pattern.

## Teaching Checklist

When explaining a new F# concept, always cover these aspects — don't skip any:

1. **What is it?** — one sentence definition
2. **What are ALL the features?** — list every variant/capability (e.g., total, partial, AND parameterized active patterns)
3. **How does it get called?** — show the explicit invocation, not just the sugar
4. **What are the parameters?** — explain every parameter, especially implicit ones (e.g., `match` passes the last argument automatically)
5. **What does it return?** — show the return type and what each case produces
6. **How does it compose?** — how does it connect with other concepts already learned (pipes, Result, map, bind)
7. **Java 21 equivalent** — show the closest Java translation for comparison