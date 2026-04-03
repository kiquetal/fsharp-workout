module WeatherDashboard

// ============================================
// SESSION 6 — Integration & Capstone
// ============================================
// Tie together everything from Sessions 1–5.
// Parse raw weather data, validate it,
// aggregate it, and display a report.
//
// No new concepts — just applying what you know.
//
// Steps:
// 1. Define domain types
// 2. Parsing module
// 3. Validation module
// 4. Analytics module
// 5. Report module
// 6. Wire it all up in main
// ============================================

open System


// --- Step 1: Define domain types ---
// StationId — single-case DU
// Temperature — single-case DU with validation
// Humidity — single-case DU with validation
// Reading — station, temperature, humidity, timestamp
// ParseError — what can go wrong

type StationId = StationId of string
type Temperature = private Temperature of float
type Humidity = private Humidity of float

type Reading = {
    Station: StationId
    Temperature: Temperature
    Humidity: Humidity
    Timestamp: DateTime
}
type Error =
    | ParseError of string
    | ValidationError of string

module constructors =
    let createTemperature(temp: float): Result<Temperature, Error> =
        if temp >= -50.0 && temp <= 60.0 then Ok (Temperature temp)
        else Error (ValidationError $"Invalid temperature: {temp}")
    let createHumidity(hum: float): Result<Humidity, Error> =
        if hum >= 0.0 && hum <= 100.0 then Ok (Humidity hum)
        else Error (ValidationError $"Invalid humidity: {hum}")
    let temperatureValue (Temperature t) = t
    let humidityValue (Humidity h) = h
    
// --- Step 2: Parsing module ---
// Parse "station-01,23.5,65,2024-03-15T10:30:00" into a Reading
//
// val parseLine : string -> Result<Reading, ParseError>
// val parseAll : string list -> Reading list * ParseError list

module parsing =
    let parseLine (line: string) : Result<Reading, Error> = 
        let splitLine = line.Split(',')
        if splitLine.Length <> 4 then Error (ParseError $"Invalid line format: {line}")
        else
            let stationId = StationId splitLine.[0]
            let tempResult = match System.Double.TryParse splitLine.[1] with
                                | (true, temp) -> constructors.createTemperature temp
                                | (false, _) -> Error (ParseError $"Invalid temperature: {splitLine.[1]}")
            let humResult = match System.Double.TryParse splitLine.[2] with
                                | (true, hum) -> constructors.createHumidity hum
                                | (false, _) -> Error (ParseError $"Invalid humidity: {splitLine.[2]}")
            let timestampResult = match DateTime.TryParse splitLine.[3] with
                                    | (true, dt) -> Ok dt
                                    | (false, _) -> Error (ParseError $"Invalid timestamp: {splitLine.[3]}")
            match tempResult, humResult, timestampResult with
            | Ok temp, Ok hum, Ok timestamp -> Ok { Station = stationId; Temperature = temp; Humidity = hum; Timestamp = timestamp }
            | Error e, _, _ -> Error e
            | _, Error e, _ -> Error e
            | _, _, Error e -> Error e
            
    let parseAll (lines: string list): (Reading list * Error list) =
        let readingOk, errorRead =
             lines |> List.map parseLine 
              |> List.partition (fun result -> match result with | Ok _ -> true | Error _ -> false)
        let okList = readingOk |> List.choose (fun r->
                    match r with
                    | Ok reading -> Some reading
                    | Error _ -> None)
        let errorList = errorRead |> List.choose (fun r->
                    match r with
                    | Ok _ -> None
                    | Error e -> Some e)
        (okList, errorList)         

// --- Step 3: Validation module ---
// Business rules: temp between -50 and 60, humidity 0-100, not in future
//
// val validateReading : Reading -> Result<Reading, ValidationError>


module validation =
    let validateReading (reading: Reading) : Result<Reading, Error> =
        let tempValue = constructors.temperatureValue reading.Temperature
        let humValue = constructors.humidityValue reading.Humidity
        if tempValue < -50.0 || tempValue > 60.0 then Error (ValidationError $"Temperature out of range: {tempValue}")
        else if humValue < 0.0 || humValue > 100.0 then Error (ValidationError $"Humidity out of range: {humValue}")
        else if reading.Timestamp > DateTime.Now then Error (ValidationError $"Timestamp cannot be in the future: {reading.Timestamp}")
        else Ok reading

// --- Step 4: Analytics module ---
// Aggregate readings using Session 5 patterns
//
// val averageTemperature : Reading list -> float
// val averageHumidity : Reading list -> float
// val hottestReading : Reading list -> Reading option
// val coldestReading : Reading list -> Reading option
// val readingsByStation : Reading list -> (StationId * Reading list) list
// val dailyAverages : Reading list -> (DateTime * float * float) list

module analytics =
    let averareTemperature (readings: Reading list) : float =
        readings |> List.averageBy (fun r -> constructors.temperatureValue r.Temperature)
    let averageHumidity (readings: Reading list) : float =
        readings |> List.averageBy (fun r -> constructors.humidityValue r.Humidity)
    let hottestReading (readings: Reading list) : Reading option =
        match readings with
        | [] -> None
        | _ -> readings |> List.maxBy (fun r -> constructors.temperatureValue r.Temperature) |> Some
    let coldestReading (readings: Reading list) : Reading option =
        match readings with
        | [] -> None
        | _ -> readings |> List.minBy (fun r -> constructors.temperatureValue r.Temperature) |> Some 
    let readingsByStation (readings: Reading list) : (StationId * Reading list) list =
        let readingByStation = readings |> List.groupBy _.Station
        readingByStation
        
    let dailyAverages(reading: Reading list): (DateTime * float * float) list =
       let groupByDay = reading |> List.groupBy _.Timestamp.Date
       groupByDay |>
       List.map (fun (date, readings) ->
                let avgTemp = readings |> List.averageBy (fun r -> constructors.temperatureValue r.Temperature)
                let avgHum = readings |> List.averageBy (fun r -> constructors.humidityValue r.Humidity)
                (date, avgTemp, avgHum))
            
       


// --- Step 5: Report module ---
// Format analytics into readable output
//
// val stationSummary : StationId -> Reading list -> string
// val fullReport : Reading list -> ParseError list -> string

// TODO: module Report = ...


// --- Sample data ---
let rawLines = [
    "station-01,23.5,65,2024-03-15T10:30:00"
    "station-02,18.2,70,2024-03-15T11:00:00"
    "station-01,bad,45,2024-03-15T12:00:00"
    "station-03,25.1,80,2024-03-15T13:00:00"
    "station-02,19.8,68,2024-03-15T14:00:00"
    "station-01,22.0,60,2024-03-15T15:00:00"
    "station-03,,75,2024-03-15T16:00:00"
    "station-02,17.5,72,2024-03-16T09:00:00"
    "station-01,24.3,58,2024-03-16T10:00:00"
    "station-03,26.8,82,2024-03-16T11:00:00"
    "station-02,20.1,66,2024-03-16T12:00:00"
    "station-01,21.5,62,2024-03-16T13:00:00"
    "station-03,150.0,85,2024-03-16T14:00:00"
    "too,few,fields"
    "station-02,19.0,67,2024-03-16T15:00:00"
]


// --- Step 6: Wire it all up ---
[<EntryPoint>]
let main _ =
    printfn "Session 6 — Weather Station Dashboard"
    // TODO: parse → validate → analyze → report
    0
