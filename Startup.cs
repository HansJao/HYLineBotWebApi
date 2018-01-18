using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HYLineBotWebApi.DataClass;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HYLineWebApi
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
            var configManager = new ConfigManager();
            Configuration.GetSection("Config").Bind(configManager);
            ConfigProvider.ConnectionString = configManager.ConnectionString;
            ConfigProvider.ChannelAccessToken = configManager.ChannelAccessToken;
            ConfigProvider.UserIDList = configManager.UserIDList;
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // if (env.IsDevelopment())
            // {
                app.UseDeveloperExceptionPage();
            // }

            app.UseMvc();
        }
    }
}
