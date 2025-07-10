using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;

namespace Enterprise.TransportConsignment.Module.Testing
{
	public class DtbBookingConsignmentBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return new DtbBookingConsignmentModule();
		}
	}
}
