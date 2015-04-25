module IdpControllerSpec // namespace fsharp_mvc_unittest

open NUnit.Framework
open System
open System.Web.Mvc

open fsharp_mvc.Controllers

type ``When signing in``() =

    [<Test>]
    member this.``Should redirect``() =
        let controller = new IdpController()
        let actionResult = controller.SignIn()
        Assert.False(String.IsNullOrEmpty(actionResult.Url))