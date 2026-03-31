# Applying F# Patterns in Java 21+

Java 21+ has no built-in `Result` type. Three ways to get the same pattern:

**1. Roll your own with sealed interfaces (no dependencies):**
```java
sealed interface Result<T, E> permits Ok, Err {}
record Ok<T, E>(T value) implements Result<T, E> {}
record Err<T, E>(E error) implements Result<T, E> {}

// smart constructor
static Result<Quantity, String> create(int n) {
    return n > 0 ? new Ok<>(new Quantity(n)) : new Err<>("Invalid: " + n);
}

// caller must handle both — switch expression
switch (create(rawQty)) {
    case Ok(var qty) -> process(qty);
    case Err(var e) -> log(e);
}
```

**2. Vavr `Either<L, R>` (popular library):**
```java
Either<String, Quantity> create(int n) {
    return n > 0 ? Either.right(new Quantity(n)) : Either.left("Invalid: " + n);
}
```

**3. `Optional` (built-in, but no error details):**
```java
Optional<Quantity> create(int n) {
    return n > 0 ? Optional.of(new Quantity(n)) : Optional.empty();
}
```

| F# | Java 21+ equivalent |
|---|---|
| `Result<'T, 'E>` | sealed `Result<T, E>` or Vavr `Either` |
| `Option<'T>` | `Optional<T>` |
| Discriminated unions | `sealed interface` + `record` |
| Smart constructor (`private` DU) | static factory + private class constructor |
| Pattern matching | `switch` expressions with `case record(var x)` |

## Functional Chaining: `map` vs `flatMap`

**One sentence to remember:** `map` wraps for you, `flatMap` trusts that your function already did.

```java
// map: transform the value inside the container
Optional.of("hello").map(s -> s.length());          // Optional<Integer>

// flatMap: your function already returns a container — don't double-wrap
Optional.of("hello").flatMap(s -> Optional.of(s.length())); // Optional<Integer>

// what happens if you use map when the function returns Optional?
Optional.of("hello").map(s -> Optional.of(s.length()));     // Optional<Optional<Integer>> — broken
```

| F# | Java | Does what |
|---|---|---|
| `Result.map` | `.map()` | Transforms inner value, re-wraps |
| `Result.bind` | `.flatMap()` | Function already wraps, no double-wrapping |
| `Result.map2` | nested `flatMap`/`map` | Combines two containers |

## Combining Multiple `Optional` Values

F# `map2` has no Java equivalent — you nest `flatMap` calls, using `map` for the last step:

```java
// F# version — flat, clean
Result.map2 (fun email size -> new Order(email, size)) emailResult sizeResult

// Java equivalent — nested, but same logic
emailOpt.flatMap(email ->           // unwrap email, don't re-wrap (inner returns Optional)
    sizeOpt.map(size ->             // unwrap size, DO re-wrap (inner returns plain value)
        new Order(email, size)));   // plain value — map wraps it
```

With three fields:

```java
emailOpt.flatMap(email ->
    sizeOpt.flatMap(size ->         // flatMap: intermediate step, more chaining ahead
        qtyOpt.map(qty ->          // map: last step, just build the value
            new Order(email, size, qty))));
```

**Rule:** `flatMap` for all intermediate steps, `map` for the last one.

## Result with `map`/`flatMap` Chaining (Railway-Oriented Programming)

The sealed `Result` above only has `switch`. Add `map` and `flatMap` to get railway-style chaining — first error short-circuits the entire pipeline:

```java
public sealed interface Result<T, E> {
    record Ok<T, E>(T value) implements Result<T, E> {}
    record Err<T, E>(E error) implements Result<T, E> {}

    default <U> Result<U, E> map(Function<T, U> f) {
        return switch (this) {
            case Ok<T, E> ok -> new Ok<>(f.apply(ok.value()));
            case Err<T, E> err -> new Err<>(err.error());
        };
    }

    default <U> Result<U, E> flatMap(Function<T, Result<U, E>> f) {
        return switch (this) {
            case Ok<T, E> ok -> f.apply(ok.value());
            case Err<T, E> err -> new Err<>(err.error());
        };
    }
}
```

**Short-circuit in action — first error wins:**

```java
// validate returns Result<Integer, String>
// transform returns Result<Integer, String>

new Ok<>(5)
    .flatMap(x -> validate(x))    // Err("too small") — stops here
    .flatMap(x -> transform(x))   // never runs
    .map(x -> x * 2);             // never runs
// result: Err("too small")
```

**Same railroad in F#:**

```fsharp
Ok 5
|> Result.bind validate      // Error "too small" — stops here
|> Result.bind transform      // never runs
|> Result.map (fun x -> x * 2) // never runs
// result: Error "too small"
```

**Rule:** `Err` flows through every `map`/`flatMap` untouched. Your functions only run on `Ok`.

## Applicative Validation: `map2` with Error Accumulation

`flatMap` stops at the first error. `map2` checks both sides independently and collects ALL errors:

```java
// Add to your Result interface — errors are List<E> so they can accumulate
static <A, B, C, E> Result<C, List<E>> map2(
        BiFunction<A, B, C> f,
        Result<A, List<E>> r1,
        Result<B, List<E>> r2) {

    // Java can't match two values at once like F#'s "match r1, r2 with"
    // So we nest: outer switch checks r1, inner switch checks r2

    return switch (r1) {
        case Ok<A, List<E>> ok1 -> switch (r2) {
            case Ok<B, List<E>> ok2 ->
                new Ok<>(f.apply(ok1.value(), ok2.value()));  // both Ok → run f
            case Err<B, List<E>> err2 ->
                new Err<>(err2.error());                      // only r2 failed
        };
        case Err<A, List<E>> err1 -> switch (r2) {
            case Ok<B, List<E>> ok2 ->
                new Err<>(err1.error());                      // only r1 failed
            case Err<B, List<E>> err2 -> {
                // both failed → join the two error lists (F#'s e1 @ e2)
                var combined = new ArrayList<>(err1.error()); // copy first list
                combined.addAll(err2.error());                // add second list
                yield new Err<>(List.copyOf(combined));       // immutable result
            }
        };
    };
}
```

**Smart constructors create the list — `map2` just merges them:**

```java
static Result<Email, List<String>> validateEmail(String s) {
    return s.contains("@") ? new Ok<>(new Email(s)) : new Err<>(List.of("Invalid email: " + s));
}

static Result<Quantity, List<String>> validateQty(int n) {
    return n > 0 ? new Ok<>(new Quantity(n)) : new Err<>(List.of("Invalid quantity: " + n));
}
```

**Usage — both errors collected:**

```java
var result = Result.map2(
    Order::new,                    // buildOrder — only runs if both Ok
    validateEmail(""),             // Err(["Invalid email: "])
    validateQty(-1)                // Err(["Invalid quantity: -1"])
);
// result: Err(["Invalid email: ", "Invalid quantity: -1"])
```

**Same thing in F#:**

```fsharp
Validation.map2 buildOrder
    (validateEmail "")             // Error [InvalidEmail ""]
    (validateQty -1)               // Error [InvalidQuantity -1]
// result: Error [InvalidEmail ""; InvalidQuantity -1]
```

**`flatMap` vs `map2`:**
- `flatMap` — fail fast, first error stops the chain
- `map2` — accumulate, check everything, merge all errors

## Sealed Interfaces as Discriminated Unions

Java 21 sealed interfaces + records give you the same exhaustive matching as F# DUs:

```java
// F#:  type Shape = Circle of float | Rectangle of float * float
sealed interface Shape permits Circle, Rectangle {}
record Circle(double radius) implements Shape {}
record Rectangle(double w, double h) implements Shape {}

// F#:  match shape with | Circle r -> ... | Rectangle (w, h) -> ...
double area(Shape shape) {
    return switch (shape) {
        case Circle(var r) -> Math.PI * r * r;
        case Rectangle(var w, var h) -> w * h;
        // no default needed — compiler knows all cases
    };
}
```

## Smart Constructors in Java

Same pattern as F# `private` DU — hide the constructor, expose a factory:

```java
// F#:  type Email = private Email of string
//      module Email = let create s = if valid then Ok (Email s) else Error ...
public record Email(String value) {
    // compact constructor — validates on creation
    public Email {
        if (value == null || !value.contains("@"))
            throw new IllegalArgumentException("Invalid email: " + value);
    }

    // or: factory method returning Optional (no exceptions)
    public static Optional<Email> create(String s) {
        return (s != null && s.contains("@"))
            ? Optional.of(new Email(s))
            : Optional.empty();
    }
}
```

## Pipelines with `Stream`

F# `|>` pipelines translate to Java `Stream` chains:

```java
// F#:  orders |> List.filter isValid |> List.map price |> List.sum
var total = orders.stream()
    .filter(Order::isValid)
    .map(Order::price)
    .reduce(BigDecimal.ZERO, BigDecimal::add);
```
