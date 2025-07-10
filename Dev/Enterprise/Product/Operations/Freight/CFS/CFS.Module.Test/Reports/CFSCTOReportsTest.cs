using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Module.Testing
{
	[TestedType(typeof(CFSCTOReports))]
	sealed class CFSCTOReportsTest : ZEmbeddedModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CFSCTOReports;
		}
	}
}
