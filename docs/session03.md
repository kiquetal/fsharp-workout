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

### `unit` is F#'s `void`

In Java, `void` means "returns nothing." In F#, `unit` is an actual value: `()`. Functions that do side effects (like printing) return `unit`.

This means you can use `fold` even for side effects — `'a` becomes `unit`:

```fsharp
fold (fun n -> printfn "mi numero es %g" n) (fun _ _ _ -> ()) myExpr
```

Each handler returns `()`. The fold walks the tree, prints at each leaf, and the operation handler discards the results.

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

## Extra Fold Exercises

### HTML Tree

```fsharp
type Html =
    | Text of string
    | Element of tag: string * children: Html list

let doc = Element("div", [
    Element("p", [Text "hello"])
    Element("p", [Text "world"])
])

let rec foldHtml handleText handleElement html =
    match html with
    | Text s -> handleText s
    | Element (tag, children) ->
        let results = List.map (fun c -> foldHtml handleText handleElement c) children
        handleElement tag results

// count text nodes
let countTexts = foldHtml (fun t -> 1) (fun tag ch -> List.sum ch)
```

### Org Chart

```fsharp
type Employee =
    | Individual of name: string * salary: int
    | Manager of name: string * salary: int * reports: Employee list

let org =
    Manager("Alice", 100000, [
        Manager("Bob", 80000, [
            Individual("Carol", 50000)
            Individual("Dave", 50000)
        ])
        Individual("Eve", 60000)
    ])
```

TODO: write `foldOrg` and use it to calculate total salary.

### The factory worker analogy for tree fold

Think of each branch node as a worker sitting at their station. They don't go into the tree — they just wait for results from below.

```
      Worker C (*)
      / \
  Worker B (+)   4
    / \
   2   3
```

1. Worker B sits at `+`. Waits for results from below.
2. Someone delivers `"2"` from the left. Someone delivers `"3"` from the right.
3. Worker B combines: `"(" + "2" + " + " + "3" + ")"` → passes `"(2 + 3)"` up.
4. Worker C sits at `*`. Waits for results from below.
5. Someone delivers `"(2 + 3)"` from the left. Someone delivers `"4"` from the right.
6. Worker C combines: `"(" + "(2 + 3)" + " * " + "4" + ")"` → done.

Each worker only sees what's handed to them — two finished results. They don't know or care how those results were made. `fold` does the delivery.

The combine function is what each worker does: take two things, produce one thing. Same function at every station, different inputs.

### Why recursive types enable recursion

The key insight: a branch case can contain more of itself. That's what makes the type recursive, and that's what makes `fold` recurse.

```fsharp
// Expr: an Operation contains Expr children — which can be more Operations
Operation(Mul,
    Operation(Add, Number 2.0, Number 3.0),   ← Expr inside Expr
    Number 4.0)

// Employee: a Manager's reports list contains Employees — which can be more Managers
Manager("Alice", 100000, [
    Manager("Bob", 80000, [                    ← Manager inside Manager
        Individual("Carol", 50000)
    ])
])

// Html: an Element's children list contains Html — which can be more Elements
Element("div", [
    Element("p", [Text "hello"])               ← Element inside Element
])
```

Every recursive type follows this pattern:
- **Leaf case** — holds plain data, no self-reference → recursion stops here
- **Branch case** — holds children of the same type → recursion goes deeper

`fold` just walks this structure. When it hits a leaf, it calls the leaf handler. When it hits a branch, it folds each child first (they might be branches too — more recursion), then passes the results to the branch handler.

The branch handler never knows about the tree. It just receives already-computed results and combines them.

### The pattern

Every fold follows the same recipe:
1. Match on the type
2. Leaf → call the leaf handler
3. Branch → fold each child first (`List.map` if many, direct call if two), then call the branch handler with the results

The handlers never recurse. `fold` does that. The handlers just combine.

## Review (10 min)
- Draw the tree for `(1 + 2) * (3 - 4)` on paper
- Trace `evaluate` through it step by step
- Note: every recursive function you wrote followed the same shape — why?
