module Tests

open System
open System.IO
open ScrapeFrameData
open Xunit

[<Fact>]
let ``My test`` () =
    use cnxn = Db.createConnection()
    let moves = Db.moves cnxn
    let data = (query {
        for move in moves do
            select move.CharacterName
    } |> Seq.toList)
    Console.WriteLine data
    Assert.True(true)
    
[<Theory>]
[<InlineData('a', "abca", "abc")>]
[<InlineData('b', "", "")>]
[<InlineData('b', "b", "")>]
let ``String.removeTrailingChar works`` char str expected =
    let actual = String.removeTrailingChar char str
    Assert.Equal(expected, actual)