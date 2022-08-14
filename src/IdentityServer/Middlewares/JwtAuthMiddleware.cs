using IdentityServer.Authorizations;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace IdentityServer
{
    /// <summary>
    /// 中间件
    /// 先做检查 header token的使用
    /// </summary>
    public class JwtTokenAuthMiddleware
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly RequestDelegate _next;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="next"></param>
        public JwtTokenAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }


        private void PreProceed(HttpContext next)
        {
            //..todo 处理请求前逻辑
        }
        private void PostProceed(HttpContext next)
        {
            //..todo 请求处理中逻辑
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpContext"></param>
        /// <returns></returns>
        public Task Invoke(HttpContext httpContext)
        {
            PreProceed(httpContext);
            //检测是否包含'Authorization'请求头
            if (!httpContext.Request.Headers.ContainsKey("Authorization"))
            {
                //如果不包含，则跳过进入下一个中间件
                PostProceed(httpContext);

                return _next(httpContext);
            }
            try
            {
                //解析token时，不需要Bearer字符
                var tokenHeader = httpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (tokenHeader.Length >= 128)
                {
                    //Console.WriteLine($"{DateTime.Now} token :{tokenHeader}");
                    TokenModelJwt tm = JwtTokenHandler.Serialize(tokenHeader);

                    //授权
                    //var claimList = new List<Claim>();
                    //var claim = new Claim(ClaimTypes.Role, tm.Role);
                    //claimList.Add(claim);
                    //var identity = new ClaimsIdentity(claimList);
                    //var principal = new ClaimsPrincipal(identity);
                    //httpContext.User = principal;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine($"{DateTime.Now} middleware wrong:{e.Message}");
            }
            PostProceed(httpContext);
            return _next(httpContext);
        }

    }
}
