using Electrum.Core;
using Electrum.Store.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Electrum.Tests
{
    public class TestConfigurator
    {
        static IServiceProvider? serviceProvider;

        public static IServiceProvider ServiceProvider
        {
            get { 
                if (serviceProvider == null)
                {
                    var services = new ServiceCollection();
                    services.UseElectron(x => x.AsServer());
                    services.UseElectronInMemoryStore();
                    services.AddDistributedMemoryCache();
                    services.AddLogging(builder => builder.AddSerilog(dispose: true));
                    serviceProvider = services.BuildServiceProvider();
                }
                return serviceProvider;
            }
        }

        public static void ConfigureLogger<T>(ITestOutputHelper output)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.TestOutput(output, Serilog.Events.LogEventLevel.Verbose)
            .CreateLogger()
            .ForContext<T>();
            Log.Information("Configured Logging!");
        }

        public static ElectrumNamespace Namespace = new ElectrumNamespace
        {
            Id = System.Guid.NewGuid(),
            Name = "Testing"
        };

    }
}
