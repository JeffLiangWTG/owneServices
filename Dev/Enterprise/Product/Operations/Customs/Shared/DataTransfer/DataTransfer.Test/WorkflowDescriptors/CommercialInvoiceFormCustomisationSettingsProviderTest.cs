using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CommercialInvoiceFormCustomisationSettingsProvider))]
	sealed class CommercialInvoiceFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<CommercialInvoiceFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				BaseJobComInvoiceHeader.Schema.JZ_OH_Buyer,
				BaseJobComInvoiceHeader.Schema.JZ_OH_Supplier,
				BaseJobComInvoiceHeader.Schema.JZ_MessageType
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

		public override CommercialInvoiceFormCustomisationSettingsProvider GetNewProvider() => new CommercialInvoiceFormCustomisationSettingsProvider();
	}
}
