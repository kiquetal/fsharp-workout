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

## How Modules Split Responsibility

```
Library.borrowBook(bookId, memberId, library)
│
├─ 1. Look up book and member
│     └─ Not found? → Error
│
├─ 2. Member.canBorrow(member)
│     └─ Already has 3 books? → Error
│
├─ 3. Book.checkout(memberId, book)
│     └─ Already checked out? → Error
│
├─ 4. Member.addBook(bookId, member)
│
└─ 5. Return updated Library (new book state + new member state)
```

Each module validates what it owns:
- `Book` module → "is this book available?"
- `Member` module → "can this member borrow more?"
- `Library` module → orchestrates both, combines results

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

## Gotchas & Insights

### Each module validates what it owns

- `Book` module → "is this book available?" (knows about `BookStatus`)
- `Member` module → "does this member already have this book?" + "can they borrow more?" (knows about `BorrowedBooks`)
- `Library` module → calls both, combines results (orchestrates)

A module never checks data it doesn't own. `Member` doesn't know about `BookStatus`. `Book` doesn't know how many books a member has. `Library` is the only one that sees both.

### Record pattern matching

You can pattern match on records by naming the fields you care about — the rest are ignored:

```fsharp
type Member = { Name: string; BorrowedBooks: BookId list }

match member with
| { Name = "Alice"; BorrowedBooks = books } -> printfn "Alice has %d books" (List.length books)
| { BorrowedBooks = [] }                    -> printfn "No books borrowed"
| { BorrowedBooks = [single] }              -> printfn "Has exactly one book"
| _                                         -> printfn "Something else"
```

Useful when matching on field values and binding at the same time. For simple field access, dot notation is cleaner:

```fsharp
// prefer this for simple reads
let canBorrow (m: Member) = List.length m.BorrowedBooks < 3

// prefer pattern match when checking shape
let describe (m: Member) =
    match m with
    | { BorrowedBooks = [] } -> "No books"
    | { BorrowedBooks = books } -> sprintf "%d books" (List.length books)
```

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
