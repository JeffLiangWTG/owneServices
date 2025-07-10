using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module
{
	[TestedType(typeof(GlbCompanyCampaignWithoutFilterModule))]
	public class GlbCompanyCampaignWithoutFilterModule_Test : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportsWorkflow()
		{
			using (GlbCompanyCampaignWithoutFilterModule module = new GlbCompanyCampaignWithoutFilterModule())
			{
				AssertEquals(ModuleIDs.GlbCompanyCampaignWithoutFilter, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompanyCampaignWithoutFilter;
		}
	}
}
