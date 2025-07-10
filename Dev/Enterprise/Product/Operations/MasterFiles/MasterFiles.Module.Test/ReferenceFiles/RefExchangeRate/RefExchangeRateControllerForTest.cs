using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class RefExchangeRateControllerForTest : RefExchangeRateController
	{
		public RefExchangeRateControllerForTest(BusinessObjectFactory factory)
		{
			TestFactory = factory;
		}

		BusinessObjectFactory TestFactory { get; }

		protected override BusinessObjectFactory GetNewFactory() => TestFactory;

		public ZPKCollection ModuleResultsPKCollectionExposed { get => ModuleResultsPKCollection; set => ModuleResultsPKCollection = value; }
	}
}
