using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using static Enterprise.MasterFiles.Business.Testing.GenericBusinessContextTest;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GenericBusinessContextProviderTest : TestCaseWithFactory
	{
		public void TestProvider()
		{
			var factory = new BusinessObjectFactory();
			var genericBusinessContext = ObjectFactory.Get<IGenericBusinessContextProvider>().GetInstance<MyTestContexts>(factory);
			AssertType<GenericBusinessContext<MyTestContexts>>(genericBusinessContext);
		}
	}
}
