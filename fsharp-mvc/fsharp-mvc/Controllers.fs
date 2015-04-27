namespace fsharp_mvc
module Controllers =

    open System
    open System.Web
    open System.Web.Mvc
    open fsharp_mvc.Models

    let CLIENT_ID = "valtech.idp.testclient.local"
    let idp_url = String.Format("https://stage-id.valtech.com/oauth2/authorize?response_type={0}&client_id={1}&scope={2}", "code", CLIENT_ID, "email")

    let GetSignInUrl isSignedIn = 
        if (isSignedIn) then "/" else idp_url

    type IdpController(user:IUser) =
        inherit Controller()

        member this.SignIn() = user.IsSignedIn |> GetSignInUrl |> this.Redirect
