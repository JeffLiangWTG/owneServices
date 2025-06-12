using Hawking.eHub.Model.eHubTransactions;
using Hawking.Unity;
using Unity;

namespace Hawking.eHub.Model.Tests
{
    public class FixturesBase
    {
        protected FixturesBase()
        {
            Container = DependencyFactory.Container;

            Unity.Config.ApplicationConfig.Initialise();
            Container.RegisterType<IeHubTransactionsContext, eHubTransactionsContext>();
        }

        protected IUnityContainer Container { get; }
    }
}
