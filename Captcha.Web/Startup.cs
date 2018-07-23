using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Linq;
using Tirskele.Captcha.CaptchaProvider.Factory;
using Tirskele.Captcha.CaptchaProvider.Providers;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tirskele.Captcha.Repositories;

namespace Triskele.Captcha.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; protected set; }
        public Startup(IConfiguration configuration)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            Configuration = configurationBuilder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {


            services.AddCors();
            
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            
            services.AddSingleton<IConfigurationSection>(Configuration.GetSection("Captcha"));

            var sp = services.BuildServiceProvider();

            var genTypeName = Configuration.GetValue<string>("Captcha:GeneratorType");
            if (string.IsNullOrEmpty(genTypeName))
            {
                services.AddSingleton<ICaptchaGenerator, EnglishAlphaNumericProvider>();
            }
            else
            {
                var type = Type.GetType(genTypeName, true);
                ICaptchaGenerator generator = (ICaptchaGenerator)ActivatorUtilities.CreateInstance(sp, type);
                services.AddSingleton<ICaptchaGenerator>(generator);
            }

            var repoTypeName = Configuration.GetValue<string>("Captcha:RepositoryType");
            if (string.IsNullOrEmpty(repoTypeName))
            {
                services.AddSingleton<ICaptchaRepository, CaptchaInMemoryRepository>();
            }
            else
            {
                var type = Type.GetType(repoTypeName, true);
                ICaptchaRepository repository = (ICaptchaRepository)ActivatorUtilities.CreateInstance(sp, type);
                services.AddSingleton<ICaptchaRepository>(repository);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            string environment = "PRD";
            if (env.IsDevelopment())
            {
                environment = "DEV";
            }

            if (env.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            var allowedCaptchaDomains = Configuration.GetValue<string>("Captcha:AllowedCaptchaDomains");
            if (string.IsNullOrEmpty(allowedCaptchaDomains))
            {
                app.UseCors(builder => builder.AllowAnyHeader().WithMethods("GET").AllowCredentials().AllowAnyOrigin());
            }
            else
            {
                var allowedOrigins = allowedCaptchaDomains.Split(';').AsEnumerable();
                app.UseCors(builder =>
                    builder.AllowAnyHeader().WithMethods("GET").AllowCredentials()
                        .SetIsOriginAllowed(o => allowedOrigins.Contains(o)));
            }

            app.UseMvc(); 

        }
    }
}
