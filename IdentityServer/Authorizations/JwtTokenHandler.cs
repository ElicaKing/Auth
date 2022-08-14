using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IdentityServer.Authorizations
{
    public class JwtTokenHandler
    {
        /// <summary>
        /// 颁发token
        /// </summary>
        /// <param name="tokenModel"></param>
        /// <returns></returns>
        public static string IssueJwt(TokenModelJwt tokenModel)
        {
            // 自己封装的 appsettign.json 操作类，看下文
            string iss = Appsettings.app(new string[] { "Audience", "Issuer" });
            string aud = Appsettings.app(new string[] { "Audience", "Audience" });
            string secret = Appsettings.app(new string[] { "Audience", "Secret" });

            var claims = new List<Claim>
          {
            //uid通常为唯一标识
            new Claim(JwtRegisteredClaimNames.Jti, tokenModel.Uid.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, $"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}"),
            new Claim(JwtRegisteredClaimNames.Nbf,$"{new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds()}") ,
            //这个就是过期时间，目前是过期1000秒，可自定义，注意JWT有自己的缓冲过期时间
            new Claim (JwtRegisteredClaimNames.Exp,$"{new DateTimeOffset(DateTime.Now.AddSeconds(5000)).ToUnixTimeSeconds()}"),
            new Claim(JwtRegisteredClaimNames.Iss,iss),
            new Claim(JwtRegisteredClaimNames.Aud,aud),

            new Claim(ClaimTypes.Role,tokenModel.Role),//为了解决一个用户多个角色(比如：Admin,System)，用下边的方法
           };

            // 可以将一个用户的多个角色全部赋予；
            // 作者：DX 提供技术支持；
            claims.AddRange(tokenModel.Role.Split(',').Select(s => new Claim(ClaimTypes.Role, s)));

            //秘钥 (SymmetricSecurityKey 对安全性的要求，密钥的长度一般256位以上)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);//jwt的加密部分，采用hs256

            //这里是jwt的载体部分，以及加密方式
            var jwt = new JwtSecurityToken(
                issuer: iss,
                claims: claims,
                signingCredentials: creds);

            var jwtHandler = new JwtSecurityTokenHandler();

            //这个系统内部会自动加入jwt的头部{"alg": "HS256","typ": "JWT"} 加入进行生成
            var encodedJwt = jwtHandler.WriteToken(jwt);

            return encodedJwt;
        }
        /// <summary>
        /// 我颁发了token令牌，需要对应的去识别令牌
        /// </summary>
        /// <param name="jwt"></param>
        /// <returns></returns>
        public static TokenModelJwt Serialize(string jwt)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = jwtHandler.ReadJwtToken(jwt);
            object role;

            try
            {
                securityToken.Payload.TryGetValue(ClaimTypes.Role, out role);
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                throw;
            }
            var tm = new TokenModelJwt
            {
                Uid = (securityToken.Id).ObjToInt(),
                Role = role?.ObjToString() ?? String.Empty,
            };
            return tm;
        }
        
    }
    /// <summary>
    /// 令牌实体
    /// </summary>
    public class TokenModelJwt
    {
        /// <summary>
        /// Id
        /// </summary>
        public long Uid { get; set; }
        /// <summary>
        /// 角色
        /// </summary>
        public string Role { get; set; }
        /// <summary>
        /// 职能
        /// </summary>
        public string Work { get; set; }

    }
}
