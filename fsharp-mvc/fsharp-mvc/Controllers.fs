namespace fsharp_mvc.Controllers

open System
open System.Web
open System.Web.Mvc

open fsharp_mvc.Models

type IdpController(user:IUser) =
    inherit Controller()

    member this.SignIn() =
        let CLIENT_ID = "valtech.idp.testclient.local"
        let idp_url = String.Format("https://stage-id.valtech.com/oauth2/authorize?response_type={0}&client_id={1}&scope={2}", "code", CLIENT_ID, "email")
        base.Redirect(if user.IsSignedIn then "/" else idp_url)
