module IdpControllerSpec // namespace fsharp_mvc_unittest

open NUnit.Framework
open System
open System.Web
open System.Web.Mvc
open Foq

open fsharp_mvc.Controllers
open fsharp_mvc.Models

[<Test>]
let ``When signing in: Should redirect to root when already signed in``() =
    let fakeUser =
        Mock<IUser>()
            .Setup(fun x -> <@ x.IsSignedIn @>).Returns(true)
            .Create()
    let controller = new IdpController(fakeUser)
    let actionResult = controller.SignIn()
    Assert.That(actionResult.Url, Is.EqualTo("/"))

[<Test>]
let ``When signing in: Should redirect to IDP when not signed in``() =
    let CLIENT_ID = "valtech.idp.testclient.local"
    let idp_url = String.Format("https://stage-id.valtech.com/oauth2/authorize?response_type={0}&client_id={1}&scope={2}", "code", CLIENT_ID, "email")
    let fakeUser =
        Mock<IUser>()
            .Setup(fun x -> <@ x.IsSignedIn @>).Returns(false)
            .Create()
    let controller = new IdpController(fakeUser)
    let actionResult = controller.SignIn()
    Assert.That(actionResult.Url, Is.EqualTo(idp_url))
