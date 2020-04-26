module ScrapeFrameData.Db

open System.Collections.Specialized
open System.Data.SQLite
open Scrape.Core
open ScrapeFrameData.Models

#if DEBUG
[<Literal>]
let connectionString = "Data Source=/Users/grant/Dev/fsharp/ScrapeFrameData/data.db;Version=3;"

[<Literal>]
let resolutionPath = "/Users/grant/Dev/fsharp/ScrapeFrameData/ScrapeFrameData/sqlite-interop-dll"
#endif
#if RELEASE
[<Literal>]
let connectionString = "Data Source=/Users/grant/Dev/fsharp/ScrapeFrameData/data.db;Version=3;"

[<Literal>]
let resolutionPath = Directory.GetCurrentDirectory()
#endif

let createConnection = fun () -> new SQLiteConnection(connectionString)

let createDb =
    fun () ->
        SQLiteConnection.CreateFile "/Users/grant/Dev/fsharp/ScrapeFrameData/data.db"

        let connection = new SQLiteConnection(connectionString)
        connection.Open()

        let tableSql = """
    create table Moves (
    Id TEXT PRIMARY KEY,
    CharacterName TEXT,
    Command TEXT,
    HitLevel TEXT,
    Damage TEXT,
    TotalDamage INTEGER DEFAULT 0,
    StartUpFrame TEXT,
    EarliestStartUpFrame INTEGER DEFAULT 0,
    LatestStartUpFrame INTEGER DEFAULT 0,
    BlockFrame TEXT,
    EarliestBlockFrame INTEGER DEFAULT 0,
    LatestBlockFrame INTEGER DEFAULT 0,
    HitFrame TEXT,
    EarliestHitFrame INTEGER DEFAULT 0,
    LatestHitFrame INTEGER DEFAULT 0,
    CounterHitFrame TEXT,
    EarliestCounterHitFrame INTEGER DEFAULT 0,
    LatestCounterHitFrame INTEGER DEFAULT 0,
    Notes TEXT
    );

    CREATE TRIGGER AutoGenerateGUID
    AFTER INSERT ON Moves
    FOR EACH ROW
    WHEN (NEW.Id IS NULL)
    BEGIN
       UPDATE Moves SET Id = (select hex( randomblob(4)) || '-' || hex( randomblob(2))
                 || '-' || '4' || substr( hex( randomblob(2)), 2) || '-'
                 || substr('AB89', 1 + (abs(random()) % 4) , 1)  ||
                 substr(hex(randomblob(2)), 2) || '-' || hex(randomblob(6)) ) WHERE rowid = NEW.rowid;
    END;
    """

        let command = new SQLiteCommand(tableSql, connection)
        command.ExecuteNonQuery() |> ignore
        connection.Close()
        connection


let executeQuery (cnxn: SQLiteConnection) (command: SQLiteCommand) =
    cnxn.Open()
    let x = command.ExecuteReader()
    let mutable lst = list.Empty
    while x.HasRows do
        lst <- List.append lst [ x.GetValues() ]
        x.Read() |> ignore
    cnxn.Close()
    lst

let collectionToRow (collection: NameValueCollection): DbRow =
    DbRow
        {| CharacterName = collection.Get "CharacterName"
           Id = collection.Get "Id"
           Command = collection.Get "Command"
           TotalDamage = collection.Get "TotalDamage" |> Transform.maybeParseInt
           HitLevel = collection.Get "HitLevel"
           Damage = collection.Get "Damage"
           StartUpFrame = collection.Get "StartUpFrame" |> FrameString
           BlockFrame = collection.Get "BlockFrame" |> FrameString
           HitFrame = collection.Get "HitFrame" |> FrameString
           CounterHitFrame = collection.Get "CounterHitFrame" |> FrameString
           Notes = collection.Get "Notes"
           EarliestStartUpFrame = collection.Get "EarliestStartUpFrame" |> Transform.maybeParseInt
           LatestStartUpFrame = collection.Get "LatestStartUpFrame" |> Transform.maybeParseInt
           EarliestBlockFrame = collection.Get "EarliestBlockFrame" |> Transform.maybeParseInt
           LatestBlockFrame = collection.Get "LatestBlockFrame" |> Transform.maybeParseInt
           EarliestHitFrame = collection.Get "EarliestHitFrame" |> Transform.maybeParseInt
           LatestHitFrame = collection.Get "LatestHitFrame" |> Transform.maybeParseInt
           EarliestCounterHitFrame = collection.Get "EarliestCounterHitFrame" |> Transform.maybeParseInt
           LatestCounterHitFrame = collection.Get "LatestCounterHitFrame" |> Transform.maybeParseInt |}


let getMoves (cnxn: SQLiteConnection) =
    let command = cnxn.CreateCommand()
    command.CommandText <- "select * from moves"
    executeQuery cnxn command |> Seq.map collectionToRow

let buildSql (Row row) charName =
    sprintf """
    insert into Moves (CharacterName,Command,HitLevel,Damage,StartUpFrame,BlockFrame,HitFrame,CounterHitFrame,Notes)
    values ('%s','%s','%s','%s','%s','%s','%s','%s','%s')
    """ charName row.Command row.HitLevel row.Damage row.StartUpFrame row.BlockFrame row.HitFrame row.CounterHitFrame
        row.Notes

let insertData characterName (cnxn: SQLiteConnection) (Table xs) =
    cnxn.Open()
    xs
    |> List.map (fun x ->
        let command = cnxn.CreateCommand()
        command.CommandText <- buildSql x characterName
        command)
    |> List.iter (fun (x: SQLiteCommand) -> x.ExecuteNonQuery() |> ignore)
    cnxn.Close()
