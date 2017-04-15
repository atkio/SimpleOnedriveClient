using Microsoft.Graph;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SimpleOnedriveClient
{
    class AuthHelper
    {
        public static bool TryRefreshToken()
        {
            try
            {
                AuthStore aus = AuthStore.Instance;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://login.microsoftonline.com");
                    var content = new FormUrlEncodedContent(new[]
                    {
                             new KeyValuePair<string, string>("client_id", aus.client_id),
                             new KeyValuePair<string, string>("redirect_uri", aus.redirect_uri),
                             new KeyValuePair<string, string>("client_secret", aus.client_secret),
                             new KeyValuePair<string, string>("refresh_token", aus.refresh_token),
                             new KeyValuePair<string, string>("grant_type", "refresh_token")
                        });
                    var result = client.PostAsync("/common/oauth2/v2.0/token", content).Result;
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    var authresult = JObject.Parse(resultContent);
                    aus.refresh_token = authresult["refresh_token"].ToString();
                    aus.access_token = authresult["access_token"].ToString();
                    aus.save();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static GraphServiceClient GetClient()
        {
            return 
            new GraphServiceClient(
                      "https://graph.microsoft.com/v1.0",
                      new DelegateAuthenticationProvider(
                          (requestMessage) =>
                          {

                              requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", AuthStore.Instance.access_token);
                               
                              return Task.FromResult(0);
                          }));

        }
        public static bool GetToken(string client_id,string client_secret,string redirect_uri,string code)
        {
            try
            {
                AuthStore aus = AuthStore.Instance;
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri("https://login.microsoftonline.com");
                    var content = new FormUrlEncodedContent(new[]
                    {
                             new KeyValuePair<string, string>("client_id", client_id),
                             new KeyValuePair<string, string>("redirect_uri", redirect_uri),
                             new KeyValuePair<string, string>("client_secret", client_secret),
                             new KeyValuePair<string, string>("code", code),
                             new KeyValuePair<string, string>("grant_type", "authorization_code")
                        });
                    var result = client.PostAsync("/common/oauth2/v2.0/token", content).Result;
                    string resultContent = result.Content.ReadAsStringAsync().Result;
                    var authresult = JObject.Parse(resultContent);
                    aus.refresh_token = authresult["refresh_token"].ToString();
                    aus.access_token = authresult["access_token"].ToString();
                    aus.client_id = client_id;
                    aus.client_secret = client_secret;
                    aus.redirect_uri = redirect_uri;
                    aus.scope = "files.readwrite+offline_access";
                    aus.save();
                }
                return true;
            }
            catch
            {
                return false;
            }

        }

    }
    class AuthStore
    {
        public string client_id { get; set; }
        public string scope { get; set; }
        public string redirect_uri { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string client_secret { get; set; }


        public const string Define = "AuthStore.json";
        private static object syncRoot = new Object();

        public static AuthStore Instance
        {
            get
            {
                if (!System.IO.File.Exists(Define))
                {
                    System.IO.File.WriteAllText(Define,
                     JsonConvert.SerializeObject(new AuthStore(),
                     Formatting.Indented));
                }

                if (_Instance == null)
                    lock (syncRoot)
                    {
                        if (_Instance == null)
                            _Instance = JsonConvert.DeserializeObject<AuthStore>(
                           System.IO.File.ReadAllText(Define));
                    }
               
                return _Instance;
            }
        }

        private static AuthStore _Instance = null;

        public void save()
        {
            System.IO.File.WriteAllText(Define,
                    JsonConvert.SerializeObject(this,
                    Formatting.Indented));
        }
    }
}
