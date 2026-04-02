# Session 5 — Collections & Pipelines

**Goal:** Master F#'s collection functions and pipeline composition. Transform, filter, group, and aggregate data fluently.

## Why This Session?

Sessions 1–4 focused on types, errors, recursion, and modules. But most real code is about processing collections of things — lists of orders, batches of events, streams of data. This session builds that muscle.

## The Domain: Order Analytics

You run an online store. You have a list of orders and need to answer business questions:
- What's the total revenue?
- Which products sell the most?
- What's the average order value per customer?
- Which customers are "high value" (spent over a threshold)?
- What's the revenue breakdown by month?

## Warm-up (10 min)

### Step 1: Define the types

```fsharp
type CustomerId = CustomerId of string
type ProductId = ProductId of string

type OrderLine = {
    Product: ProductId
    Quantity: int
    UnitPrice: float
}

type Order = {
    Id: string
    Customer: CustomerId
    Lines: OrderLine list
    PlacedAt: System.DateTime
}
```

### Step 2: Build sample data

Create 8–10 orders with different customers, products, dates, and quantities. This is your test dataset.

## Main Exercise (50 min)

### Step 3: Basic aggregations

Write these as pipeline functions (`orders |> ...`):

- `totalRevenue : Order list -> float`
- `orderTotal : Order -> float` (helper — sum of line totals)
- `largestOrder : Order list -> Order option`
- `averageOrderValue : Order list -> float`

Key functions: `List.map`, `List.sumBy`, `List.maxBy`, `List.averageBy`

### Step 4: Grouping and filtering

- `revenueByCustomer : Order list -> (CustomerId * float) list`
- `highValueCustomers : float -> Order list -> CustomerId list` (spent over threshold)
- `topSellingProducts : int -> Order list -> (ProductId * int) list` (top N by quantity)

Key functions: `List.groupBy`, `List.sortByDescending`, `List.truncate`, `List.collect`

### Step 5: Time-based analysis

- `revenueByMonth : Order list -> (int * int * float) list` (year, month, revenue)
- `ordersInRange : System.DateTime -> System.DateTime -> Order list -> Order list`

### Step 6: Stretch — composition

Combine your functions using `>>` to build reusable pipelines:

```fsharp
// a pipeline that takes orders and returns a formatted report
let monthlyReport = revenueByMonth >> List.map formatMonthRow >> String.concat "\n"
```

## Key Collection Functions

| Function | What it does | Java equivalent |
|----------|-------------|-----------------|
| `List.map` | Transform each item | `.stream().map()` |
| `List.filter` | Keep items matching predicate | `.stream().filter()` |
| `List.choose` | Map + filter in one (returns Option) | `.stream().map().filter().map()` |
| `List.collect` | Map to lists, then flatten | `.stream().flatMap()` |
| `List.groupBy` | Group by key | `Collectors.groupingBy()` |
| `List.sortBy` | Sort by key | `.stream().sorted()` |
| `List.sumBy` | Sum by projection | `.stream().mapToInt().sum()` |
| `List.averageBy` | Average by projection | — |
| `List.truncate` | Take first N | `.stream().limit()` |
| `List.distinct` | Remove duplicates | `.stream().distinct()` |
| `List.partition` | Split into two lists by predicate | `Collectors.partitioningBy()` |
| `List.fold` | Reduce with accumulator | `.stream().reduce()` |

## Gotchas & Insights

### `List.map` vs `List.collect` — when your lambda returns a list

If your lambda returns a list, `List.map` nests them:

```fsharp
orders |> List.map (fun o -> o.Lines)
// type: OrderLine list list  ← nested!
// [ [line1; line2]; [line3]; [line4; line5] ]
```

`List.collect` does the same but flattens:

```fsharp
orders |> List.collect (fun o -> o.Lines)
// type: OrderLine list  ← flat
// [line1; line2; line3; line4; line5]
```

Rule: if your lambda returns a single value, use `List.map`. If it returns a list, use `List.collect`.

### `List.collect` is `flatMap`

```fsharp
// get all order lines from all orders
orders |> List.collect (fun o -> o.Lines)

// Java equivalent
orders.stream().flatMap(o -> o.getLines().stream())
```

### `List.choose` vs `List.map` + `List.filter`

```fsharp
// two steps
items |> List.map tryParse |> List.choose id

// one step — choose does both
items |> List.choose tryParse
```

### Pipelines read top-to-bottom

```fsharp
orders
|> List.filter (fun o -> o.Customer = customerId)
|> List.collect (fun o -> o.Lines)
|> List.sumBy (fun line -> float line.Quantity * line.UnitPrice)
```

Each line is one transformation. Read it like a recipe.

### `>>` vs `|>`

- `|>` passes a value: `x |> f` = `f x`
- `>>` composes functions: `f >> g` = `fun x -> g (f x)`

Use `|>` when you have data. Use `>>` when you're building a pipeline to use later.

## Key Concepts

| Concept | What it means | Where you'll see it |
|---------|--------------|---------------------|
| Pipeline | Chain of transformations | `orders \|> filter \|> map \|> sum` |
| Projection | Extract one field/value | `fun o -> o.Customer` |
| Grouping | Partition by key | `List.groupBy (fun o -> o.Customer)` |
| Aggregation | Reduce to single value | `List.sumBy`, `List.averageBy` |
| Composition | Build reusable pipelines | `f >> g >> h` |

## Review (10 min)
- Look at your pipelines — can you read each one as a sentence?
- Could you add a new query without changing existing functions?
- Notice: no loops, no mutable variables, no indices — just transformations
