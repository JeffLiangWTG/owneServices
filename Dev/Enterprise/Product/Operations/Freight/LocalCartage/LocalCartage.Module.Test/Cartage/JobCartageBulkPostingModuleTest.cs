using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	public class JobCartageBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return new CartageModuleForTest();
		}
	}
}
