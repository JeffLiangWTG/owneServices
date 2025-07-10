using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.Module.Testing
{
	sealed class LoadListConsolBulkPostingModuleTest : BulkPostingModuleTest
	{
		protected override IBulkPostingModuleInternalsForTesting GetNewModuleForTest()
		{
			return (LoadListConsolModule)ZModuleFactory.Instance.Create(ModuleIDs.LoadListConsol);
		}
	}
}
