module ExpressionEngine

// ============================================
// SESSION 3 — Pattern Matching & Recursion
// ============================================
// Build an arithmetic expression evaluator.
// Expressions are trees — recursion is the
// only natural way to process them.
//
// Work through these steps:
// 1. Define the types (Operator, Expr)
// 2. Build expressions by hand
// 3. Evaluate expressions (recursive)
// 4. Pretty-print expressions (recursive)
// 5. Simplify expressions (recursive)
// 6. Extract a generic fold (stretch)
// ============================================


// --- Step 1: Define the types ---
// An expression is either:
//   - A number (leaf)
//   - An operation on two expressions (branch)
//
// Think: what does the Operator type look like?
//        what does the Expr type look like?
//        (hint: Expr is recursive — it references itself)


type Operator = Add | Subtract | Multiply | Divide
type Expr =
    | Number of float
    | Operation of Operator * Expr * Expr

// --- Step 2: Build expressions by hand ---
// Construct these as values before writing any functions:
//   simple:  2 + 3
//   nested:  (2 + 3) * 4
//   deep:    (10 / 2) - (1 + 2)
//
// If you can build them, you understand the type.

let simple = Operation (Add, Number 2.0, Number 3.0)
let nested = Operation(Multiply , simple, Number 4.0)
let deep = Operation(Subtract, Operation(Divide, Number 10.0, Number 2.0), Operation(Add, Number 1.0, Number 2.0))
// --- Step 3: Evaluate ---
// val evaluate : Expr -> Result<float, string>
//
// Number → return the value
// Operation → evaluate left, evaluate right, combine
// Division by zero → Error
//
// Hint: you already know Result.bind from Session 2

let rec evaluate expr : Result<float,string> =
    match expr with
    | Number n -> Ok n
    | Operation (op, left, right) ->  evaluate left |> Result.bind ( fun l ->
             evaluate right |> Result.bind ( fun  r ->
                match op with
                | Add -> Ok (l + r)
                | Subtract -> Ok (l - r)
                | Multiply -> Ok (l * r)
                | Divide -> if r = 0.0 then Error "Division by zero" else Ok (l / r)
             )
             
       )
    

// --- Step 4: Pretty-print ---
// val format : Expr -> string
//
// Number 2 → "2"
// Add(Number 2, Number 3) → "(2 + 3)"
//
// Same recursive shape as evaluate

let rec format (expr: Expr) : string =
    match expr with
    | Number n -> n.ToString()
    | Operation(operator, expr, expr1) ->
      let optStr =
                        match operator with
                        | Add -> " + "
                        | Subtract -> " - "
                        | Multiply -> " * "
                        | Divide -> " / "
      "(" + format expr + optStr + format expr1 + ")"

// --- Step 5: Simplify ---
// val simplify : Expr -> Expr
//
// x + 0 → x       0 + x → x
// x * 1 → x       1 * x → x
// x * 0 → 0       0 * x → 0
//
// This transforms the tree, not evaluates it.
// Hint: match on the Operation AND the children together

let rec simplify (expr: Expr) : Expr =
    match expr with
    | Number e -> Number e
    | Operation (op, left, right) ->
      let leftSimp = simplify left
      let rightSimp = simplify right
      match (op, leftSimp, rightSimp) with
      | (Add, _, Number 0.0) -> leftSimp
      | (Add, Number 0.0, _) -> rightSimp
      | (Multiply, _, Number 1.0) -> leftSimp
      | (Multiply, Number 1.0, _) -> rightSimp
      | (Multiply, _, Number 0.0) -> Number 0.0
      | (Multiply, Number 0.0, _) -> Number 0.0
      | _ -> Operation (op, leftSimp, rightSimp)
            


// --- Step 6: Fold (stretch) ---
// Notice evaluate, format, simplify all follow the same pattern.
// Extract it into a generic fold:
//
// val fold : (float -> 'a) -> (Operator -> 'a -> 'a -> 'a) -> Expr -> 'a

// TODO: let rec fold fNumber fOp expr = ...


// --- Try it out ---
[<EntryPoint>]
let main _ =
    // TODO: build expressions, evaluate, format, simplify
    // printfn "Expression: %s" (format nested)
    // printfn "Result: %A" (evaluate nested)
    0
