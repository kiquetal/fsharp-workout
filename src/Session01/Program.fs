module CoffeeOrder

// ============================================
// SESSION 1 — Types as Design
// ============================================
// Your job: model a coffee order system where
// invalid states are impossible to construct.
//
// Work through these steps:
// 1. Define CupSize
// 2. Define Milk
// 3. Define Drink (hint: Latte REQUIRES milk)
// 4. Define Order
// 5. Write validate: RawOrder -> Result<Order, OrderError>
// 6. Write price: Order -> decimal
// ============================================

// --- Step 1: Cup sizes ---
type CupSize = Small | Medium | Large
 
// --- Step 2: Milk options ---
type Milk =  OAT | ALMOND | WHOLE
 
// --- Step 3: Drinks ---
// Think: which drink MUST carry milk data?
type Drink =
        | Espresso
        | Americano
        | Latte of Milk
        | Cappuccino of Milk
        
// --- Step 4: Order record ---
type Order =
        { Drink: Drink
          Size: CupSize
          Quantity: int }
// --- Step 5: Raw input & validation ---
// This represents unvalidated input (e.g., from a user)
type RawOrder = { DrinkName: string; Size: string; Milk: string option; Quantity: int }

type OrderError =
    | InvalidDrink of string
    | InvalidSize of string
    | InvalidMilk of string
    | InvalidQuantity of int
// type OrderError = ...
let validate (raw: RawOrder) : Result<Order, OrderError> =
     
     let result =
         match raw.DrinkName.ToLower() with
         | "espresso" ->
             match raw.Milk with
                | Some _ -> Error (InvalidMilk "espresso cannot have milk")
                | None -> Ok (Espresso)
         | "americano" -> Ok (Americano)
         | "latte" ->
             match raw.Milk with
             | Some milstr -> 
                 match milstr.ToLower() with
                 | "oat" -> Ok (Latte OAT)
                 | "almond" -> Ok (Latte ALMOND)
                 | "whole" -> Ok (Latte WHOLE)
                 | _ -> Error (InvalidMilk milstr)
             | None -> Error (InvalidMilk "missing")
         | "cappuccino" ->
                match raw.Milk with
                | Some milstr -> 
                    match milstr.ToLower() with
                    | "oat" -> Ok (Cappuccino OAT)
                    | "almond" -> Ok (Cappuccino ALMOND)
                    | "whole" -> Ok (Cappuccino WHOLE)
                    | _ -> Error (InvalidMilk milstr)
                | None -> Error (InvalidMilk "missing")
         | other -> Error (InvalidDrink other)
         
     let sizeResult = 
        match raw.Size.ToLower() with
        | "small" -> Ok Small
        | "medium" -> Ok Medium
        | "large" -> Ok Large
        | other -> Error (InvalidSize other)
     let quantityResult =
        if raw.Quantity > 0 then Ok raw.Quantity
            else Error (InvalidQuantity raw.Quantity)
        
     match result, sizeResult, quantityResult with
     | Ok drink, Ok size, Ok qty -> Ok { Drink = drink; Size = size; Quantity = qty }
     | Error e, _, _ -> Error e
     | _, Error e, _ -> Error e
     | _, _, Error e -> Error e
        
// --- Step 6: Pricing ---
let price (order: Order) : decimal =
        let basePrice =
            match order.Drink with
            | Espresso -> 2.00M
            | Americano -> 2.50M
            | Latte _ -> 3.50M
            | Cappuccino _ -> 3.75M
        let sizeMultiplier =
            match order.Size with
            | Small -> 1.0M
            | Medium -> 1.2M
            | Large -> 1.5M
     
        basePrice * sizeMultiplier * decimal order.Quantity
     
        

// --- Try it out ---
// Uncomment and test when ready:
[<EntryPoint>]
let main _ =
     let raw = { DrinkName = "latte"; Size = "medium"; Milk = Some "oat"; Quantity = 2 }
     match validate raw with
     | Ok order -> printfn "Order total: $%M" (price order)
     | Error err -> printfn "Invalid order: %A" err
     0
