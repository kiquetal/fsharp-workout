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


type CustomizationType = EXTRA_SHOT | SYRUP of string | WHIPPED_CREAM
type Quantity = int
// --- Step 4: Order record ---
type Order =
        { Drink: Drink
          Size: CupSize
          Quantity: Quantity
          Customizations: CustomizationType list 
         }
// --- Step 5: Raw input & validation ---
// This represents unvalidated input (e.g., from a user)

type RawOrder = { DrinkName: string; Size: string; Milk: string option; Quantity: int ; Customizations: string list }
type OrderError =
    | InvalidDrink of string
    | InvalidSize of string
    | InvalidMilk of string
    | InvalidQuantity of int
    | InvalidCustomization of string
// type OrderError = ...
let parseMilk (milkStr: string option) : Result<Milk, OrderError> =
    match milkStr with
    | Some milstr -> 
        match milstr.ToLower() with
        | "oat" -> Ok OAT
        | "almond" -> Ok ALMOND
        | "whole" -> Ok WHOLE
        | _ -> Error (InvalidMilk milstr)
    | None -> Error (InvalidMilk "missing")
let validate (raw: RawOrder) : Result<Order, OrderError> =
     
     let result =
         match raw.DrinkName.ToLower() with
         | "espresso" ->
             match raw.Milk with
                | Some _ -> Error (InvalidMilk "espresso cannot have milk")
                | None -> Ok (Espresso)
         | "americano" -> Ok (Americano)
         | "latte" -> parseMilk raw.Milk |> Result.map Latte
         | "cappuccino" -> parseMilk raw.Milk |> Result.map Cappuccino
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
        
     let customizationResult =
         raw.Customizations |> List.map (function
             | "extra shot" -> Ok EXTRA_SHOT
             | "whipped cream" -> Ok WHIPPED_CREAM
             | s when s.StartsWith("syrup ") -> Ok (SYRUP (s.Substring(6)))
             | other -> Error (InvalidCustomization other))
         
     let validateAllResult =
         customizationResult |> List.fold( fun acc  cur ->
                match acc, cur with
                | Ok cs, Ok c -> Ok (c :: cs)
                | Error e, _ -> Error e
                | _, Error e -> Error e) (Ok [])
         
     match result, sizeResult, quantityResult, validateAllResult with
     | Ok drink, Ok size, Ok qty , Ok cs-> Ok { Drink = drink; Size = size; Quantity = qty; Customizations = cs }
     | Error e, _, _, _ -> Error e
     | _, Error e, _ , _ -> Error e
     | _, _, Error e, _-> Error e
     | _, _, _, Error e -> Error  e 
        
 //suppose i want a customization



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
     
        let customizationCost =
            match order.Drink with
            | Latte _ | Cappuccino _ -> 0.50M // e.g., for extra shot or syrup
            | _ -> 0.00M
        
        
            
            
        basePrice * sizeMultiplier * decimal order.Quantity 
        


// --- Try it out ---
// Uncomment and test when ready:
[<EntryPoint>]
let main _ =
     let raw = { DrinkName = "latte"; Size = "medium"; Milk = Some "oat"; Quantity = 2 ; Customizations = []}
     let badRaww = { DrinkName = "latte"; Size = "medium"; Milk = Some "oat"; Quantity = -1 ; Customizations = []}
     let badCapuccinoOrder = { DrinkName = "cappuccino"; Size = "medium"; Milk = None; Quantity = 1 ; Customizations= []}
     let allOrders = [raw; badRaww; badCapuccinoOrder]
     let results = allOrders |> List.map validate
     results |> List.iter (function
        | Ok order -> printfn "Valid order: %A, Price: $%.2f" order (price order)
        | Error e -> printfn "Invalid order: %A" e)
     0 // return an integer exit code
     
