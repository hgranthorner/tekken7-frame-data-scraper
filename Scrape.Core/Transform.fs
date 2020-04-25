module Scrape.Core.Transform

open ScrapeFrameData.Models

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
        let vals =
            String.split "</td>" str
            |> Seq.map (String.replace "<td>" "")
            |> Seq.map (String.replace "\n" "")
            |> Seq.removeLast
            |> Seq.toList

        match List.length vals with
        | 8 ->
            { Command = vals.[0]
              HitLevel = vals.[1]
              Damage = vals.[2]
              StartUpFrame = vals.[3]
              BlockFrame = vals.[4]
              HitFrame = vals.[5]
              CounterHitFrame = vals.[6]
              Notes = vals.[7] }
        | _ ->
            let message = sprintf "Failed to process data: %s" <| String.concat "|" vals
            failwith message

let combineRows: CombineRows = fun rows -> Table rows

let parseTable: ParseTable =
    fun table ->
        table
        |> extractRowStrings
        |> List.map parseRow
        |> combineRows
