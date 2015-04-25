namespace fsharp_mvc.Controllers

open System.Web.Mvc

type IdpController() =
    inherit Controller()

    member this.SignIn() = base.Redirect("/")
