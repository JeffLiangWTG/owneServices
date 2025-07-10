using System.Linq;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ForwardingConsolFormCustomisationSettingsProvider))]
	public class ForwardingConsolFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<ForwardingConsolFormCustomisationSettingsProvider>
	{
		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				JobConsolSchema.JK_TransportMode.Name,
				JobConsolSchema.JK_RL_NKLoadPort.Name,
				JobConsolSchema.JK_RL_NKDischargePort.Name,
				JobConsolSchema.JK_OA_ShippingLineAddress.Name
			};

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			var tabs = provider.DisplayTabs;

			var expectedTabNames = new string[]
			{
				"NCTSTabPage"
			};

			AssertEquals("Expected 1 tabs", 1, provider.DisplayTabs.Count);
			AssertContainsExactElementsInAnyOrder(expectedTabNames, tabs.Cast<FormCustomisableElement>().Select(elem => (string)elem.ElementName).ToArray());
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override ForwardingConsolFormCustomisationSettingsProvider GetNewProvider()
		{
			return new ForwardingConsolFormCustomisationSettingsProvider();
		}
	}
}
