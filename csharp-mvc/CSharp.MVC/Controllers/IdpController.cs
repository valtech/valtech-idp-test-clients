using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace CSharp.MVC.Controllers
{
    public class IdpController : Controller
    {
        private const string CLIENT_ID = "valtech.idp.testclient.local";

        public ActionResult SignIn()
        {
            if (Session["signed_in"] != null) return Redirect("/");
            var authorizeUrl = String.Format("https://stage-id.valtech.com/oauth2/authorize?response_type={0}&client_id={1}&scope={2}", "code", CLIENT_ID, "email");
            return Redirect(authorizeUrl);
        }

        public ActionResult Callback(string code)
        {
            var accessToken = ExchangeCodeForAccessToken(code);
            var userInfo = FetchUserInfo(accessToken);
            Session["signed_in"] = true;
            Session["email"] = userInfo["email"];
            return Redirect("/");
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
                postData.Add("client_id", CLIENT_ID);
                postData.Add("client_secret", ConfigurationManager.AppSettings["IdpClientSecret"]);
                var responseData = webClient.UploadValues("https://stage-id.valtech.com/oauth2/token", postData);
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
                var json = webClient.DownloadString("https://stage-id.valtech.com/api/users/me");
                var jss = new JavaScriptSerializer();
                return jss.Deserialize<dynamic>(json);
            }
        }

        public ActionResult SignOut()
        {
            Session.Clear();
            return Redirect(String.Format("https://stage-id.valtech.com/oidc/end-session?client_id={0}", CLIENT_ID));
        }
    }
}
