using Electrum.Core;
using Electrum.Core.Store;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Xunit;

namespace Electrum.Tests
{
    public class ObjectRepositoryTests
    {

        public ElectrumObjectRepositoryFactory RepositoryFactory => TestConfigurator.ServiceProvider.GetService<ElectrumObjectRepositoryFactory>();

        [Fact]
        public void CanGetRepositoryFactory()
        {
            Assert.NotNull(RepositoryFactory);
        }

        [Fact]
        public void CanCreateRepository()
        {
            var repo = RepositoryFactory.GetRepo<ElectrumJob>();
            Assert.NotNull(repo);
        }

        [Fact]
        public void CanPutItemIntoRepository()
        {
            var repo = RepositoryFactory.GetRepo<ElectrumJob>();
            Assert.NotNull(repo);
            var testItem = new ElectrumJob
            {
                Id = System.Guid.NewGuid(),
                Namespace = TestConfigurator.Namespace
            };
            var addedItem = repo.Add(testItem);
            Assert.NotNull(addedItem);
            Assert.Equal(testItem.Id, addedItem.Id);
            Assert.Equal(testItem.Namespace, addedItem.Namespace);
        }

        [Fact]
        public void CanGetItemFromRepository()
        {
            var repo = RepositoryFactory.GetRepo<ElectrumJob>();
            Assert.NotNull(repo);
            var testItem = new ElectrumJob
            {
                Id = System.Guid.NewGuid(),
                Namespace = TestConfigurator.Namespace
            };
            var addedItem = repo.Add(testItem);
            Assert.NotNull(addedItem);
            Assert.Equal(testItem.Id, addedItem.Id);
            Assert.Equal(testItem.Namespace, addedItem.Namespace);

            // Get the item back
            var item = repo.FirstOrDefault(x => x.Id == testItem.Id);
            Assert.NotNull(item);
            Assert.Equal(testItem.Id, item.Id);
            Assert.Equal(testItem.Namespace, item.Namespace);
            Assert.Equal(testItem.Namespace.Id, item.Namespace.Id);
        }

        [Fact]
        public void CanPutMultipleItemsIntoRepository()
        {
            var repo = RepositoryFactory.GetRepo<ElectrumJob>();
            Assert.NotNull(repo);
            var testItem1 = new ElectrumJob
            {
                Id = System.Guid.NewGuid(),
                Namespace = TestConfigurator.Namespace
            };
            var addedItem1 = repo.Add(testItem1);
            Assert.NotNull(addedItem1);
            Assert.Equal(testItem1.Id, addedItem1.Id);
            Assert.Equal(testItem1.Namespace, addedItem1.Namespace);
            var testItem2 = new ElectrumJob
            {
                Id = System.Guid.NewGuid(),
                Namespace = TestConfigurator.Namespace
            };
            var addedItem2 = repo.Add(testItem2);
            Assert.NotNull(addedItem2);
            Assert.Equal(testItem2.Id, addedItem2.Id);
            Assert.Equal(testItem2.Namespace, addedItem2.Namespace);

            // Get the item back
            var item = repo.FirstOrDefault(x => x.Id == testItem1.Id);
            Assert.NotNull(item);
            Assert.Equal(testItem1.Id, item.Id);
            Assert.Equal(testItem1.Namespace, item.Namespace);
            Assert.Equal(testItem1.Namespace.Id, item.Namespace.Id);

            // Get the item back
            item = repo.FirstOrDefault(x => x.Id == addedItem2.Id);
            Assert.NotNull(item);
            Assert.Equal(testItem2.Id, item.Id);
            Assert.Equal(testItem2.Namespace, item.Namespace);
            Assert.Equal(testItem2.Namespace.Id, item.Namespace.Id);
        }

        [Fact]
        public void CanPutMultipleItemsIntoDifferentRepository()
        {
            var jobRepo = RepositoryFactory.GetRepo<ElectrumJob>();
            var namespaceRepo = RepositoryFactory.GetRepo<ElectrumNamespace>();
            Assert.NotNull(jobRepo);
            var testItem1 = new ElectrumJob
            {
                Id = System.Guid.NewGuid(),
                Namespace = TestConfigurator.Namespace
            };
            var addedItem1 = jobRepo.Add(testItem1);
            Assert.NotNull(addedItem1);
            Assert.Equal(testItem1.Id, addedItem1.Id);
            Assert.Equal(testItem1.Namespace, addedItem1.Namespace);
            var testItem2 = new ElectrumNamespace
            {
                Id = System.Guid.NewGuid(),
                Name = "Test"
            };
            var addedItem2 = namespaceRepo.Add(testItem2);
            Assert.NotNull(addedItem2);
            Assert.Equal(testItem2.Id, addedItem2.Id);
            Assert.Equal(testItem2.Name, addedItem2.Name);

            // Get the item back
            var item = jobRepo.FirstOrDefault(x => x.Id == testItem1.Id);
            Assert.NotNull(item);
            Assert.Equal(testItem1.Id, item.Id);
            Assert.Equal(testItem1.Namespace, item.Namespace);
            Assert.Equal(testItem1.Namespace.Id, item.Namespace.Id);

            // Get the item back
            var retrievedNamespaceFromJobRepo = jobRepo.FirstOrDefault(x => x.Id == addedItem2.Id);
            Assert.Null(retrievedNamespaceFromJobRepo);
            var retrievedNamespace = namespaceRepo.FirstOrDefault(x => x.Id == addedItem2.Id);
            Assert.NotNull(retrievedNamespace);
            Assert.Equal(testItem2.Id, retrievedNamespace.Id);
            Assert.Equal(testItem2.Name, retrievedNamespace.Name);
        }

        [Fact]
        public void CanRemoveItemFromRepository()
        {
            var repo = RepositoryFactory.GetRepo<ElectrumJob>();
            Assert.NotNull(repo);
            var testItem = new ElectrumJob
            {
                Id = System.Guid.NewGuid(),
                Namespace = TestConfigurator.Namespace
            };
            var addedItem = repo.Add(testItem);
            Assert.NotNull(addedItem);
            Assert.Equal(testItem.Id, addedItem.Id);
            Assert.Equal(testItem.Namespace, addedItem.Namespace);

            repo.Remove(testItem);
            var item = repo.FirstOrDefault(x => x.Id == addedItem.Id);
            Assert.Null(item);
        }
    }
}