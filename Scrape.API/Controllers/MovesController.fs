namespace Scrape.API.Controllers

open Microsoft.AspNetCore.Mvc
open Scrape.Core
open Scrape.CreateDb
open ScrapeFrameData.Models

[<ApiController>]
[<Route("api/[controller]")>]
type MovesController () =
    inherit ControllerBase()

    [<HttpGet>]
    member __.GetAllMoves() =
        Db.createConnection()
        |> Db.getMoves
        |> Seq.map Transform.rowToDto
        
    [<HttpGet>]
    [<Route("{character}")>]
    member __.GetMovesForCharacter(character: string) =
        Db.createConnection()
        |> Db.getMoves
        |> Seq.filter (fun (DbRow move) -> move.CharacterName.ToLower() = character.ToLower())
        |> Seq.map Transform.rowToDto
