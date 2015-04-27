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
    let GetSignOutUrl = 
        String.Format("https://stage-id.valtech.com/oidc/end-session?client_id={0}", CLIENT_ID)


    type IdpController(user:IUser) =
        inherit Controller()

        member this.SignIn() = user.IsSignedIn |> GetSignInUrl |> this.Redirect
        member this.SignOut() = 
            user.Session.Clear() 
            GetSignOutUrl |> this.Redirect
