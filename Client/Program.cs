using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            // 从元数据中发现端点
            var client = new HttpClient();
            var disco =  client.GetDiscoveryDocumentAsync("https://localhost:5001").Result;
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }
            // 请求令牌
            var tokenResponse = client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,

                ClientId = "client",
                ClientSecret = "secret",
                Scope = "api1"
            }).Result;

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                //return;
            }
            Console.WriteLine(tokenResponse.Json);

            // 调用api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response =  apiClient.GetAsync("https://localhost:6001/identity").Result;
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content =  response.Content.ReadAsStringAsync().Result;
                Console.WriteLine(JArray.Parse(content));
            }
            Console.WriteLine("Hello World!");
        }
    }
}
