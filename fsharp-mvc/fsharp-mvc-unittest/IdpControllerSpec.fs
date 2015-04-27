namespace fsharp_mvc_unittest
module IdpControllerSpec = 

    open NUnit.Framework
    open System
    open System.Web
    open System.Web.Mvc
    open Foq

    open fsharp_mvc
    open fsharp_mvc.Controllers
    open fsharp_mvc.Models

    // TODO: Should this be in a separate file?
    module GetSignedIn =

        [<Test>]
        let ``GetSignInUrl should return root when already signed in``() =
            Assert.That(true |> GetSignInUrl, Is.EqualTo("/"))

        [<Test>]
        let ``GetSignInUrl should return IDP when not signed in``() =
            let CLIENT_ID = "valtech.idp.testclient.local"
            let idp_url = String.Format("https://stage-id.valtech.com/oauth2/authorize?response_type={0}&client_id={1}&scope={2}", "code", CLIENT_ID, "email")
            Assert.That(false |> GetSignInUrl, Is.EqualTo(idp_url))


    // TODO: Figure out how to organize F# tests...
    module IdpController =
        module SignIn =
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

        module SignOut =
            [<Test>]
            let ``When signing out: Should redirect to SignOut url``() =
                let fakeUser =
                    Mock<IUser>()
                        .Setup(fun x -> <@ x.Session @>).Returns(mock() : HttpSessionStateBase)
                        .Create()
                let CLIENT_ID = "valtech.idp.testclient.local"
                let end_session_url = String.Format("https://stage-id.valtech.com/oidc/end-session?client_id={0}", CLIENT_ID)
                let controller = new IdpController(fakeUser)
                let actionResult = controller.SignOut()

                Assert.That(actionResult.Url, Is.EqualTo(end_session_url))

            [<Test>]
            let ``When signing out: Should clear the session``() =
                // Arrange
                let session = mock() : HttpSessionStateBase
                let fakeUser =
                    Mock<IUser>()
                        .Setup(fun x -> <@ x.Session @>).Returns(session)
                        .Create()
                let controller = new IdpController(fakeUser)
                // Act
                let actionResult = controller.SignOut()
                //Assert
                verify <@ session.Clear() @> atleastonce
                // TODO: Discuss if we want to go down the verify-method-is-called-pattern or not.