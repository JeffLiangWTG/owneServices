using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderFormCustomisationSettingsProvider))]
	public class OrderFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<OrderFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				JobOrderHeaderSchema.JD_OA_BuyerAddress.Name,
				JobOrderHeaderSchema.JD_OA_SupplierAddress.Name,
				JobOrderHeaderSchema.JD_TransportMode.Name
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

		public override OrderFormCustomisationSettingsProvider GetNewProvider()
		{
			return new OrderFormCustomisationSettingsProvider();
		}
	}
}
