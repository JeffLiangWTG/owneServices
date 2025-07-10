using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(SalesEnquiryFormCustomisationSettingsProvider))]
	sealed class SalesEnquiryFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<SalesEnquiryFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				OrgColdCallRegisterSchema.O1_EnquiryType.Name,
				OrgColdCallRegisterSchema.O1_LeadSource.Name,
				OrgColdCallRegisterSchema.O1_OH_ConvertedToQualifiedLead.Name
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

		public override SalesEnquiryFormCustomisationSettingsProvider GetNewProvider()
		{
			return new SalesEnquiryFormCustomisationSettingsProvider();
		}
	}
}
