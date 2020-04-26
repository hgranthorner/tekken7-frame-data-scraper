module Tests

open System
open System.IO
open ScrapeFrameData
open ScrapeFrameData.Models
open Xunit

[<Fact>]
let ``Can get moves from sql db`` () =
    use cnxn = Db.createConnection()
    let moves = Db.getMoves cnxn
    let x = Seq.toList moves
    Assert.True(true)
    
[<Theory>]
[<InlineData('a', "abca", "abc")>]
[<InlineData('a', "abcaaa", "abc")>]
[<InlineData('b', "", "")>]
[<InlineData('b', "b", "")>]
let ``String removeTrailingChar works`` char str expected =
    let actual = String.removeTrailingChar char str
    Assert.Equal(expected, actual)
    
[<Fact>]
let ``Test directory`` () =
    Console.WriteLine Directory.GetCurrentDirectory
    Assert.True true