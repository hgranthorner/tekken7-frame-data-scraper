namespace Scrape.API.Controllers

open Microsoft.AspNetCore.Mvc
open Scrape.Core
open Scrape.CreateDb
open ScrapeFrameData.Models

[<ApiController>]
[<Route("api/[controller]")>]
type CharactersController () =
    inherit ControllerBase()

    [<HttpGet>]
    member __.GetCharacters() =
        Db.moves()
        |> Seq.map (fun (DbRow x) -> x.CharacterName)
        |> Seq.distinct

    [<HttpGet>]
    [<Route("{characterName}/moves")>]
    member __.GetMovesForCharacter characterName =
        Db.moves()
        |> Seq.filter (fun (DbRow x) -> x.CharacterName = characterName)
        |> Seq.map Transform.rowToDto

    