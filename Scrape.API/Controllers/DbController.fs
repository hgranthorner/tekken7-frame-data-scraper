namespace Scrape.API.Controllers

open Microsoft.AspNetCore.Mvc

[<ApiController>]
[<Route("api/[controller]")>]
type DbController () =
    inherit ControllerBase()

    [<HttpGet>]
    member __.Get() =
        Scrape.CreateDb.Generate.createDb()


