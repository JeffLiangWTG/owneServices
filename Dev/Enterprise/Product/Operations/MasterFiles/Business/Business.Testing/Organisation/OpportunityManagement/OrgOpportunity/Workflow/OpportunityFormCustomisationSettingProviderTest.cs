using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OpportunityFormCustomisationSettingProvider))]
	sealed class OpportunityFormCustomisationSettingProviderTest : FormCustomisationSettingsProviderTest<OpportunityFormCustomisationSettingProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				OrgOpportunitySchema.P8_OpportunityType.Name,
				OrgOpportunitySchema.P8_Source.Name,
				OrgOpportunitySchema.P8_PackageType.Name
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

		public override OpportunityFormCustomisationSettingProvider GetNewProvider()
		{
			return new OpportunityFormCustomisationSettingProvider();
		}
	}
}
