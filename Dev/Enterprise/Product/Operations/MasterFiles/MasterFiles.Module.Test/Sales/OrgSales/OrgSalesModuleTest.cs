using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgSalesModule))]
	sealed class OrgSalesModuleTest : ZModuleBasherTest
	{
		protected override bool HasController()
		{
			return false;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Sales;
		}
	}
}
