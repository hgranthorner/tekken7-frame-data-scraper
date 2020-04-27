module Scrape.Core.Transform

open System.Text.RegularExpressions
open ScrapeFrameData.Models

let maybeParseInt (str: string) =
    try
        Some(int str)
    with _ -> None

let extractTableStrings: ExtractTableStrings =
    fun str ->
        let divider = "<div id=\"basic\"></div>\n<h3>Basic Moves</h3>"
        String.substringFromPatterns "<table>" "</table>" str
        |> String.split divider
        |> List.map TableString

let getBody (TableString x) =
    String.substringFromPatterns "<tbody>" "</tbody>" x |> String.replace "<tbody>" ""

let extractRowStrings: ExtractRowStrings =
    fun tableS ->
        getBody tableS
        |> String.split "</tr>"
        |> Seq.map (String.replace "<tr>" "")
        |> Seq.map (String.replace "\n" "")
        |> Seq.removeLast
        |> Seq.map RowString
        |> Seq.toList

let parseRow: ParseRow =
    fun (RowString str) ->
        String.split "</td>" str
        |> Seq.map (String.replace "<td>" "")
        |> Seq.map (String.replace "\n" "")
        |> Seq.removeLast
        |> Seq.toList
        |> fun xs ->
            match List.length xs with
            | 8 ->
                Row
                    {| Command = xs.[0]
                       HitLevel = xs.[1]
                       Damage = xs.[2]
                       StartUpFrame = xs.[3]
                       BlockFrame = xs.[4]
                       HitFrame = xs.[5]
                       CounterHitFrame = xs.[6]
                       Notes = xs.[7] |}
            | _ ->
                let message = sprintf "Failed to process data: %s" <| String.concat "|" xs
                failwith message

let combineRows: CombineRows = fun rows -> Table rows

let parseTable: ParseTable =
    fun table ->
        table
        |> extractRowStrings
        |> List.map parseRow
        |> combineRows

let damageToTotalDamage (DbRow row): DbRow =
    DbRow
        {| row with
               TotalDamage =
                   if String.isNullOrEmpty row.Damage then "0" else row.Damage
                   |> String.removePattern "("
                   |> String.removePattern ")"
                   |> String.removePattern "~"
                   |> String.removePattern "?"
                   |> String.removeTrailingChar ','
                   |> String.split ","
                   |> Seq.map maybeParseInt
                   |> Seq.map (function
                       | Some (x) -> x
                       | None -> 0)
                   |> Seq.fold (+) 0
                   |> Some |}

let maxMatch (coll: MatchCollection) =
    query {
        for m in coll do
            maxByNullable
                (m.Value
                 |> maybeParseInt
                 |> Option.toNullable)
    }
    |> Option.ofNullable

let minMatch (coll: MatchCollection) =
    query {
        for m in coll do
            minByNullable
                (m.Value
                 |> maybeParseInt
                 |> Option.toNullable)
    }
    |> Option.ofNullable

type MinimumValue =
    | Ten
    | Any

let parseEarliestAndLatestFrames (FrameString frame) (minimumValue: MinimumValue) =
    let pattern =
        match minimumValue with
        | Ten -> "(-*[0-9]{2})"
        | Any -> "(-*[0-9]+)"

    let collection = Regex.Matches(frame, pattern)
    match collection.Count with
    | 0 -> (Some 0, Some 0)
    | _ -> (minMatch collection, maxMatch collection)

let getEarliestAndLatestFrames (DbRow row): DbRow =
    let (eStartup, lStartup) = parseEarliestAndLatestFrames row.StartUpFrame MinimumValue.Ten
    let (eHit, lHit) = parseEarliestAndLatestFrames row.HitFrame MinimumValue.Any
    let (eBlock, lBlock) = parseEarliestAndLatestFrames row.BlockFrame MinimumValue.Any
    let (eCounter, lCounter) = parseEarliestAndLatestFrames row.CounterHitFrame MinimumValue.Any
    DbRow
        {| row with
               EarliestStartUpFrame = eStartup
               LatestStartUpFrame = lStartup
               EarliestBlockFrame = eBlock
               LatestBlockFrame = lBlock
               EarliestHitFrame = eHit
               LatestHitFrame = lHit
               EarliestCounterHitFrame = eCounter
               LatestCounterHitFrame = lCounter |}

let rowToDto (DbRow row) =
    {| Id = row.Id
       CharacterName = row.CharacterName
       Command = row.Command
       HitLevel = row.HitLevel
       Damage = row.Damage
       TotalDamage = row.TotalDamage
       StartUpFrame = row.StartUpFrame |> fun (FrameString x) -> x
       BlockFrame = row.BlockFrame |> fun (FrameString x) -> x
       HitFrame = row.HitFrame |> fun (FrameString x) -> x
       CounterHitFrame = row.CounterHitFrame |> fun (FrameString x) -> x
       Notes = row.Notes
       EarliestStartUpFrame =
           row.EarliestStartUpFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       LatestStartUpFrame =
           row.LatestStartUpFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       EarliestBlockFrame =
           row.EarliestBlockFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       LatestBlockFrame =
           row.LatestBlockFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       EarliestHitFrame =
           row.EarliestHitFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       LatestHitFrame =
           row.LatestHitFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       EarliestCounterHitFrame =
           row.EarliestCounterHitFrame
           |> fun x ->
               if x.IsSome then x.Value else 0
       LatestCounterHitFrame =
           row.LatestCounterHitFrame
           |> fun x ->
               if x.IsSome then x.Value else 0 |}
