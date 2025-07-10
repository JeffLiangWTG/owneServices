using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCusCodeFilter))]
	sealed class OrgCusCodeFilterTest : ModuleTextFilterTest
	{
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new OrgCusCodeFilter("moo", DummyBizoSchema.Z0_Description);
		}
	}
}
