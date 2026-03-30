module OrderPipeline

// ============================================
// SESSION 2 — Railway-Oriented Programming
// ============================================
// Session 1 gave you types + basic validation.
// Now you'll chain operations cleanly using
// Result, build a processing pipeline, and
// learn to compose fallible functions.
//
// Work through these steps:
// 1. Define domain types (reuse/refine from S1)
// 2. Write a Result helper module (bind, map, etc.)
// 3. Build a multi-step order pipeline
// 4. Accumulate ALL errors (not just the first)
// 5. Write a discount engine using active patterns
// 6. Compose the full pipeline with |>
// ============================================


// --- Step 1: Domain types ---
// Carry forward from Session 1, but add an Email
// for the customer. Use a single-case DU to make
// it a proper domain primitive.

type Email = Email of string
type CupSize = Small | Medium | Large
type Milk = Oat | Almond | Whole
type Drink =
    | Espresso
    | Americano
    | Latte of Milk
    | Cappuccino of Milk

type Quantity = private Quantity of int

type Order =
    { Customer: Email
      Drink: Drink
      Size: CupSize
      Quantity: Quantity }

type OrderError =
    | InvalidEmail of string
    | InvalidDrink of string
    | InvalidSize of string
    | MilkRequired of string
    | MilkNotAllowed of string
    | InvalidQuantity of int

// Raw unvalidated input
type RawOrder =
    { Email: string
      DrinkName: string
      Size: string
      Milk: string option
      Quantity: int }


// --- Step 2: Result helpers ---
// Build your own mini "railway" toolkit.
// These are the building blocks for chaining
// fallible operations without nested matches.

module Result =
    // TODO: implement bind — chain two Result-returning functions
    // val bind : ('a -> Result<'b, 'e>) -> Result<'a, 'e> -> Result<'b, 'e>
    let bind (f: 'a -> Result<'b, 'e>) (result: Result<'a, 'e>) : Result<'b, 'e> =
        failwith "TODO"

    // TODO: implement map2 — combine two Results with a function
    // val map2 : ('a -> 'b -> 'c) -> Result<'a, 'e> -> Result<'b, 'e> -> Result<'c, 'e>
    let map2 (f: 'a -> 'b -> 'c) (r1: Result<'a, 'e>) (r2: Result<'b, 'e>) : Result<'c, 'e> =
        failwith "TODO"

    // TODO: implement apply — applicative style for Results
    // val apply : Result<('a -> 'b), 'e> -> Result<'a, 'e> -> Result<'b, 'e>
    let apply (fResult: Result<('a -> 'b), 'e>) (xResult: Result<'a, 'e>) : Result<'b, 'e> =
        failwith "TODO"


// --- Step 3: Smart constructors & parsers ---
// Each returns Result<'T, OrderError>

module Validate =
    let email (s: string) : Result<Email, OrderError> =
        // TODO: check it contains '@' and is non-empty
        failwith "TODO"

    let cupSize (s: string) : Result<CupSize, OrderError> =
        // TODO: parse "small", "medium", "large" (case-insensitive)
        failwith "TODO"

    let quantity (n: int) : Result<Quantity, OrderError> =
        // TODO: must be > 0
        failwith "TODO"

    let drink (name: string) (milk: string option) : Result<Drink, OrderError> =
        // TODO: parse drink name, enforce milk rules
        // Espresso/Americano must NOT have milk
        // Latte/Cappuccino MUST have milk
        // Hint: you'll need a helper to parse the milk string
        failwith "TODO"


// --- Step 4: Error accumulation ---
// The pipeline in Session 1 stopped at the first error.
// Now collect ALL validation errors at once.

type ValidationError = ValidationError of OrderError list

module Validation =
    // TODO: implement map2 that accumulates errors in a list
    // val map2 : ('a -> 'b -> 'c) -> Result<'a, OrderError list> -> Result<'b, OrderError list> -> Result<'c, OrderError list>
    let map2 (f: 'a -> 'b -> 'c) (r1: Result<'a, OrderError list>) (r2: Result<'b, OrderError list>) : Result<'c, OrderError list> =
        failwith "TODO"

    // TODO: lift a single-error Result into a list-error Result
    // val liftResult : Result<'a, OrderError> -> Result<'a, OrderError list>
    let liftResult (r: Result<'a, OrderError>) : Result<'a, OrderError list> =
        failwith "TODO"

    // TODO: validate a full RawOrder, accumulating all errors
    // val validateOrder : RawOrder -> Result<Order, OrderError list>
    let validateOrder (raw: RawOrder) : Result<Order, OrderError list> =
        // Hint: use liftResult on each field validator,
        // then combine with map2 (or a custom applicative)
        failwith "TODO"


// --- Step 5: Discount engine with active patterns ---
// Active patterns let you create custom "matchers"
// that decompose data in readable ways.

// TODO: define an active pattern that classifies order size
// (|SmallOrder|MediumOrder|LargeOrder|)
let (|SmallOrder|MediumOrder|LargeOrder|) (order: Order) =
    failwith "TODO"

// TODO: define a partial active pattern for bulk orders (qty >= 5)
// (|BulkOrder|_|)
let (|BulkOrder|_|) (order: Order) =
    failwith "TODO"

type Discount =
    | NoDiscount
    | Percentage of decimal
    | FlatOff of decimal

let calculateDiscount (order: Order) : Discount =
    // TODO: use the active patterns above
    // Large + Bulk -> 15% off
    // Bulk alone -> 10% off
    // Large alone -> 5% off
    // otherwise -> no discount
    failwith "TODO"


// --- Step 6: Full pipeline ---
// Compose everything: validate -> price -> discount -> final price

module Pricing =
    let basePrice (order: Order) : decimal =
        let drinkPrice =
            match order.Drink with
            | Espresso -> 2.00m
            | Americano -> 2.50m
            | Latte _ -> 3.50m
            | Cappuccino _ -> 3.75m
        let sizeMultiplier =
            match order.Size with
            | Small -> 1.0m
            | Medium -> 1.2m
            | Large -> 1.5m
        let (Quantity qty) = order.Quantity
        drinkPrice * sizeMultiplier * decimal qty

    let applyDiscount (price: decimal) (discount: Discount) : decimal =
        match discount with
        | NoDiscount -> price
        | Percentage pct -> price * (1.0m - pct / 100.0m)
        | FlatOff amount -> max 0.0m (price - amount)

    let finalPrice (order: Order) : decimal =
        // TODO: compose basePrice and applyDiscount using
        // calculateDiscount and the pipe operator
        failwith "TODO"


// --- Step 7: Process a batch of orders ---
// Given a list of RawOrders, validate all of them,
// partition into successes/failures, and price the valid ones.

type ProcessedOrder = { Order: Order; Price: decimal }
type BatchResult = { Processed: ProcessedOrder list; Errors: (RawOrder * OrderError list) list }

let processBatch (rawOrders: RawOrder list) : BatchResult =
    // TODO: validate each, partition results, price valid orders
    // Hint: List.map, List.choose or List.partition, then build BatchResult
    failwith "TODO"


// --- Try it out ---
[<EntryPoint>]
let main _ =
    let orders =
        [ { Email = "alice@example.com"; DrinkName = "latte"; Size = "large"; Milk = Some "oat"; Quantity = 6 }
          { Email = ""; DrinkName = "espresso"; Size = "small"; Milk = None; Quantity = 1 }
          { Email = "bob@test.com"; DrinkName = "mocha"; Size = "huge"; Milk = None; Quantity = -1 }
          { Email = "carol@example.com"; DrinkName = "cappuccino"; Size = "medium"; Milk = Some "almond"; Quantity = 2 } ]

    let result = processBatch orders

    printfn "=== Processed Orders ==="
    result.Processed |> List.iter (fun po ->
        let (Email email) = po.Order.Customer
        printfn "  %s: %A (%A) — $%.2f" email po.Order.Drink po.Order.Size po.Price)

    printfn "\n=== Errors ==="
    result.Errors |> List.iter (fun (raw, errs) ->
        printfn "  %s: %A" raw.Email errs)

    0
