using System;
using Hawking.eHub.Model.eHubTransactions;
using Hawking.Unity;
using Unity;
using Xunit;

namespace Hawking.eHub.Model.Tests
{
    public class eHubClientsFixtures : FixturesBase
    {
        public eHubClientsFixtures() : base()
        {
        }

        [Fact]
        public void TestGeteHubClients()
        {
            var context = DependencyFactory.Container.Resolve<IeHubTransactionsContext>();

            var pk = Guid.NewGuid();
            var client = new eHubClient() { CC_PK = pk, CC_FriendlyName = "test", CC_ID = "test" }; ;
            context.eHubClient.Add(client);
            var c = context.eHubClient.Find(pk);
            Assert.NotNull(c);
        }
    }
}
