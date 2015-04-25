module IdpControllerSpec // namespace fsharp_mvc_unittest

open NUnit.Framework
open System
open System.Web.Mvc

open fsharp_mvc.Controllers

[<Test>]
let ``When signing in: Should redirect``() =
    let controller = new IdpController()
    let actionResult = controller.SignIn()
    Assert.False(String.IsNullOrEmpty(actionResult.Url))
