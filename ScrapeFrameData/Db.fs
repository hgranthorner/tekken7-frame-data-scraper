module ScrapeFrameData.Db

open System.Data.SQLite
open Dapper
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

let createConnection = fun () ->
    new SQLiteConnection(connectionString)

let createDb = fun () ->
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
    TotalDamage INTEGER,
    StartUpFrame TEXT,
    BlockFrame TEXT,
    HitFrame TEXT,
    CounterHitFrame TEXT,
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

let insertData characterName (cnxn: SQLiteConnection) (Table xs) =
    cnxn.Open()
    let sql = """
    insert into Moves (CharacterName,Command,HitLevel,Damage,StartUpFrame,BlockFrame,HitFrame,CounterHitFrame,Notes)
    values (@CharacterName,@Command,@HitLevel,@Damage,@StartUpFrame,@BlockFrame,@HitFrame,@CounterHitFrame,@Notes)
    """
    xs
    |> Seq.map (fun x -> {| x with CharacterName = characterName |})
    |> Seq.iter (fun x -> cnxn.Execute(sql, x) |> ignore)
    cnxn.Close()

let moves (cnxn: SQLiteConnection): DbRow seq =
    cnxn.Open()
    let sql = """
    select *
    from moves
    """
    let x = cnxn.Query<DbRow> sql
    cnxn.Close()
    x
    
//let updateTotalDamage cnxn =
//    moves cnxn
//    |> Seq.map