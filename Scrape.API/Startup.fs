namespace Scrape.API

open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer

type Startup private () =
    new (configuration: IConfiguration) as this =
        Startup() then
        this.Configuration <- configuration

    // This method gets called by the runtime. Use this method to add services to the container.
    member this.ConfigureServices(services: IServiceCollection) =
        // Add framework services.
        services.AddControllers() |> ignore
        services.AddMvc (fun option -> option.EnableEndpointRouting <- false) |> ignore
        services.AddSpaStaticFiles (fun configuration -> configuration.RootPath <- "tekkenclient/build")

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then
            app.UseDeveloperExceptionPage() |> ignore
            
        

        app.UseHttpsRedirection() |> ignore
        app.UseStaticFiles() |> ignore
        app.UseSpaStaticFiles()
        app.UseMvc() |> ignore
        app.UseSpa(fun spa ->
            spa.Options.SourcePath <- Path.Join(env.ContentRootPath, "tekkenclient")
            if env.IsDevelopment()
                then spa.UseReactDevelopmentServer("start"))
//        app.UseRouting() |> ignore
//        app.UseEndpoints(fun endpoints ->
//            endpoints.MapControllers() |> ignore
//            ) |> ignore

    member val Configuration : IConfiguration = null with get, set
