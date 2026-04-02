module Library

// ============================================
// SESSION 4 — Modules & Domain Modeling
// ============================================
// Build a library book lending system.
// Organize into modules with clear boundaries.
// Tie together: types as design, Result pipelines,
// pattern matching, and now module encapsulation.
//
// Work through these steps:
// 1. Define domain types
// 2. Build the Book module
// 3. Build the Member module
// 4. Build the Library module (orchestration)
// 5. Query functions (stretch)
// ============================================


// --- Step 1: Define domain types ---
// BookId, MemberId — single-case DU wrappers (Session 1)
// BookStatus — Available or CheckedOut of MemberId
// Book — id, title, status
// Member — id, name, borrowed book ids
// LendingError — what can go wrong?


type BookId = BookId of string
type MemberId = MemberId of string
type BookStatus = Available | CheckedOut of MemberId

type Book =
    {
        Id: BookId
        Title: string
        Status: BookStatus
        
    }

type Memmber = {
    Id: MemberId
    Name: string
    BorrowedBooks: BookId list
}

type LendingError =
    | BookNotFound of BookId
    | MemberNotFound of MemberId
    | BookAlreadyCheckedOut of BookId
    | MemberCannotBorrow of MemberId
    | BookNotCheckedOut of BookId
    | BookCheckedOutByAnotherMember of BookId * MemberId


// --- Step 2: Book module ---
// Group book-related operations together.
//
// val create : BookId -> string -> Book
// val checkout : MemberId -> Book -> Result<Book, LendingError>
// val returnBook : Book -> Result<Book, LendingError>

module Book =
    let create (id: BookId) (title: string) : Book =
        { Id = id; Title = title; Status = Available }
        
    let checkout (memberId: MemberId) (book: Book) =
        match book.Status with
        | Available -> Ok { book with Status = CheckedOut memberId }
        | CheckedOut _ -> Error (BookAlreadyCheckedOut book.Id)

    let returnBook (b: Book) =
        match b.Status with
        | Available -> Error (BookNotCheckedOut b.Id)
        | CheckedOut _ -> Ok { b with Status = Available }
// --- Step 3: Member module ---
// Group member-related operations together.
//
// val create : MemberId -> string -> Member
// val canBorrow : Member -> bool
// val addBook : BookId -> Member -> Result<Member, LendingError>
// val removeBook : BookId -> Member -> Result<Member, LendingError>

// TODO: module Member = ...


// --- Step 4: Library module (orchestration) ---
// Holds all books and members. Coordinates operations.
//
// type Library = { Books: Map<BookId, Book>; Members: Map<MemberId, Member> }
//
// val borrowBook : BookId -> MemberId -> Library -> Result<Library, LendingError>
// val returnBook : BookId -> MemberId -> Library -> Result<Library, LendingError>
//
// Each operation: look up → validate → update → return new state

// TODO: module LibraryOps = ...


// --- Step 5: Query functions (stretch) ---
// val availableBooks : Library -> Book list
// val booksBorrowedBy : MemberId -> Library -> Book list

// TODO: add to LibraryOps module


// --- Try it out ---
[<EntryPoint>]
let main _ =
    // TODO: create a library with some books and members
    // TODO: borrow some books, return some, print the state
    printfn "Session 4 — Library Book Lending"
    0
