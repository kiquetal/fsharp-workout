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

## Why Parameterized Active Patterns Are Confusing

With complete patterns, everything is visible — one parameter, it's obviously the thing you're matching:

```fsharp
let (|LargeOrder|SmallOrder|) (order: Order) =
//                              ^^^^^
//                              one param, clearly the order
    match order.Size with
    | Large -> LargeOrder
    | _     -> SmallOrder

match order with
| LargeOrder -> ...   // obvious: order goes in
```

With parameterized patterns, the match value becomes **invisible**:

```fsharp
let (|PricedOver|_|) (threshold: decimal) (coffee: Coffee) =
//                    ^^^^^^^^^^^^^^^^^^   ^^^^^^^^^^^^^^^
//                    you pass this        match passes this (ALWAYS last)
    if coffee.Price > threshold then Some coffee.Price else None

match coffee with
| PricedOver 6.00m price -> ...
//           ^^^^^  ^^^^^
//           threshold    NOT coffee — this is destructuring the Some
```

There's no syntax that tells you which parameter is the match target. You have to look at the function definition and remember: **last parameter = match target**.

### The value after the pattern is destructuring, not input

This is the hardest part. In `| PricedOver 6.00m price ->`, `price` is **not** the coffee being passed in. It's the binding of what `Some` returned — just like normal `Option` destructuring:

```fsharp
// Normal Option destructuring:
match Some 7.50m with
| Some price -> price       // price = 7.50m

// Active pattern — same thing, Some is hidden:
match coffee with
| PricedOver 6.00m price -> price   // price = 7.50m (from Some 7.50m)
```

You can ignore the value if you don't need it — all three are equivalent:

```fsharp
| PricedOver 6.00m price -> ...   // bind it, use it
| PricedOver 6.00m _     -> ...   // explicitly ignore it
| PricedOver 6.00m       -> ...   // implicitly ignore it (shorter)
```

### Type annotations are your friend

When confused, add types to the active pattern definition:

```fsharp
let (|PricedOver|_|) (threshold: decimal) (coffee: Coffee) : decimal option =
//                    ^^^^^^^^^^^^^^^^^^   ^^^^^^^^^^^^^^^   ^^^^^^^^^^^^^^
//                    you pass this        match passes this  what you destructure
```

Read it as: "give me a threshold and a coffee, I'll maybe give you back a decimal."

### How to read a parameterized pattern in match

```fsharp
match coffee with
| PricedOver 6.00m price -> sprintf "Premium ($%M)" price
```

Read it as:
1. F# calls `(|PricedOver|_|) 6.00m coffee` — you gave `6.00m`, match gave `coffee`
2. If it returns `Some x` → match succeeds, `price` binds to `x`
3. If it returns `None` → skip to next case

## Teaching Checklist

When explaining a new F# concept, always cover these aspects — don't skip any:

1. **What is it?** — one sentence definition
2. **What are ALL the features?** — list every variant/capability (e.g., total, partial, AND parameterized active patterns)
3. **How does it get called?** — show the explicit invocation, not just the sugar
4. **What are the parameters?** — explain every parameter, especially implicit ones (e.g., `match` passes the last argument automatically)
5. **What does it return?** — show the return type and what each case produces
6. **How does it compose?** — how does it connect with other concepts already learned (pipes, Result, map, bind)
7. **Java 21 equivalent** — show the closest Java translation for comparison