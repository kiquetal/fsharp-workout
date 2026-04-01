# Session 3 — Pattern Matching & Recursion

**Goal:** Model recursive data structures with DUs, process them with recursive functions, and understand how pattern matching drives the recursion.

## The Domain: Arithmetic Expressions

You already think in expressions every day:

```
(2 + 3) * 4 = 20
```

This is secretly a tree:

```
      (*)
      / \
    (+)   4
    / \
   2   3
```

Each node is either:
- A **number** (leaf — no children)
- An **operation** (branch — two children: left and right)

This is a recursive data structure — an operation contains expressions, which can themselves be operations.

## Why This Domain?

- You can draw it on paper — every expression is a tree
- DUs model it perfectly — `Number` or `Operation` (same as `Ok` or `Error`)
- Recursion is the only natural way to process it — you can't avoid it
- It connects to everything you've learned: DUs, pattern matching, `Result` for division by zero

## Warm-up (10 min)

### Step 1: Define the types

Think about what an expression can be:
- A literal number (leaf)
- An operation combining two expressions (branch)

What operations? Start with `Add`, `Sub`, `Mul`, `Div`.

Two types needed:
- One for the operator (which operation)
- One for the expression (recursive — an expression can contain expressions)

### Step 2: Build expressions by hand

Before writing any functions, construct a few expressions as values:

```
Simple:     2 + 3
Nested:     (2 + 3) * 4
Deep:       (10 / 2) - (1 + 2)
```

If you can build them, you understand the type.

## Main Exercise (50 min)

### Step 3: Evaluate

Write a function `evaluate : Expr -> Result<float, ExprError>`.

- `Number` → just return the value
- `Operation` → evaluate left, evaluate right, combine with the operator
- Division by zero → return `Error`

This is your first recursive function. The pattern:
1. Match on the expression
2. Base case: `Number` → return immediately
3. Recursive case: `Operation` → evaluate children first, then combine

### Step 4: Pretty-print

Write `format : Expr -> string` that turns an expression back into readable text:

```
Add(Number 2, Number 3)  →  "(2 + 3)"
Mul(Add(Number 2, Number 3), Number 4)  →  "((2 + 3) * 4)"
```

Same recursive structure as `evaluate` — match, base case, recursive case.

### Step 5: Transform

Write `simplify : Expr -> Expr` that applies algebraic rules:
- `x + 0` → `x`
- `0 + x` → `x`
- `x * 1` → `x`
- `1 * x` → `x`
- `x * 0` → `0`
- `0 * x` → `0`

This returns an `Expr`, not a value — you're transforming the tree, not evaluating it.

### Step 6: Fold (stretch)

Notice that `evaluate`, `format`, and `simplify` all follow the same pattern:
1. Match on `Number` → do something
2. Match on `Operation` → recurse on children, combine results

Extract this into a generic `fold`:

```
fold : (float -> 'a) -> (Operator -> 'a -> 'a -> 'a) -> Expr -> 'a
```

Then rewrite `evaluate` and `format` using `fold`.

## Gotchas & Insights

### `Result.bind` is Java's `flatMap`

| F# | Java | When to use |
|-----|------|------------|
| `Result.map` | `.map()` | Next step can't fail — returns a plain value |
| `Result.bind` | `.flatMap()` | Next step can fail — returns a `Result` |

`bind` = "map then flatten." Same concept, different naming tradition.

### The `*` syntax in DU cases is not a tuple

When you see `*` in a DU case, it means "this case carries multiple pieces of data." It's just F#'s syntax for "and."

```fsharp
| Latte of Milk                         // 1 value
| Operation of Operator * Expr * Expr   // 3 values
```

Construct: `Operation(Add, Number 2.0, Number 3.0)`
Decompose: `| Operation(op, left, right) -> ...`

## Key Concepts

| Concept | What it means | Where you'll see it |
|---------|--------------|---------------------|
| Recursive DU | A type that references itself | `Expr` contains `Expr` |
| Base case | The non-recursive branch | `Number` — just a value, no children |
| Recursive case | The branch that calls itself | `Operation` — evaluate children first |
| Structural recursion | Follow the shape of the data | One match branch per DU case |
| Catamorphism (fold) | Generic recursion pattern | Replace constructors with functions |

## Understanding Fold — A Tree Walker

`fold` is just the recursion skeleton extracted into a reusable function. You pass in two handlers:

```fsharp
let rec fold handleNumber handleOperation expr = ...
```

- `handleNumber` — "what do I do when I hit a `Number`?"
- `handleOperation` — "what do I do when I have an operator and two already-processed children?"

### Trace: formatting `(2 + 3)`

```
fold handleNumber handleOperation (Operation(Add, Number 2.0, Number 3.0))

1. Matches Operation → recurse on children
2. fold ... (Number 2.0) → matches Number → handleNumber 2.0 → "2"
3. fold ... (Number 3.0) → matches Number → handleNumber 3.0 → "3"
4. handleOperation Add "2" "3" → "(2 + 3)"
```

### What changes between use cases is only the two handlers

| Use case | handleNumber | handleOperation |
|----------|-------------|-----------------|
| format | `fun n -> n.ToString()` | `fun op l r -> "(" + l + symbol + r + ")"` |
| evaluate | `fun n -> Ok n` | `fun op l r -> (* bind + math *)` |
| simplify | `fun n -> Number n` | `fun op l r -> (* check rules, return Expr *)` |

The recursion is always the same. Only the "what to do" changes.

## Review (10 min)
- Draw the tree for `(1 + 2) * (3 - 4)` on paper
- Trace `evaluate` through it step by step
- Note: every recursive function you wrote followed the same shape — why?
