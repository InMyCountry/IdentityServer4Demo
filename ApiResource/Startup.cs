﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ApiResource
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
          .AddAuthorization()
          .AddJsonFormatters();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    // IdentityServer的地址
                    options.Authority = "http://localhost:5000";
                    options.ApiName = "api1";
                    options.ApiSecret = "api1 secret";
                });

            //services.AddAuthentication("Bearer")
            //    .AddJwtBearer("Bearer", options =>
            //    {
            //        // IdentityServer的地址
            //        options.Authority = "http://localhost:5000";
            //        // 不需要Https
            //        options.RequireHttpsMetadata = false;
            //        // 和资源名称相对应
            //        options.Audience = "api1";
            //    });

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}
