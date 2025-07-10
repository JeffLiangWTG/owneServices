using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	[TestedType(typeof(BillOfLadingFormCustomisationSettingsProvider))]
	public class BillOfLadingFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<BillOfLadingFormCustomisationSettingsProvider>
	{
		public override BillOfLadingFormCustomisationSettingsProvider GetNewProvider() => new BillOfLadingFormCustomisationSettingsProvider();

		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
			AssertEquals(null, provider.DisplayTabs.Settings);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				JobShipmentSchema.JS_RL_NKOrigin.Name,
				JobShipmentSchema.JS_RL_NKDestination.Name,
				AgencyShipment.Schema.JS_NKDischargePort,
				AgencyShipment.Schema.JS_NKLoadPort,
				JobShipmentSchema.JS_PackingMode.Name,
				AgencyShipment.Schema.ConsignorPK,
				AgencyShipment.Schema.ConsigneePK,
				AgencyShipment.Schema.ShipmentJobHeaderPK
			};
			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}
	}
}
