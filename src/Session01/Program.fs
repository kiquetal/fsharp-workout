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
// type CupSize = ...

// --- Step 2: Milk options ---
// type Milk = ...

// --- Step 3: Drinks ---
// Think: which drink MUST carry milk data?
// type Drink = ...

// --- Step 4: Order record ---
// type Order = ...

// --- Step 5: Raw input & validation ---
// This represents unvalidated input (e.g., from a user)
// type RawOrder = { DrinkName: string; Size: string; Milk: string option; Quantity: int }
// type OrderError = ...
// let validate (raw: RawOrder) : Result<Order, OrderError> = ...

// --- Step 6: Pricing ---
// let price (order: Order) : decimal = ...

// --- Try it out ---
// Uncomment and test when ready:
// [<EntryPoint>]
// let main _ =
//     let raw = { DrinkName = "latte"; Size = "medium"; Milk = Some "oat"; Quantity = 2 }
//     match validate raw with
//     | Ok order -> printfn "Order total: $%M" (price order)
//     | Error err -> printfn "Invalid order: %A" err
//     0
