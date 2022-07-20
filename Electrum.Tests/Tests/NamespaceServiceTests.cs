using Electrum.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Electrum.Tests.Tests
{
    public class NamespaceServiceTests
    {

        public IElectrumNamespaceService NamespaceService => TestConfigurator.ServiceProvider.GetService<IElectrumNamespaceService>();

        public NamespaceServiceTests(ITestOutputHelper output)
        {
            TestConfigurator.ConfigureLogger<NamespaceServiceTests>(output);
        }

        [Fact]
        public void CanGetNamespaceService()
        {
            Assert.NotNull(NamespaceService);
        }

        [Fact]
        public void CanCreateNamespace()
        {
            var id = Guid.NewGuid().ToString();
            var ns = NamespaceService.CreateNamespace(id);
            Assert.NotNull(ns);
            Assert.Equal(id, ns.Name);
            Assert.NotEqual(Guid.Empty, ns.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(200)]
        [InlineData(500)]
        public void CanCreateNestedNamespaces(int count)
        {
            var names = new List<string>();
            string currName = Guid.NewGuid().ToString();
            for (int i = 1; i < count; i++)
            {
                names.Add(currName);
                currName += "/" + Guid.NewGuid().ToString();
            }
            names.Add(currName);
            var bottomLevelNs = NamespaceService.CreateNamespace(currName);
            Assert.NotNull(bottomLevelNs);
            Assert.Equal(currName, bottomLevelNs.Name);
            foreach (var name in names)
            {
                var ns = NamespaceService.GetNamespace(name);
                Assert.NotNull(ns);
                Assert.Equal(name, ns.Name);
            }
        }

    }
}
