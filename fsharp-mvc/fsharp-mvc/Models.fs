namespace fsharp_mvc.Models

open System.Web

type IUser =
    abstract member IsSignedIn : bool with get
    abstract member Session : HttpSessionStateBase with get

type User(httpContext: HttpContextBase) =
    interface IUser with
        member this.IsSignedIn = httpContext.Session.Item("signed_in") <> null
        member this.Session = httpContext.Session
        