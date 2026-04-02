# Session 4 — Modules & Domain Modeling

**Goal:** Organize domain logic into modules with clear boundaries. Build a small bounded context end-to-end using everything from Sessions 1–3.

## The Domain: Library Book Lending

A library where members can borrow and return books. Simple rules:
- A book can be **Available** or **CheckedOut** (by a specific member)
- A member can borrow at most 3 books at a time
- Borrowing an already checked-out book is an error
- Returning a book that isn't checked out is an error

## Why This Domain?

- Small enough to finish in 90 minutes
- Needs multiple modules that talk to each other
- Has real business rules to encode in types
- Connects to everything you've learned: DUs for state, Result for errors, pattern matching for transitions

## Warm-up (10 min)

### Step 1: Define the domain types

Think about the entities:
- `BookId`, `MemberId` — single-case DU wrappers (Session 1 pattern)
- `Book` — has an id, title, and a status
- `BookStatus` — Available or CheckedOut (who has it?)
- `Member` — has an id, name, and list of borrowed book ids

Think about the errors:
- What can go wrong when borrowing?
- What can go wrong when returning?

### Step 2: Organize into modules

F# modules are like static classes in Java — they group related functions. The key design question: **what belongs together?**

Suggested structure:
- A module for `Book` operations (create, checkout, return)
- A module for `Member` operations (create, borrow, return)
- A module for `Library` that orchestrates both

## Main Exercise (50 min)

### Step 3: Book module

Functions:
- `create : BookId -> string -> Book`
- `checkout : MemberId -> Book -> Result<Book, LendingError>`
- `returnBook : Book -> Result<Book, LendingError>`

Rules:
- Can only checkout an Available book
- Can only return a CheckedOut book

### Step 4: Member module

Functions:
- `create : MemberId -> string -> Member`
- `canBorrow : Member -> bool` (max 3 books)
- `addBook : BookId -> Member -> Result<Member, LendingError>`
- `removeBook : BookId -> Member -> Result<Member, LendingError>`

### Step 5: Library module — orchestrate

This is where it comes together. The Library holds the state (all books, all members) and coordinates operations:

- `borrowBook : BookId -> MemberId -> Library -> Result<Library, LendingError>`
- `returnBook : BookId -> MemberId -> Library -> Result<Library, LendingError>`

Each operation must:
1. Look up the book and member
2. Validate the business rules
3. Update both book and member
4. Return the new Library state (no mutation!)

### Step 6: Stretch — query functions

- `availableBooks : Library -> Book list`
- `booksBorrowedBy : MemberId -> Library -> Book list`
- `overdueReport : Library -> string` (if you add a due date to CheckedOut)

## Key Concepts

| Concept | What it means | Where you'll see it |
|---------|--------------|---------------------|
| Module | Groups related functions | `Book.create`, `Member.canBorrow` |
| Encapsulation | Hide internals, expose API | `private` helpers inside modules |
| Bounded context | A self-contained domain model | Library = books + members + rules |
| Orchestration | One module coordinates others | `Library.borrowBook` calls Book + Member |
| Immutable state | Return new state, don't mutate | `{ library with Books = ... }` |

## Review (10 min)
- Look at your module boundaries — does each module have a single responsibility?
- Could you add a new rule (e.g., max borrow duration) without changing every module?
- Notice: the types prevent invalid states, the modules organize the logic, Result handles errors
