# Session 1 — Types as Design

**Goal:** Learn to encode domain rules in the type system so invalid states can't exist.

## Warm-up (10 min)
- Create the project structure
- Write your first record type and discriminated union
- Understand the difference: records = data, DUs = choices

## Main Exercise (50 min)
Model a **coffee order system** using only types and pure functions:

1. Define a `CupSize` (Small, Medium, Large)
2. Define a `Milk` option (Whole, Oat, None)
3. Define a `Drink` (Espresso, Latte needs milk, Drip)
4. Create an `Order` record with drink, size, and quantity
5. Write a `validate` function: `RawOrder -> Result<Order, OrderError>`
6. Write a `price` function: `Order -> decimal`

**Key insight:** A `Latte` without milk should be *impossible to construct*, not just validated at runtime.

## Stretch Goal (20 min)
- Add a `Customization` type (extra shot, syrup flavor)
- Make `price` handle customizations
- Try making the `quantity` a constrained type (must be 1–10)
- Add units of measure to pricing so the compiler catches dimensional mistakes

## Units of Measure for Pricing

F# can track physical/domain units through arithmetic at compile time — zero runtime cost.
Apply this to the coffee pricing so you can't accidentally add a price to a quantity:

```fsharp
[<Measure>] type usd
[<Measure>] type cup

let basePrice = 3.50M<usd/cup>
let qty = 2<cup>

let total = basePrice * decimal qty   // decimal<usd> ✓ — compiler derives the unit
let nonsense = basePrice + decimal qty // compile error — can't add usd/cup + cup
```

The rules follow dimensional analysis (same as physics):
- **Addition/subtraction** — units must match (`usd + usd` ✓, `usd + cup` ✗)
- **Multiplication** — units combine (`usd/cup × cup = usd`, numerator/denominator cancel)
- **Division** — units divide (`usd / cup = usd/cup`)

Units are erased at runtime — it's just `decimal` underneath. All checking happens at compile time.

## Domain Primitives & Smart Constructors

Wrap raw primitives in single-case DUs when they carry a domain constraint or distinct meaning.
This prevents accidental misuse — the compiler won't let you mix up values that share the same underlying type.

```fsharp
// Without wrapper: Quantity is just int — compiler can't tell it apart from a price or age
type Order = { Drink: Drink; Size: CupSize; Quantity: int }
let total = price order + decimal order.Quantity  // compiles, but nonsense (money + count)

// With wrapper + private: Quantity is its own type AND can only be created through validation
type Quantity = private Quantity of int

module Quantity =
    let create (n: int) : Result<Quantity, OrderError> =
        if n > 0 then Ok(Quantity n)
        else Error(InvalidQuantity n)

    let value (Quantity n) = n  // the only way to read the inner int from outside
```

The `private` on the DU case means code outside this module **cannot** construct `Quantity` directly or pattern-match on it.
`Quantity.create` is the only way in — it guarantees the value is always valid.
`Quantity.value` is the only way out — it unwraps the inner `int` for you:

```fsharp
// ✗ won't compile outside the defining module — constructor is private
let q = Quantity 5

// ✓ must go through the smart constructor
let q = Quantity.create 5  // Result<Quantity, OrderError>

// ✓ read the value through the exposed unwrapper
let total = basePrice * decimal (Quantity.value order.Quantity)
```

**When to wrap:** the value has a constraint (positive, non-empty, format) or a distinct domain meaning.
**When not to wrap:** loop counters, intermediate calculations, local throwaway values.

## Using the Unwrapper (`quantityValue`)

When the DU constructor is `private`, external code can't pattern match on it directly.
That's why the smart constructor module exposes an unwrapper — it's just a getter, no validation:

```fsharp
module SmartConstructors =
    let create (n: int) : Result<Quantity, OrderError> =
        if n > 0 then Ok (Quantity n)
        else Error (InvalidQuantity n)

    let quantityValue (Quantity n) = n  // destructure in the parameter — same as pattern matching
```

You call `create` once at the boundary. After that, `quantityValue` just extracts the `int` whenever you need it:

```fsharp
// at the boundary — validates
let quantityResult = SmartConstructors.create raw.Quantity

// inside the domain — just unwraps, no validation
basePrice * sizeMultiplier * decimal (SmartConstructors.quantityValue order.Quantity) + customizationCost
```

The `(Quantity n)` syntax in the parameter is shorthand for pattern matching — since there's only one case, F# lets you destructure directly in the argument position. Same idea as `let fst (a, _) = a` for tuples.

## Composing Orders: Order vs OrderItem

As the domain grows, a single `Order` record does double duty — it's both "one line item" and "the whole order."
Split them: rename the current `Order` to `OrderItem`, then compose into a real `Order`:

```fsharp
type OrderItem =
    { Drink: Drink
      Size: CupSize
      Quantity: Quantity
      Customizations: CustomizationType list }

type OrderNumber = private OrderNumber of string

module OrderNumber =
    let create (s: string) : Result<OrderNumber, OrderError> =
        if System.String.IsNullOrWhiteSpace(s) then Error (InvalidOrderNumber s)
        else Ok (OrderNumber s)

    let value (OrderNumber s) = s

type Order =
    { Number: OrderNumber
      Items: OrderItem list }
```

Protect `Order` creation with a smart constructor to prevent empty orders:

```fsharp
module Order =
    let create (number: OrderNumber) (items: OrderItem list) : Result<Order, OrderError> =
        match items with
        | [] -> Error EmptyOrder
        | _ -> Ok { Number = number; Items = items }
```

`OrderNumber` uses `private` on the DU case — same pattern as `Quantity`.
`Order` is a record, so you can't make its constructor `private` the same way.
Instead, the protection bubbles up from the parts: if `OrderNumber` and `Quantity` are private,
nobody can build a valid `Order` without going through the smart constructors.

## Review (10 min)
- Commit your work
- Note: which invalid states did the types prevent?
