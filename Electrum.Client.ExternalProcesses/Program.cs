using Electrum.Communication.gRPC.Worker;
using Electrum.Core;
using Electrum.Core.Execution;
using Electrum.Store.InMemory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Electrum.Client.ExternalProcesses
{
    internal class Program
    {

        public static List<JobConfiguration> Configurations = new List<JobConfiguration>();

        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            Configurations.Add(new JobConfiguration
            {
                Namespace = "testing_namespace",
                Name = "test_job",
                ProcessPath = "C:\\Windows\\System32\\cmd.exe",
                Args = "/c ipconfig"
            });
            var services = new ServiceCollection();
            services.UseElectron(x => x.AsClient(y => y.WithJobDiscoveryService<ConfigJobDiscoveryService>()));
            services.UseElectronInMemoryStore();
            services.AddDistributedMemoryCache();
            services.AddLogging(builder => builder.AddSerilog(dispose: true));
            var serviceProvider = services.BuildServiceProvider();
            var grpcClient = new ElectrumGRPCJobWorker("http://localhost:5002", "TestClient", 3, "some-random-key", serviceProvider.GetService<JobExecutorService>());
            grpcClient.StartListener().Wait();
        }
    }
}