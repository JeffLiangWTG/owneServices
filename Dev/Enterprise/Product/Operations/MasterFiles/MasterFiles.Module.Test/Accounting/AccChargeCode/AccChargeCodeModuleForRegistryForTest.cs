using CargoWise.EntityFramework;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class AccChargeCodeModuleForRegistryForTest : AccChargeCodeModuleForRegistry
	{
		public IBusinessObjectCollection GetNewGridCollectionForTesting() => GetNewGridCollection();
	}
}
