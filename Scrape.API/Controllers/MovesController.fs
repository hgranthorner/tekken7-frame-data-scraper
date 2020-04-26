namespace Scrape.API.Controllers

open Microsoft.AspNetCore.Mvc
open Scrape.CreateDb

[<ApiController>]
[<Route("api/[controller]")>]
type MovesController () =
    inherit ControllerBase()

    [<HttpGet>]
    member __.Get() =
        Db.createConnection()
        |> Db.getMoves


