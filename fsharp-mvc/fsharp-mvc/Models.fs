namespace fsharp_mvc.Models

open System.Web

type IUser =
    abstract member SignedIn:unit->bool

type User(httpContext: HttpContextBase) =
    interface IUser with
        member this.SignedIn() = httpContext.Session.Item("signed_in") <> null