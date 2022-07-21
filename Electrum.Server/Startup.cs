using Electrum.Core;
using Electrum.Store.InMemory;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Electrum.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.UseElectron(options => options.AsServer());
            services.UseElectronInMemoryStore();
            services.AddDistributedMemoryCache();
            services.AddGrpc();
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGrpcService<Electrum.Communication.gRPC.Worker.Server.GrpcJobExecutionClient>();
            });
        }
    }
}