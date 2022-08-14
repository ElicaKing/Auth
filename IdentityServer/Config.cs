
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace IdentityServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile()
            };
        public static IEnumerable<ApiResource> GetApis =>
         new List<ApiResource>
            {
                new ApiResource("api","My API",new[] {JwtClaimTypes.Subject,JwtClaimTypes.Email,JwtClaimTypes.Name,JwtClaimTypes.Role,JwtClaimTypes.PhoneNumber })
            };
        public static IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("api", "My API")
        };

        public static IEnumerable<Client> Clients =>
        new List<Client>
        {
            new Client
            {
                ClientId = "client",

                // 没有交互式用户，使用 clientid/secret 进行身份验证
                AllowedGrantTypes = GrantTypes.ClientCredentials,

                // 用于身份验证的密钥
                ClientSecrets =
                {
                    new Secret("secret".Sha256())
                },

                // 客户端有权访问的范围
                AllowedScopes = { "api" }
        },
        // 交互式 ASP.NET Core MVC 客户端
        new Client
        {
            ClientId = "mvc",
            ClientSecrets = { new Secret("secret".Sha256()) },

            AllowedGrantTypes = GrantTypes.Code,
            RequireConsent = true,
            // 登录后重定向到哪里
            RedirectUris = { "https://localhost:7001/signin-oidc" },
            
            // 注销后重定向到哪里
            PostLogoutRedirectUris = { "https://localhost:7001/signout-callback-oidc" },

            AllowedScopes = new List<string>
            {
                IdentityServerConstants.StandardScopes.OpenId,
                IdentityServerConstants.StandardScopes.Profile
            }
        }
    };
    }
}