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
    | InvalidMilk of string

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
    // val bind : ('a -> Result<'b, 'e>) -> Result<'a, 'e> -> Result<'b, 'e>
    let bind (f: 'a -> Result<'b, 'e>) (result: Result<'a, 'e>) : Result<'b, 'e> =
          match result with
            | Ok value -> f value
            | Error err -> Error err

    // val map2 : ('a -> 'b -> 'c) -> Result<'a, 'e> -> Result<'b, 'e> -> Result<'c, 'e>
    let map2 (f: 'a -> 'b -> 'c) (r1: Result<'a, 'e>) (r2: Result<'b, 'e>) : Result<'c, 'e> =
        match r1, r2 with
        | Ok v1, Ok v2 -> Ok (f v1 v2)
        | Error e, _ -> Error e
        | _, Error e -> Error e

    // val apply : Result<('a -> 'b), 'e> -> Result<'a, 'e> -> Result<'b, 'e>
    let apply (fResult: Result<('a -> 'b), 'e>) (xResult: Result<'a, 'e>) : Result<'b, 'e> =
        match fResult, xResult with
        | Ok f, Ok v -> Ok (f v)
        | Error e, _ -> Error e
        | _, Error e -> Error e


// --- Step 3: Smart constructors & parsers ---
// Each returns Result<'T, OrderError>

module Validate =
    let email (s: string) : Result<Email, OrderError> =
        if s.Contains("@") && s.Length > 0
            then Ok (Email s)
        else
            Error (InvalidEmail s)

    let helperParserMilk (milk: string ) : Result<Milk, OrderError> =
      match milk.ToLower() with
        | "oat" -> Ok Oat
        | "almond" -> Ok Almond
        | "whole" -> Ok Whole
        | other -> Error (InvalidMilk other)
        
    let cupSize (s: string) : Result<CupSize, OrderError> =
        match s.ToLower() with
        | "small" -> Ok Small
        | "medium" -> Ok Medium
        | "large" -> Ok Large
        | other -> Error (InvalidSize other)
    let quantity (n: int) : Result<Quantity, OrderError> =
        if n > 0 then Ok (Quantity n)
        else Error (InvalidQuantity n)
    
    let drink (name: string) (milk: string option) : Result<Drink, OrderError> =
        match name with
        | "americano" -> Ok Americano
        | "expresso" -> Ok Espresso
        | "latte" | "cappuccino" ->
            match milk with
            | Some m -> helperParserMilk m |> Result.map (fun mk ->
                if name = "latte" then Latte mk else Cappuccino mk)
            | None -> Error (MilkRequired name)
        | other -> Error (InvalidDrink other)


// --- Step 4: Error accumulation ---
// The pipeline in Session 1 stopped at the first error.
// Now collect ALL validation errors at once.

type ValidationError = ValidationError of OrderError list

module Validation =
    // val map2 : ('a -> 'b -> 'c) -> Result<'a, OrderError list> -> Result<'b, OrderError list> -> Result<'c, OrderError list>
    let map2 (f: 'a -> 'b -> 'c) (r1: Result<'a, OrderError list>) (r2: Result<'b, OrderError list>) : Result<'c, OrderError list> =

        match r1, r2 with
        | Ok v1, Ok v2 -> Ok (f v1 v2)
        | Error e1, Ok _ -> Error e1    
        | Error e1, Error e2 -> Error (e1 @ e2)
        | Ok _, Error e2 -> Error e2
        
        

    // val liftResult : Result<'a, OrderError> -> Result<'a, OrderError list>
    let liftResult (r: Result<'a, OrderError>) : Result<'a, OrderError list> =
        match r with
        | Ok v -> Ok v
        | Error e -> Error [e]
        
        

    // val validateOrder : RawOrder -> Result<Order, OrderError list>
    let validateOrder (raw: RawOrder) : Result<Order, OrderError list> =
        // Hint: use liftResult on each field validator,
        // then combine with map2 (or a custom applicative)
        let emailResult = liftResult (Validate.email raw.Email)
        let drinkResult = liftResult (Validate.drink raw.DrinkName raw.Milk)
        let sizeResult = liftResult (Validate.cupSize raw.Size)
        let quantityResult = liftResult (Validate.quantity raw.Quantity)
        
        let validateOne = map2 (fun email drink -> (email,drink)) emailResult drinkResult
        let validateTwo = map2 (fun (email,drink) size -> (email, drink, size)) validateOne sizeResult
        let validateThree = map2 (fun (email, drink, size) quantity -> { Customer = email; Drink = drink; Size = size; Quantity = quantity }) validateTwo quantityResult
        validateThree


// --- Step 5: Discount engine with active patterns ---
// Active patterns let you create custom "matchers"
// that decompose data in readable ways.

// (|SmallOrder|MediumOrder|LargeOrder|)
let (|SmallOrder|MediumOrder|LargeOrder|) (order: Order) =
    match order.Size with
    | Small -> SmallOrder
    | Medium -> MediumOrder
    | Large -> LargeOrder


// (|BulkOrder|_|)
let (|BulkOrder|_|) (order: Order) =
    let (Quantity qty) = order.Quantity
    if qty >= 5 then Some BulkOrder else None

type Discount =
    | NoDiscount
    | Percentage of decimal
    | FlatOff of decimal

let calculateDiscount (order: Order) : Discount =
    // Large + Bulk -> 15% off
    // Bulk alone -> 10% off
    // Large alone -> 5% off
    // otherwise -> no discount
    match order with
    | LargeOrder & BulkOrder -> Percentage 15.0m
    | BulkOrder -> Percentage 10.0m
    | LargeOrder -> Percentage 5.0m
    | _ -> NoDiscount


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
        // calculateDiscount and the pipe operator
        let discount = calculateDiscount order
        let price = basePrice order
        applyDiscount price discount

// --- Step 7: Process a batch of orders ---
// Given a list of RawOrders, validate all of them,
// partition into successes/failures, and price the valid ones.

type ProcessedOrder = { Order: Order; Price: decimal }
type BatchResult = { Processed: ProcessedOrder list; Errors: (RawOrder * OrderError list) list }

let processBatch (rawOrders: RawOrder list) : BatchResult =
    // TODO: validate each, partition results, price valid orders
    // Hint: List.map, List.choose or List.partition, then build BatchResult
    rawOrders |> List.map (fun ro -> (ro, Validation.validateOrder ro))
    |> List.partition (fun (ro, rs)-> match rs with
        | Ok _ -> true
        | Error _ -> false)
    

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
