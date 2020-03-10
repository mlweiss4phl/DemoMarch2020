using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Okta.AspNetCore;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Security.Claims;

namespace okta_aspnetcore_mvc_example.Controllers
{
    public class AccountController : Controller
    {
        class UserLogin
        {
            public string LastLoginDT { get; set; }
        }
        private static readonly HttpClient _Client = new HttpClient();

        public IActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SignIn([FromForm]string sessionToken)
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
            {
                var properties = new AuthenticationProperties();
                properties.Items.Add("sessionToken", sessionToken);

                properties.RedirectUri = "/Home/Profile";

                UpdateUserAsync("0"); //arguemt is fill in
                return Challenge(properties, OktaDefaults.MvcAuthenticationScheme);                

            }
            

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult SignOut()
        {
            return new SignOutResult(
                new[]
                {
                     OktaDefaults.MvcAuthenticationScheme,
                     CookieAuthenticationDefaults.AuthenticationScheme,
                },
                new AuthenticationProperties { RedirectUri = "/Home/" });
        }

        public async System.Threading.Tasks.Task UpdateUserAsync(string strLogins)
        {
            string url = "https://dev-176138.okta.com/api/v1/users/me"; //+ HttpContext.User.Identity.Name;
            UserLogin ul = new UserLogin() { LastLoginDT = System.DateTime.Now.ToString() };
            var json = JsonSerializer.Serialize(ul);
            var response = await Request(HttpMethod.Post, url, json, new Dictionary<string, string>(), strLogins);
            string responseText = await response.Content.ReadAsStringAsync();
        }

        static async System.Threading.Tasks.Task<HttpResponseMessage> Request(HttpMethod pMethod, string pUrl, string pJsonContent, Dictionary<string, string> pHeaders, string strLogins)
        {
            var httpRequestMessage = new HttpRequestMessage();
            try
            {

                httpRequestMessage.Method = pMethod;
                httpRequestMessage.RequestUri = new System.Uri(pUrl);
                httpRequestMessage.Headers.Add("Accept", "application/json");
                //httpRequestMessage.Headers.Add("Content-Type", "application/json");
                httpRequestMessage.Headers.Add("Authorization", "SSWS 00Ba8NKIFShdhsgzohVux_FzX_vCjbmtNbjVMZne04");
                /*
                            foreach (var head in pHeaders)
                            {
                                httpRequestMessage.Headers.Add(head.Key, head.Value);
                            }
                */
                string strBody = @"{""profile"": {""last_login"": """ + System.DateTime.Now.ToString() + @" ""} }";
//                string strBody = @"{""profile"": {""last_login"": """ + System.DateTime.Now.ToString() + @" "", ""logins"": """ + strLogins + @" ""} }";
                var json = JsonSerializer.Serialize(strBody);
                switch (pMethod.Method)
                {
                    case "POST":
                        HttpContent httpContent = new StringContent(strBody, System.Text.Encoding.UTF8, "application/json");
                        httpRequestMessage.Content = httpContent;
                        break;

                }

            }
            catch (System.Exception ex)
            {
                var poo = "{0} Exception caught: " + ex;
            }
            return await _Client.SendAsync(httpRequestMessage);

        }
    }
}