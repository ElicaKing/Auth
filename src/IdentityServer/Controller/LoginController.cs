using IdentityModel.Client;
using IdentityServer.Authorizations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Controller
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpGet("token")]
        public async Task<IActionResult> GetToken()
        {
            // discover endpoints from metadata
            var client = new HttpClient();

            var disco = await client.GetDiscoveryDocumentAsync("https://localhost:5001");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return Ok(disco.Error);
            }

            // request token
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = "client",
                ClientSecret = "secret",

                Scope = "api1"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return Ok(tokenResponse.Error);
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var apiClient = new HttpClient();
            apiClient.SetBearerToken(tokenResponse.AccessToken);

            var response = await apiClient.GetAsync("http://localhost:6001/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine(JArray.Parse(content));
                return Ok(JArray.Parse(content));
            }
            return Ok();
        }
        [HttpGet]
        public async Task<IActionResult> GetToken(string name, string password)
        {
            string jwtStr = string.Empty;
            bool suc = false;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                jwtStr = "login fail !";
            }
            //此处可以添加自己的业务场景，通过db验证用户合法逻辑
            //todo

            jwtStr = JwtTokenHandler.IssueJwt(new TokenModelJwt 
            { Uid = 1, Role = "Admin",Work ="touch fish" });
            suc = true;
            return Ok(new
            {
                success = suc,
                msg = suc ? "获取成功" : "获取失败",
                response = jwtStr
            });
        }
        [HttpGet("encodejwt")]
        public async Task<IActionResult> GetEncodeJwt(string jwt)
        {
            bool suc = false;
            var jwtStr = JwtTokenHandler.Serialize(jwt);
            suc = true;
            return Ok(new
            {
                success = suc,
                msg = suc ? "获取成功" : "获取失败",
                response = jwtStr
            });
        }
    }
}
