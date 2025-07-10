using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class JobShipmentBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (JobShipmentModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment);
		}
	}
}
