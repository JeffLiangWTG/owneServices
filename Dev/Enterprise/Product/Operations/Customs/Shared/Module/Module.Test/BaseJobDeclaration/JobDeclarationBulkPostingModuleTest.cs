using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;

namespace Enterprise.Customs.Module.Testing
{
	sealed class JobDeclarationBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return new TestHelperBaseJobDeclarationModule();
		}
	}
}
