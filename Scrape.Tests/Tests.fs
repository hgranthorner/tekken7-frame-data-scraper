namespace Tests

open ScrapeFrameData
open Xunit

module Db =

    [<Fact>]
    let ``Db.Can get moves from sql db`` () =
        use cnxn = Db.createConnection ()
        let x = Db.getMoves cnxn
        do if Seq.length x = 0
           then failwith "Please run db creation script."
           else ()
        x
        |> Seq.take 1
        |> Seq.toList
        |> ignore
        Assert.True(true)

module StringExtensions =
    
    
    [<Theory>]
    [<InlineData('a', "abca", "abc")>]
    [<InlineData('a', "abcaaa", "abc")>]
    [<InlineData('b', "", "")>]
    [<InlineData('b', "b", "")>]
    let ``String.removeTrailingChar works`` char str expected =
        let actual = String.removeTrailingChar char str
        Assert.Equal(expected, actual)
