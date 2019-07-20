using System;
using System.Net.Http;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace ClientSample
{
    class Program
    {
        static void Main(string[] args)
        {

            HttpClient httpClient = new HttpClient();
          var discoverInfo= httpClient.GetDiscoveryDocumentAsync("http://localhost:5000").Result;
            if (discoverInfo.IsError)
            {
                Console.WriteLine(discoverInfo.Error);
            }
            else
            {
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(discoverInfo));
            }

          var authorResponse=  httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest() {
                Address=discoverInfo.TokenEndpoint,
                ClientId="client",
                ClientSecret="secret",
                Scope="api1"
            }).Result;
            if (authorResponse.IsError)
            {
                Console.WriteLine(authorResponse.Error);
            }
            else
            {
                Console.WriteLine($"token:{authorResponse.AccessToken}");
            }

            HttpClient apiHttpClient = new HttpClient();
            apiHttpClient.SetBearerToken(authorResponse.AccessToken);
            var apiResponse = apiHttpClient.GetAsync("http://localhost:5001/api/IdentityTest/get").Result;
            if (!apiResponse.IsSuccessStatusCode)
            {
                Console.WriteLine(apiResponse.StatusCode);
            }
            else
            {
                Console.WriteLine(JArray.Parse(apiResponse.Content.ReadAsStringAsync().Result));
            }

        }
    }
}
