using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbBranchNotCurrentCompanyRelatedModule))]
	sealed class GlbBranchNotCurrentCompanyRelatedModuleTest : GlbBranchModuleTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbBranchNotCurrentCompanyRelated;
		}
	}
}
