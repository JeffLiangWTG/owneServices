using System;
using System.Linq;
using Hawking.eHub.Mapping.Config.eHubTransactions;
using Unity;
using Xunit;

namespace Hawking.eHub.Mapping.Config.Tests
{
    public class ResourceHelperFixtures
    {
        IeHubMappingDataContext dataContext;

        public ResourceHelperFixtures()
        {
            Hawking.Unity.DependencyFactory.Container.RegisterType<IeHubMappingDataContext, eHubMappingJsonDataContext>();
            dataContext = Hawking.Unity.DependencyFactory.Container.Resolve<IeHubMappingDataContext>();
        }
        
        [Fact] public void TesteHubClients() { var dataset = dataContext.eHubClients; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubClientRegistrations() { var dataset = dataContext.eHubClientRegistrations; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubClientSystems() { var dataset = dataContext.eHubClientSystems; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubClientSystemRegistrations() { var dataset = dataContext.eHubClientSystemRegistrations; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubCodeMapKeys() { var dataset = dataContext.eHubCodeMapKeys; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubCodeMapValues() { var dataset = dataContext.eHubCodeMapValues; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubCodeSets() { var dataset = dataContext.eHubCodeSets; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubCodeSetResults() { var dataset = dataContext.eHubCodeSetResults; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubMessageTypes() { var dataset = dataContext.eHubMessageTypes; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubTransformationMappings() { var dataset = dataContext.eHubTransformationMappings; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubTransformationSets() { var dataset = dataContext.eHubTransformationSets; Assert.True(dataset.Any()); }
        [Fact] public void TesteHubTransformationTypes() { var dataset = dataContext.eHubTransformationTypes; Assert.True(dataset.Any()); }
    }
}
