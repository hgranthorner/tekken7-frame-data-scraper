open System
open System.Net
open System.Net.Http
open Scrape.Core
open Scrape.CreateDb

let getCharacterData charName =
    let client = new HttpClient()

    let data =
        async {
            let! res =
                sprintf "https://rbnorway.org/%s-t7-frames/" charName
                |> client.GetAsync
                |> Async.AwaitTask
            match res.StatusCode with
            | HttpStatusCode.OK -> return! res.Content.ReadAsStringAsync() |> Async.AwaitTask
            | _ ->
                failwith
                    (sprintf "Failed to get character data for %s. Here's the status code %s." charName
                     <| res.StatusCode.ToString())
                return ""
        } |> Async.RunSynchronously

    if data.Contains "The page you requested could not be found."
    then failwith ("Failed to get character data for " + charName)
    else data


let createDb () =
    let characterNames = [| "ganryu"; "dragunov"; "shaheen"; "claudio"; "yoshimitsu" |]
    let cnxn = Db.createDb()
    Array.iter (fun characterName ->
        getCharacterData characterName
        |> Transform.extractTableStrings
        |> List.map Transform.parseTable
        |> List.map (Db.insertData characterName cnxn)
        |> fun _ -> sprintf "Finished downloading data for %s." characterName |> Console.WriteLine) characterNames
    
    let cnxn = Db.createConnection()
    Db.getMoves cnxn
    |> Seq.map Transform.damageToTotalDamage
    |> Seq.map Transform.getEarliestAndLatestFrames
    |> Seq.iter (Db.updateData cnxn)
    |> fun _ -> Console.WriteLine "Finished updating data."

[<EntryPoint>]
let main argv =
    createDb()
    0 // return an integer exit code
