# Patterns Worth Studying

| Pattern | What it is | When to use it |
|---------|-----------|----------------|
| **Railway-Oriented Programming** | Chain `Result` values with `bind`/`map`. Two tracks: success and error. First error derails the pipeline — no need for try/catch. | Validation, parsing, any multi-step operation that can fail |
| **Making Illegal States Unrepresentable** | Use DUs so invalid combinations can't be constructed. E.g., `Latte` carries `Milk` — you can't build a Latte without it. | Domain modeling, anywhere you'd otherwise write runtime checks |
| **Parse, Don't Validate** | Convert raw untyped data (strings, ints) into rich domain types at the boundary. Once parsed, the types guarantee correctness. | Input handling, API boundaries, deserialization |
