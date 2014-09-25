using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web.Configuration;
using Nancy;
using Nancy.Json;
using Nancy.Responses;
using HttpStatusCode = Nancy.HttpStatusCode;

namespace CSharp.Nancy
{
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = parameters =>
            {
                ViewBag["signed_in"] = IsSignedIn;
               
                if (IsSignedIn)
                {
                    ViewBag["header"] = "Welcome!";
                    ViewBag["text"] = string.Format("Signed in as {0}.", Request.Session["email"]);
                }
                else
                {
                    ViewBag["header"] = "Not signed in";
                    ViewBag["text"] = "Click the button below to sign in.";
                }
                return View["index"];
            };

            Get["/sign-in"] = parameters =>
            {
                if (IsSignedIn) return Response.AsRedirect("/");

                var state = Guid.NewGuid().ToString();
                var authorizeUrl = String.Format("{0}/oauth2/authorize?response_type={1}&client_id={2}&scope={3}&state={4}", IdpBaseUrl, "code", IdpClientId, "email", state);

                Request.Session["state"] = state;
                return new RedirectResponse(authorizeUrl);
            };

            Get["/sign-in/callback"] = parameters =>
            {
                if (!string.Equals(Request.Session["state"], (string) Request.Query["state"]))
                {
                    return Response.AsJson(new { error_description = "Invalid state, possible CSRF detected."}, HttpStatusCode.BadRequest);
                }

                var accessToken = ExchangeCodeForAccessToken(Request.Query.code);
                var userInfo = FetchUserInfo(accessToken);

                Request.Session["signed_in"] = true;
                Request.Session["email"] = userInfo["email"];

                return Response.AsRedirect("/");
            };

            Get["/sign-out"] = parameters =>
            {
                if (!IsSignedIn) return Response.AsRedirect("/");
                Request.Session.DeleteAll();
                return Response.AsRedirect(string.Format("{0}/oidc/end-session?client_id={1}", IdpBaseUrl, IdpClientId));
            };
        }

        private string ExchangeCodeForAccessToken(string code)
        {
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                NameValueCollection postData = new NameValueCollection();
                postData.Add("grant_type", "authorization_code");
                postData.Add("code", code);
                postData.Add("client_id", IdpClientId);
                postData.Add("client_secret", IdpClientSecret);
                var responseData = webClient.UploadValues(string.Format("{0}/oauth2/token", IdpBaseUrl), postData);
                var json = Encoding.UTF8.GetString(responseData);
                var jss = new JavaScriptSerializer();
                dynamic data = jss.Deserialize<dynamic>(json);
                return data["access_token"];
            }
        }

        private dynamic FetchUserInfo(string accessToken)
        {
            using (var webClient = new WebClient())
            {
                webClient.Encoding = Encoding.UTF8;
                webClient.Headers.Add("Authorization", "Bearer " + accessToken);
                var json = webClient.DownloadString(string.Format("{0}/api/users/me", IdpBaseUrl));
                var jss = new JavaScriptSerializer();
                return jss.Deserialize<dynamic>(json);
            }
        }

        private bool IsSignedIn
        {
            get { return Request.Session["signed_in"] != null; }
        }

        private string IdpBaseUrl
        {
            get { return "https://stage-id.valtech.com"; }
        }

        private string IdpClientId
        {
            get { return "valtech.idp.testclient.local"; }
        }

        private string IdpClientSecret
        {
            get { return WebConfigurationManager.AppSettings["IdpClientSecret"]; }
        }
    }
}