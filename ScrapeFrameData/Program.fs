open System
open System.IO
open System.Net.Http
open Scrape.Core
open ScrapeFrameData

let getCharacterData charName =
    let client = new HttpClient()
    async {
        let! res =
            sprintf "https://rbnorway.org/%s-t7-frames/" charName
            |> client.GetAsync
            |> Async.AwaitTask
        return! res.Content.ReadAsStringAsync() |> Async.AwaitTask
    }
    |> Async.RunSynchronously

[<EntryPoint>]
let main argv =
    
    let characterNames = [| "ganryu"; "dragunov"; "shaheen"; "claudio" |]
    let path = if Array.length argv <> 1
               then failwith "Please pass path for db."//"/Users/grant/Dev/fsharp/ScrapeFrameData/data.txt"
               else argv.[0]
    do if File.Exists path then
        ()
       else
           File.Create path
           |> fun x -> x.Close()
           |> ignore

    let cnxn = Db.createDb
    Array.iter (fun characterName ->
        getCharacterData characterName
        |> Transform.extractTableStrings
        |> List.map Transform.parseTable
        |> List.map (Db.loadData characterName cnxn)
        |> fun _ -> sprintf "Finished downloading data for %s." characterName |> Console.WriteLine) characterNames
    //    |> fun x -> File.AppendAllText(path, x.ToString())

    0 // return an integer exit code
