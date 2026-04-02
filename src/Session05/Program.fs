module OrderAnalytics

// ============================================
// SESSION 5 — Collections & Pipelines
// ============================================
// Process a list of orders to answer business
// questions. No loops, no mutation — just
// pipelines of transformations.
//
// Work through these steps:
// 1. Define the types
// 2. Build sample data
// 3. Basic aggregations
// 4. Grouping and filtering
// 5. Time-based analysis
// 6. Composition (stretch)
// ============================================


// --- Step 1: Define the types ---

type CustomerId = CustomerId of string
type ProductId = ProductId of string

type OrderLine = {
    Product: ProductId
    Quantity: int
    UnitPrice: float
}

type Order = {
    Id: string
    Customer: CustomerId
    Lines: OrderLine list
    PlacedAt: System.DateTime
}


// --- Step 2: Build sample data ---
// Create 8-10 orders with different customers, products, dates.
// This is your test dataset.

// TODO: let sampleOrders = [ ... ]


// --- Step 3: Basic aggregations ---
// val orderTotal : Order -> float
// val totalRevenue : Order list -> float
// val largestOrder : Order list -> Order option
// val averageOrderValue : Order list -> float
//
// Key functions: List.map, List.sumBy, List.maxBy, List.averageBy

// TODO: implement


// --- Step 4: Grouping and filtering ---
// val revenueByCustomer : Order list -> (CustomerId * float) list
// val highValueCustomers : float -> Order list -> CustomerId list
// val topSellingProducts : int -> Order list -> (ProductId * int) list
//
// Key functions: List.groupBy, List.sortByDescending, List.truncate, List.collect

// TODO: implement


// --- Step 5: Time-based analysis ---
// val revenueByMonth : Order list -> (int * int * float) list
// val ordersInRange : DateTime -> DateTime -> Order list -> Order list

// TODO: implement


// --- Step 6: Stretch — composition ---
// Combine functions with >> to build reusable pipelines
// Example: let monthlyReport = revenueByMonth >> List.map formatRow >> String.concat "\n"

// TODO: implement


// --- Try it out ---
[<EntryPoint>]
let main _ =
    printfn "Session 5 — Order Analytics"
    // TODO: run your functions on sampleOrders and print results
    0
