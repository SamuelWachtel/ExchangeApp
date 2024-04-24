using Serilog;
using System.Configuration;
using System.Net.Http.Json;

namespace Network
{
    public class IdentityServiceClient
    {
        public static async Task<string> GetAccessToken()
        {
            string username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["password"];

            string tokenUrl = "";
            string chosenCountry = ConfigurationManager.AppSettings["chosenCountry"];
            if (chosenCountry.ToUpper() == "KENYA")
            {
                tokenUrl = ConfigurationManager.AppSettings["identityServceKenya"];

            }
            else if (chosenCountry.ToUpper() == "OMAN")
            {
                tokenUrl = ConfigurationManager.AppSettings["identityServceOman"];
            }
            else
            {
                Log.Information("Invalid country");
            }


            using (var client = new HttpClient())
            {
                var parameters = new
                {
                    client_id = username,
                    client_secret = password,
                    grant_type = "client_credentials"
                };

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("client_id", username),
                    new KeyValuePair<string, string>("client_secret", password),
                    new KeyValuePair<string, string>("grant_type", "client_credentials")
                });

                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                    string accessToken = tokenResponse.access_token;
                    return accessToken;
                }
                else
                {
                    throw new Exception("Failed to obtain access token. Status code: " + response.StatusCode);
                }
            }
        }
    }

    public class TokenResponse
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string scope { get; set; }
    }
}