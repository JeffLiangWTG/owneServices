using Unity;
using Hawking.eHub.Mapping.Config.eHubTransactions;
using Xunit;
using Hawking.eHub.Mapping.Config.Services;
using Hawking.eHub.Model.Services;

namespace Hawking.eHub.Mapping.Config.Tests.Services
{
    public class MappingConfigServiceFixtures
    {
        readonly IUnityContainer UnityContainer;

        public MappingConfigServiceFixtures()
        {
            UnityContainer = Unity.DependencyFactory.Container;

            UnityContainer.RegisterType<IeHubMappingDataContext, eHubMappingJsonDataContext>();
            UnityContainer.RegisterType<IMappingConfigService, MappingConfigService>();
        }

        [Fact]
        public void TestLifeCycle()
        {
            Assert.NotNull(UnityContainer.Resolve<IMappingConfigService>());
        }

        [Fact]
        public void TestSelectTransformsByPartiesMessage()
        {

        }
    }
}
