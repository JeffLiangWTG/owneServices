using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CampaignFormCustomisationSettingsProvider))]
	sealed class CampaignFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<CampaignFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				GlbCompanyCampaignSchema.G0_Category.Name,
				GlbCompanyCampaignSchema.G0_Type.Name
			};

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
			AssertEquals(null, provider.DisplayTabs.Settings);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override CampaignFormCustomisationSettingsProvider GetNewProvider()
		{
			return new CampaignFormCustomisationSettingsProvider();
		}
	}
}
