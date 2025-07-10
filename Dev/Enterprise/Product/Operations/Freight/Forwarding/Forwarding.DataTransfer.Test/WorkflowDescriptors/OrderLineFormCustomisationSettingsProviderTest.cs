using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(OrderLineFormCustomisationSettingsProvider))]
	public class OrderLineFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<OrderLineFormCustomisationSettingsProvider>
	{
		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
			AssertEquals(null, provider.DisplayTabs.Settings);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(System.Array.Empty<string>(), provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override OrderLineFormCustomisationSettingsProvider GetNewProvider()
		{
			return new OrderLineFormCustomisationSettingsProvider();
		}
	}
}
