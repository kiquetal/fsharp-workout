module Session04.Tests

open Xunit
open Library

// --- Helpers ---
let bookId1 = BookId "b1"
let bookId2 = BookId "b2"
let bookId3 = BookId "b3"
let bookId4 = BookId "b4"
let memberId1 = MemberId "m1"
let memberId2 = MemberId "m2"

let mkLibrary books members : Library.Library =
    { Books = books |> List.map (fun (b: Book) -> b.Id, b) |> Map.ofList
      Members = members |> List.map (fun (m: Member) -> m.Id, m) |> Map.ofList }

// --- Book module tests ---

[<Fact>]
let ``Book.create returns Available book`` () =
    let book = Book.create bookId1 "The Hobbit"
    Assert.Equal(Available, book.Status)
    Assert.Equal("The Hobbit", book.Title)

[<Fact>]
let ``Book.checkout succeeds for Available book`` () =
    let book = Book.create bookId1 "The Hobbit"
    let result = Book.checkout memberId1 book
    match result with
    | Ok b -> Assert.Equal(CheckedOut memberId1, b.Status)
    | Error _ -> Assert.Fail "Expected Ok"

[<Fact>]
let ``Book.checkout fails for already checked out book`` () =
    let book = { Book.create bookId1 "The Hobbit" with Status = CheckedOut memberId1 }
    let result = Book.checkout memberId2 book
    match result with
    | Error (BookAlreadyCheckedOut id) -> Assert.Equal(bookId1, id)
    | _ -> Assert.Fail "Expected BookAlreadyCheckedOut"

[<Fact>]
let ``Book.returnBook succeeds for checked out book`` () =
    let book = { Book.create bookId1 "The Hobbit" with Status = CheckedOut memberId1 }
    let result = Book.returnBook book
    match result with
    | Ok b -> Assert.Equal(Available, b.Status)
    | Error _ -> Assert.Fail "Expected Ok"

[<Fact>]
let ``Book.returnBook fails for available book`` () =
    let book = Book.create bookId1 "The Hobbit"
    let result = Book.returnBook book
    match result with
    | Error (BookNotCheckedOut id) -> Assert.Equal(bookId1, id)
    | _ -> Assert.Fail "Expected BookNotCheckedOut"

// --- Member module tests ---

[<Fact>]
let ``Member.create starts with empty borrowed list`` () =
    let m = Member.create memberId1 "Alice"
    Assert.Empty(m.BorrowedBooks)

[<Fact>]
let ``Member.canBorrow is true when under limit`` () =
    let m = Member.create memberId1 "Alice"
    Assert.True(Member.canBorrow m)

[<Fact>]
let ``Member.canBorrow is false at limit`` () =
    let m = { Member.create memberId1 "Alice" with BorrowedBooks = [bookId1; bookId2; bookId3] }
    Assert.False(Member.canBorrow m)

[<Fact>]
let ``Member.addBook succeeds when under limit`` () =
    let m = Member.create memberId1 "Alice"
    let result = Member.addBook bookId1 m
    match result with
    | Ok updated -> Assert.Contains(bookId1, updated.BorrowedBooks)
    | Error _ -> Assert.Fail "Expected Ok"

[<Fact>]
let ``Member.addBook fails when at limit`` () =
    let m = { Member.create memberId1 "Alice" with BorrowedBooks = [bookId1; bookId2; bookId3] }
    let result = Member.addBook bookId4 m
    match result with
    | Error (MemberCannotBorrow _) -> ()
    | _ -> Assert.Fail "Expected MemberCannotBorrow"

[<Fact>]
let ``Member.addBook fails for duplicate book`` () =
    let m = { Member.create memberId1 "Alice" with BorrowedBooks = [bookId1] }
    let result = Member.addBook bookId1 m
    match result with
    | Error _ -> ()
    | Ok _ -> Assert.Fail "Expected error for duplicate"

// --- Library orchestration tests ---

[<Fact>]
let ``Library.borrowBook succeeds for valid borrow`` () =
    let lib = mkLibrary [Book.create bookId1 "The Hobbit"] [Member.create memberId1 "Alice"]
    let result = Library.borrowBook bookId1 memberId1 lib
    match result with
    | Ok updated ->
        let book = Map.find bookId1 updated.Books
        let m = Map.find memberId1 updated.Members
        Assert.Equal(CheckedOut memberId1, book.Status)
        Assert.Contains(bookId1, m.BorrowedBooks)
    | Error e -> Assert.Fail $"Expected Ok, got {e}"

[<Fact>]
let ``Library.borrowBook fails for nonexistent book`` () =
    let lib = mkLibrary [] [Member.create memberId1 "Alice"]
    let result = Library.borrowBook bookId1 memberId1 lib
    match result with
    | Error (BookNotFound _) -> ()
    | _ -> Assert.Fail "Expected BookNotFound"

[<Fact>]
let ``Library.borrowBook fails for nonexistent member`` () =
    let lib = mkLibrary [Book.create bookId1 "The Hobbit"] []
    let result = Library.borrowBook bookId1 memberId1 lib
    match result with
    | Error (MemberNotFound _) -> ()
    | _ -> Assert.Fail "Expected MemberNotFound"

[<Fact>]
let ``Library.borrowBook fails for already checked out book`` () =
    let book = { Book.create bookId1 "The Hobbit" with Status = CheckedOut memberId2 }
    let lib = mkLibrary [book] [Member.create memberId1 "Alice"]
    let result = Library.borrowBook bookId1 memberId1 lib
    match result with
    | Error (BookAlreadyCheckedOut _) -> ()
    | _ -> Assert.Fail "Expected BookAlreadyCheckedOut"

[<Fact>]
let ``Library.borrowBook fails when member at borrow limit`` () =
    let m = { Member.create memberId1 "Alice" with BorrowedBooks = [bookId1; bookId2; bookId3] }
    let lib = mkLibrary [Book.create bookId4 "New Book"] [m]
    let result = Library.borrowBook bookId4 memberId1 lib
    match result with
    | Error (MemberCannotBorrow _) -> ()
    | _ -> Assert.Fail "Expected MemberCannotBorrow"

// --- Query tests ---

[<Fact>]
let ``availableBooks returns only available books`` () =
    let b1 = Book.create bookId1 "Available Book"
    let b2 = { Book.create bookId2 "Taken Book" with Status = CheckedOut memberId1 }
    let lib = mkLibrary [b1; b2] []
    let available = Library.availableBooks lib
    Assert.Equal(1, List.length available)
    Assert.Equal(bookId1, available.[0].Id)

[<Fact>]
let ``bookBoorow returns books borrowed by member`` () =
    let b1 = { Book.create bookId1 "Book 1" with Status = CheckedOut memberId1 }
    let m = { Member.create memberId1 "Alice" with BorrowedBooks = [bookId1] }
    let lib = mkLibrary [b1] [m]
    let borrowed = Library.bookBoorow lib memberId1
    Assert.Equal(1, List.length borrowed)
    Assert.Equal(bookId1, borrowed.[0].Id)

[<Fact>]
let ``bookBoorow returns empty for unknown member`` () =
    let lib = mkLibrary [] []
    let borrowed = Library.bookBoorow lib memberId1
    Assert.Empty(borrowed)
