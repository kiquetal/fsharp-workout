# F# Workout Project

## Overview
A learning and practice project for F# functional programming, with strong emphasis on type-driven development.

## Environment
- OS: Linux
- Runtime: .NET 10
- Language: F# 10
- Paradigm: Functional-first, type-driven design
- All CLI examples must use Linux-compatible commands and `dotnet` CLI for .NET 10

## Conventions
- Prefer discriminated unions over class hierarchies
- Use `Result<'T, 'E>` over exceptions for expected failures
- Favor immutable data and pure functions
- Use single-case DUs for domain primitives (e.g., `type Email = Email of string`)
- Prefer pipeline operator (`|>`) for data transformations
- Keep modules small and focused on a single domain concept
- Use `Option` instead of null
- Prefer pattern matching over if/else chains

## Project Structure
```
src/       — F# source files
tests/     — test projects
```
