using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Voting.Web.Hubs;

namespace Voting.Web
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
            services.AddMvc();
            services.AddSignalR();
            services.AddSingleton<HttpClient>();

            var redisConnectionString = Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
            if (string.IsNullOrEmpty(redisConnectionString))
            {
                services.AddSingleton<IVotingService, ApiVotingService>();
            }
            else
            {
                var redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
                services.AddSingleton<ConnectionMultiplexer>(redisConnection);
                services.AddTransient<IVotingService, RedisVotingService>();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();

            app.UseSignalR(builder => 
            {
                builder.MapHub<VotingHub>("/votinghub");
            });

            app.UseMvc();

        }
    }
}
