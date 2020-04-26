module ScrapeFrameData.Db

open System.Collections.Specialized
open System.Data.SQLite
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

let maybeParseInt str =
    try
        Some(int str)
    with _ -> None


let collectionToRow (collection: NameValueCollection): DbRow =
    DbRow
        {| CharacterName = collection.Get "CharacterName"
           Id = collection.Get "Id"
           Command = collection.Get "Command"
           TotalDamage = collection.Get "TotalDamage" |> maybeParseInt
           HitLevel = collection.Get "HitLevel"
           Damage = collection.Get "Damage"
           StartUpFrame = collection.Get "StartUpFrame"
           BlockFrame = collection.Get "BlockFrame"
           HitFrame = collection.Get "HitFrame"
           CounterHitFrame = collection.Get "CounterHitFrame"
           Notes = collection.Get "Notes"
           EarliestStartUpFrame = collection.Get "EarliestStartUpFrame" |> maybeParseInt
           LatestStartUpFrame = collection.Get "LatestStartUpFrame" |> maybeParseInt
           EarliestBlockFrame = collection.Get "EarliestBlockFrame" |> maybeParseInt
           LatestBlockFrame = collection.Get "LatestBlockFrame" |> maybeParseInt
           EarliestHitFrame = collection.Get "EarliestHitFrame" |> maybeParseInt
           LatestHitFrame = collection.Get "LatestHitFrame" |> maybeParseInt
           EarliestCounterHitFrame = collection.Get "EarliestCounterHitFrame" |> maybeParseInt
           LatestCounterHitFrame = collection.Get "LatestCounterHitFrame" |> maybeParseInt |}


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

let moves (cnxn: SQLiteConnection): DbRow seq =
    cnxn.Open()
    let sql = """
    select *
    from moves
    """
    //let x = () //cnxn.Query<DbRow>(sql)
    cnxn.Close()
    Seq.empty<DbRow>

//let updateTotalDamage cnxn =
//    moves cnxn
//    |> Seq.map
