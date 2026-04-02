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

let sampleOrders = [{
    Id = "order1"
    Customer = CustomerId "customer1"
    Lines = [
        { Product = ProductId "productA"; Quantity = 2; UnitPrice = 10.0 }
        { Product = ProductId "productB"; Quantity = 1; UnitPrice = 20.0 }
    ]
    PlacedAt = System.DateTime(2024, 1, 15)
 },
    {
        Id = "order2"
        Customer =
            CustomerId "customer2"
        Lines = [
            { Product = ProductId "productA"; Quantity = 1; UnitPrice = 10.0 }
            { Product = ProductId "productC"; Quantity = 3; UnitPrice = 15.0 }
        ]
        PlacedAt = System.DateTime(2024, 2, 10)
    },
    {
        Id = "order3"
        Customer = CustomerId "customer1"
        Lines = [
            { Product = ProductId "productB"; Quantity = 2; UnitPrice = 20.0 }
        ]
        PlacedAt = System.DateTime(2024, 3, 5)
    },
    
    {
        Id = "order4"
        Customer = CustomerId "customer3"
        Lines = [
            { Product = ProductId "productC"; Quantity = 1; UnitPrice = 15.0 }
            { Product = ProductId "productD"; Quantity = 4; UnitPrice = 5.0 }
        ]
        PlacedAt = System.DateTime(2024, 1, 20)
    },
    {
        Id = "order5"
        Customer = CustomerId "customer2"
        Lines = [
            { Product = ProductId "productA"; Quantity = 2; UnitPrice = 10.0 }
            { Product = ProductId "productD"; Quantity = 1; UnitPrice = 5.0 }
        ]
        PlacedAt = System.DateTime(2024, 2, 25)
        
        
    },
    {
        Id = "order6"
        Customer = CustomerId "customer4"
        Lines = [
            { Product = ProductId "productB"; Quantity = 1; UnitPrice = 20.0 }
            { Product = ProductId "productC"; Quantity = 2; UnitPrice = 15.0 }
        ]
        PlacedAt = System.DateTime(2024, 3, 15)
    },
    {
        Id = "order7"
        Customer = CustomerId "customer3"
        Lines = [
            { Product = ProductId "productA"; Quantity = 3; UnitPrice = 10.0 }
            { Product = ProductId "productD"; Quantity = 2; UnitPrice = 5.0 }
        ]
        PlacedAt = System.DateTime(2024, 1, 30)
    },
    {
        Id = "order8"
        Customer = CustomerId "customer4"
        Lines = [
            { Product = ProductId "productC"; Quantity = 1; UnitPrice = 15.0 }
            { Product = ProductId "productD"; Quantity = 3; UnitPrice = 5.0 }
        ]
        PlacedAt = System.DateTime(2024, 2, 5)     
    },
    {
        Id = "order9"
        Customer
            = CustomerId "customer1"
        Lines = [
            { Product = ProductId "productA"; Quantity = 1; UnitPrice = 10.0 }
            { Product = ProductId "productC"; Quantity = 2; UnitPrice = 15.0 }
        ]
        PlacedAt = System.DateTime(2024, 3, 20)
    }
    ]
    

// --- Step 3: Basic aggregations ---
// val orderTotal : Order -> float
// val totalRevenue : Order list -> float
// val largestOrder : Order list -> Order option
// val averageOrderValue : Order list -> float
//
// Key functions: List.map, List.sumBy, List.maxBy, List.averageBy

let orderTotal (order: Order) : float =
        order.Lines |> List.sumBy (fun line -> float line.Quantity * line.UnitPrice)
  
let totalRevenue (orders: Order list) : float =
        orders |> List.sumBy orderTotal

//just which order has the most different products, not the most quantity
let largestOrder (orders: Order list) : Order option =
    orders |> List.maxBy (fun order -> List.length order.Lines) |> Some

let largestQuantityOrder (orders: Order list) : Order option =
    orders |> List.maxBy (fun order -> order.Lines |> List.sumBy _.Quantity) |> Some


let averageOrderValue (orders: Order list) : float =
    orders |> List.averageBy orderTotal

// --- Step 4: Grouping and filtering ---
// val revenueByCustomer : Order list -> (CustomerId * float) list
// val highValueCustomers : float -> Order list -> CustomerId list
// val topSellingProducts : int -> Order list -> (ProductId * int) list
//
// Key functions: List.groupBy, List.sortByDescending, List.truncate, List.collect

let revenueByCustomer (orders: Order list): (CustomerId * float) list =
   let orderByCustomers = orders |> List.groupBy _.Customer
   orderByCustomers |> List.map (fun (customer, orders) -> customer, orders |> List.sumBy orderTotal)
   
let highValueCustomers (threshold: float) (orders: Order list): CustomerId list =
    orders |> List.filter (fun order -> orderTotal order > threshold) |> List.map _.Customer
    
let topSellingProducts (n: int) (orders: Order list): (ProductId * int) list =
    orders |> List.collect (fun order -> order.Lines |> List.map (fun line -> line.Product, line.Quantity))
        |> List.groupBy (fun (product, _) -> product)
        |> List.map (fun (product, items) ->
            let totalQuantity = items |> List.sumBy (fun (_, quantity) -> quantity)
            (product, totalQuantity))
        |> List.sortByDescending snd
        |> List.truncate n
              
      

// --- Step 5: Time-based analysis ---
// val revenueByMonth : Order list -> (int * int * float) list
// val ordersInRange : DateTime -> DateTime -> Order list -> Order list

let revenueByMonth (orders: Order list): (int * int * float) list =
    orders
    |> List.groupBy (fun order -> (order.PlacedAt.Year, order.PlacedAt.Month))
    |> List.map ( fun ((year, month), orderInMonth) ->
                              let revenue =  orderInMonth |> List.sumBy orderTotal
                              (year, month, revenue)
    )

let ordersInRange (start: System.DateTime) (end': System.DateTime) (orders: Order list): Order list =
    orders |> List.filter (fun order -> order.PlacedAt >= start && order.PlacedAt <= end')
    
    
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
