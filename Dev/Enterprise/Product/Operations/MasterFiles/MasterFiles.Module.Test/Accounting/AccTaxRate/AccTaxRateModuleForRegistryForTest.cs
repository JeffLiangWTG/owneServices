using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccTaxRateModuleForRegistryForTest : AccTaxRateModuleForRegistry
	{
		public IBusinessObjectCollection GetNewGridCollectionForTesting() => GetNewGridCollection();
	}
}
