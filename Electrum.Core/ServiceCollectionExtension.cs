using Electrum.Core.Discovery;
using Electrum.Core.Distribution;
using Electrum.Core.Execution;
using Electrum.Core.Logging;
using Electrum.Core.Services;
using Electrum.Core.Services.Implementations;
using Electrum.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Electrum.Core
{
    public static class ServiceCollectionExtension
    {

        public static IServiceCollection UseElectron(this IServiceCollection services, Action<ElectrumConfiguration> options)
        {
            services.AddScoped<ElectrumJobManager>();
            services.AddScoped<ElectrumObjectRepositoryFactory>();
            var config = new ElectrumConfiguration(services);
            options(config);
            return services;
        }

    }

    public class ElectrumConfiguration
    {
        public IServiceCollection ServiceCollection { get; set; }
        public ElectrumConfiguration(IServiceCollection services)
        {
            ServiceCollection = services;
        }

        public void AsServer()
        {
            ServiceCollection.AddScoped<IJobSchedulerService, JobSchedulerService>();
            ServiceCollection.AddScoped<IElectrumNamespaceService, ElectrumNamespaceService>();
            ServiceCollection.AddScoped<JobLog>();
            ServiceCollection.AddScoped<JobDistributionService>();
        }

        public void AsClient(Action<ElectrumClientConfiguration> options)
        {
            ServiceCollection.AddScoped<JobExecutorService>();
            var conf = new ElectrumClientConfiguration(ServiceCollection);
            options(conf);
            conf.ApplyDefaults();
        }





        public class ElectrumClientConfiguration
        {
            private bool UseDefaultDiscoveryService = true;
            private IServiceCollection services;

            public ElectrumClientConfiguration(IServiceCollection services)
            {
                this.services = services;
            }
            
            public ElectrumClientConfiguration WithJobDiscoveryService<T>() where T : class, IJobDiscoveryService
            {
                UseDefaultDiscoveryService = false;
                services.AddScoped<IJobDiscoveryService, T>();
                return this;
            }

            internal void ApplyDefaults()
            {
                if(UseDefaultDiscoveryService)
                {
                    services.AddScoped<IJobDiscoveryService, ElectrumJobDiscoveryService>();
                }
            }

        }
    }
}
