using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgSalesCallFormCustomisationSettingsProvider))]
	sealed class OrgSalesCallFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<OrgSalesCallFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				OrgSalesCallSchema.OQ_TypeOfCall.Name,
				OrgSalesCallSchema.OQ_Status.Name,
				OrgSalesCallSchema.OQ_Category.Name
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

		public override OrgSalesCallFormCustomisationSettingsProvider GetNewProvider()
		{
			return new OrgSalesCallFormCustomisationSettingsProvider();
		}
	}
}
