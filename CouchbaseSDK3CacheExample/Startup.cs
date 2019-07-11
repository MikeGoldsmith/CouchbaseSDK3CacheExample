using Couchbase;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CouchbaseSDK3CacheExample
{
    public class Startup
    {
        public Startup(Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public Microsoft.Extensions.Configuration.IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var configuration = new Configuration()
                .WithServers("couchbase://10.112.193.101")
                .WithCredentials("Administrator", "password")
                .WithBucket("default");

            var cluster = new Cluster(configuration);
            var bucket = cluster.Bucket("default").Result;
            var collection = bucket.DefaultCollection.Result;

            services.AddSingleton<ICluster>(cluster);
            services.AddSingleton<IBucket>(bucket);
            services.AddSingleton<ICollection>(collection);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
