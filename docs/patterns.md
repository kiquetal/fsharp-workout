# Patterns Worth Studying

| Pattern | What it is | When to use it |
|---------|-----------|----------------|
| **Railway-Oriented Programming** | Chain `Result` values with `bind`/`map`. Two tracks: success and error. First error derails the pipeline — no need for try/catch. | Validation, parsing, any multi-step operation that can fail |
| **Making Illegal States Unrepresentable** | Use DUs so invalid combinations can't be constructed. E.g., `Latte` carries `Milk` — you can't build a Latte without it. | Domain modeling, anywhere you'd otherwise write runtime checks |
| **Parse, Don't Validate** | Convert raw untyped data (strings, ints) into rich domain types at the boundary. Once parsed, the types guarantee correctness. | Input handling, API boundaries, deserialization |

## Teaching Checklist

When explaining a new F# concept, always cover these aspects — don't skip any:

1. **What is it?** — one sentence definition
2. **What are ALL the features?** — list every variant/capability (e.g., total, partial, AND parameterized active patterns)
3. **How does it get called?** — show the explicit invocation, not just the sugar
4. **What are the parameters?** — explain every parameter, especially implicit ones (e.g., `match` passes the last argument automatically)
5. **What does it return?** — show the return type and what each case produces
6. **How does it compose?** — how does it connect with other concepts already learned (pipes, Result, map, bind)
7. **Java 21 equivalent** — show the closest Java translation for comparison