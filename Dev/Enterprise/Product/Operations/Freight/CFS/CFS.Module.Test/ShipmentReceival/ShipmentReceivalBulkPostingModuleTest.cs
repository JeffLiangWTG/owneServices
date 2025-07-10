using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class ShipmentReceivalBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (ShipmentReceivalModule)ZModuleFactory.Instance.Create(ModuleIDs.ShipmentReceival);
		}
	}
}
