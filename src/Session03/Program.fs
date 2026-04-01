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

// TODO: let simple = ...
// TODO: let nested = ...
// TODO: let deep = ...


// --- Step 3: Evaluate ---
// val evaluate : Expr -> Result<float, string>
//
// Number → return the value
// Operation → evaluate left, evaluate right, combine
// Division by zero → Error
//
// Hint: you already know Result.bind from Session 2

// TODO: let rec evaluate (expr: Expr) = ...


// --- Step 4: Pretty-print ---
// val format : Expr -> string
//
// Number 2 → "2"
// Add(Number 2, Number 3) → "(2 + 3)"
//
// Same recursive shape as evaluate

// TODO: let rec format (expr: Expr) = ...


// --- Step 5: Simplify ---
// val simplify : Expr -> Expr
//
// x + 0 → x       0 + x → x
// x * 1 → x       1 * x → x
// x * 0 → 0       0 * x → 0
//
// This transforms the tree, not evaluates it.
// Hint: match on the Operation AND the children together

// TODO: let rec simplify (expr: Expr) = ...


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
