using System;
using System.IO;
using System.Text;
using AuthHost.WEB.Authentication;
using AuthHost.WEB.Authentication.Middlewares;
using AuthHost.WEB.Filters;
using AuthHost.WEB.Infrastructure.DI;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;

namespace AuthHost.WEB
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true)
                .AddEnvironmentVariables();

            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Elasticsearch().WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri("http://localhost:9200"))
                {
                    MinimumLogEventLevel = LogEventLevel.Information,
                    AutoRegisterTemplate = true,
                    IndexFormat = "test-{0:yyyy.MM}"
                })
                .CreateLogger();

            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            DependencyResolver.Resolve(services, Configuration);
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services
                .Configure<TokenProviderOptions>(Configuration.GetSection("TokenProviderOptions"))
                .Configure<TokenProviderOptions>(options =>
                {
                    var signingKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(options.SecretKey));
                    options.SigningCredentials =
                        new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
                });

            services
                .Configure<TokenValidationParameters>(Configuration.GetSection("TokenValidationParameters"))
                .Configure<TokenValidationParameters>(options =>
                {
                    var signingKey = new SymmetricSecurityKey(
                        Encoding.ASCII.GetBytes(Configuration["TokenValidationParameters:SecretKey"]));
                    options.IssuerSigningKey = signingKey;
                });

            services.AddMvc(options =>
            {
                options.Filters.Add(typeof(ErrorFilter));
            });

            services.AddAutoMapper();
        }

        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IOptions<TokenProviderOptions> tokenProviderOptions,
            IOptions<TokenValidationParameters> tokenValidationOptions)
        {

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();


            loggerFactory.AddSerilog();


            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseMiddleware<TokenProviderMiddleware>(tokenProviderOptions);

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationOptions.Value
            });

            app.UseMvc();

        }
    }
}