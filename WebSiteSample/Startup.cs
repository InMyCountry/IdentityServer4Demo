using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using static IdentityModel.OidcConstants;

namespace WebSiteSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // 关闭Jwt的Claim类型映射，以便允许 well-known claims (e.g. ‘sub’ and ‘idp’) 
            // 如果不关闭就会修改从授权服务器返回的 Claim
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // 2) 将身份验证服务添加到DI
            services.AddAuthentication(options =>
            {
                // 使用cookie来本地登录用户（通过DefaultScheme = "Cookies"）
                options.DefaultScheme = "Cookies";
                // 设置 DefaultChallengeScheme = "oidc" 时，表示我们使用 OIDC 协议
                options.DefaultChallengeScheme = "oidc";
            })
            // 我们使用添加可处理cookie的处理程序
            .AddCookie("Cookies")
            // 配置执行OpenID Connect协议的处理程序
            .AddOpenIdConnect("oidc", options =>
            {
                // 
                options.SignInScheme = "Cookies";
                // 表明我们信任IdentityServer客户端
                options.Authority = "http://localhost:5000";
                // 表示我们不需要 Https
                options.RequireHttpsMetadata = false;
                // 用于在cookie中保留来自IdentityServer的 token，因为以后可能会用
                options.SaveTokens = true;

                options.ClientId = "hybrid client";
                options.ClientSecret = "hybrid secret";
                options.ResponseType = "code id_token"; // Authorization Code

                options.Scope.Clear();
                options.Scope.Add("api1");
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("address");
                options.Scope.Add("phone");
                options.Scope.Add("email");
                options.Scope.Add("roles");
                options.Scope.Add("locations");
                // Scope中添加了OfflineAccess后，就可以在 Action 中获得 refreshToken
                options.Scope.Add(StandardScopes.OfflineAccess);

                // 集合里的东西 都是要被过滤掉的属性，nbf amr exp...
                options.ClaimActions.Remove("nbf");
                options.ClaimActions.Remove("amr");
                options.ClaimActions.Remove("exp");

                // 不映射到User Claims里
                options.ClaimActions.DeleteClaim("sid");
                options.ClaimActions.DeleteClaim("sub");
                options.ClaimActions.DeleteClaim("idp");

                // 让Claim里面的角色成为mvc系统识别的角色
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.Name,
                    RoleClaimType = JwtClaimTypes.Role
                };

            });

            services.AddAuthorization(action => {
                action.AddPolicy("roleTest", bulider => {
                    //要求登陆
                    bulider.RequireAuthenticatedUser();
                    bulider.RequireRole("管理员");
                });


            });

        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            // 管道中加入身份验证功能
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseMvcWithDefaultRoute();
        }
    }
}
