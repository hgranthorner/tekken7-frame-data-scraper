module Tests

open System
open System.IO
open ScrapeFrameData
open ScrapeFrameData.Models
open Xunit

[<Fact>]
let ``Can get moves from sql db`` () =
    use cnxn = Db.createConnection()
    let x = Db.getMoves cnxn
    do if Seq.length x = 0 then failwith "Please run db creation script." else ()
    x
    |> Seq.take 1
    |> Seq.toList |> ignore
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
    let x = Directory.GetCurrentDirectory()
    let y = Directory.GetParent x
         |> fun x -> x.Name
         |> Directory.GetParent
         |> fun x -> x.Name
         |> Directory.GetParent
         |> fun x -> x.Name
         |> Directory.GetParent
         |> fun x -> x.Name
         |> Directory.GetParent
         |> fun x -> x.Name
         |> Directory.GetParent
         |> fun x -> x.Name
         |> Directory.GetParent
         |> fun x -> x.Name
    Assert.True true