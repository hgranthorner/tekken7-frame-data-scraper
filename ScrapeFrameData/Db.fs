module ScrapeFrameData.Db

open System.Data.SQLite
open Dapper
open ScrapeFrameData.Models

let createDb =
    let dbFileName = "test.db"
    let dbPath = "/Users/grant/Dev/fsharp/ScrapeFrameData/" + dbFileName
    let connectionStringFile =
        sprintf "Data Source=%s;Version=3;" <| dbPath
    
    SQLiteConnection.CreateFile dbPath
    
    let connection = new SQLiteConnection(connectionStringFile)
    connection.Open()
    
    let tableSql = """
    create table Moves (
    CharacterName TEXT,
    Command TEXT,
    HitLevel TEXT,
    Damage TEXT,
    StartUpFrame TEXT,
    BlockFrame TEXT,
    HitFrame TEXT,
    CounterHitFrame TEXT,
    Notes TEXT
    )
    """
    
    let command = new SQLiteCommand(tableSql, connection)
    command.ExecuteNonQuery() |> ignore
    connection.Close()
    connection

let loadData characterName (cnxn: SQLiteConnection) (Table xs) =
    cnxn.Open()
    let insertData = """
    insert into Moves (CharacterName,Command,HitLevel,Damage,StartUpFrame,BlockFrame,HitFrame,CounterHitFrame,Notes)
    values (@CharacterName,@Command,@HitLevel,@Damage,@StartUpFrame,@BlockFrame,@HitFrame,@CounterHitFrame,@Notes)
    """
    xs
    |> Seq.map (fun x -> {|x with CharacterName = characterName|})
    |> Seq.iter (fun x -> cnxn.Execute(insertData, x) |> ignore)
    cnxn.Close()

