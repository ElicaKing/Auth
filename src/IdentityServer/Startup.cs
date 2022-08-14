using IdentityServer4.Services;
using IdentityServer4.Validation;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System;
using System.IO;
using System.Text;

namespace IdentityServer
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment,IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // uncomment, if you want to add an MVC-based UI
            services.AddControllers();
            services.AddControllersWithViews();
            var baseapp = AppContext.BaseDirectory;
            var xmlPath = Path.Combine(baseapp, "LittleElm.Blog.Api.xml");
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "桑小榆呀",
                    Version = "v1",
                    Description = "框架说明文档",
                    Contact = new OpenApiContact
                    {
                        Email = "elicaliu@163.com",
                        Name = "桑小榆呀",
                        Url = new Uri("https://mp.weixin.qq.com/s/lSipKntoBPBUn6v2wRkVXA")
                    }
                }
                );
                var baseapp = AppContext.BaseDirectory;
                var xmlPath = Path.Combine(baseapp, "IdentityServer.xml");
                c.IncludeXmlComments(xmlPath);
                c.OperationFilter<AddResponseHeadersFilter>();
                c.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                c.OperationFilter<SecurityRequirementsOperationFilter>();
           
                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT授权(数据将在请求头中进行传输) 直接在下框中输入Bearer {token}（注意两者之间是一个空格）",
                    Name = "Authorization",//jwt默认的参数名称
                    In = ParameterLocation.Header,//jwt默认存放Authorization信息的位置(请求头中)
                    Type = SecuritySchemeType.ApiKey
                });
            });
            services.AddSingleton(new Appsettings(Configuration));

            //此处使用jwt 认证方式
            services.AddAuthentication(x =>
            {
                //声明cheme格式，就是{Bearer xx}
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })// 也可以直接写字符串，AddAuthentication("Bearer")
              .AddJwtBearer(o =>
              {
                  //读取配置文件

                  var audienceConfig = Configuration.GetSection("Audience:Audience");
                  var symmetricKeyAsBase64 = Configuration.GetSection("Audience:Secret");
                  var keyByteArray = Encoding.ASCII.GetBytes(symmetricKeyAsBase64.Value);
                  var signingKey = new SymmetricSecurityKey(keyByteArray);

                  var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                  o.TokenValidationParameters = new TokenValidationParameters
                  {
                      ValidateIssuerSigningKey = true,
                      IssuerSigningKey = signingKey,//参数配置在下边
                      ValidateIssuer = true,
                      ValidIssuer = symmetricKeyAsBase64.Value,//发行人
                      ValidateAudience = true,
                      ValidAudience = audienceConfig.Value,//订阅人
                      ValidateLifetime = true,
                      ClockSkew = TimeSpan.FromSeconds(200),//这个是缓冲过期时间，也就是说，即使我们配置了过期时间，这里也要考虑进去，过期时间+缓冲，默认好像是7分钟，你可以直接设置为0
                      RequireExpirationTime = true,
                  };
              });
            //配置identityServer
            //var builder = services.AddIdentityServer()
            //    .AddInMemoryIdentityResources(Config.IdentityResources)
            //    .AddDeveloperSigningCredential()        //这仅适用于没有证书可以使用的开发场景。
            //    .AddInMemoryApiScopes(Config.ApiScopes)
            //    .AddInMemoryClients(Config.Clients)
            //    .AddTestUsers(TestUsers.Users);

            // not recommended for production - you need to store your key material somewhere secure
            //builder.AddDeveloperSigningCredential();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                // 根据版本名称倒序 遍历展示
                var ApiName = "IdentityServer";//这里你可以从appsettings.json中获取，比如我封装了一个类Appsettings.cs，具体查看我的源代码
                var version = "v1";
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{ApiName} {version}");
                    c.RoutePrefix = "";//help
                    //c.IndexStream = () =>
                    //GetType().GetTypeInfo().Assembly.GetManifestResourceStream("LittleElm.Blog.Api.index.html");
                });
            }

            // uncomment if you want to add MVC
            app.UseStaticFiles();
            app.UseRouting();

            //app.UseIdentityServer();

            //先开启认证，
            app.UseAuthentication();
            //再开始授权中间件
            app.UseAuthorization();
            app.UseMiddleware<JwtTokenAuthMiddleware>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
