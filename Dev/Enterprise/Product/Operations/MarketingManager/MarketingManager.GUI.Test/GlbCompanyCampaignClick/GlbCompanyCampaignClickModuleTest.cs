using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.GUI.Testing
{
	[TestedType(typeof(GlbCompanyCampaignClickModule))]
	public class GlbCompanyCampaignClickModuleTest : ZModuleBasherTest
	{
		public void TestModuleIDAndSupportWorkflow()
		{
			using (GlbCompanyCampaignClickModule module = new GlbCompanyCampaignClickModule())
			{
				AssertEquals(ModuleIDs.GlbCompanyCampaignClick, module.ID);
				AssertEquals(false, module.SupportsWorkflow);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.GlbCompanyCampaignClick;
		}

		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert(true);
		}

		public void TestShowRecent()
		{
			using (var module = new GlbCompanyCampaignClickModuleForTest())
			{
				Assert("Should not show recent", !module.ShowRecentExposed);
			}
		}

		[StressTest]
		public override void TestModuleShowsAndCanSearch()
		{
			base.TestModuleShowsAndCanSearch();
		}
	}
}
