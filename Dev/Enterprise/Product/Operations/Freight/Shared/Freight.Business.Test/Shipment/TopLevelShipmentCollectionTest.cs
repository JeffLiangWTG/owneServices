using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TopLevelShipmentCollectionTest : BaseFreightTest
	{
		public void TestIsThisPartOfTheCollection()
		{
			var cLD_MAS = FreightTestHelper.GetShipment<CommonShipment>("CLD_MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var cLD_SUB = FreightTestHelper.GetShipment("CLD_SUB", cLD_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var bCN_MAS = FreightTestHelper.GetShipment<CommonShipment>("BCN_MAS", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var bCN_SUB = FreightTestHelper.GetShipment("BCN_SUB", bCN_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var sTD = FreightTestHelper.GetShipment<CommonShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			var mAS_2 = FreightTestHelper.GetShipment<CommonShipment>("MAS_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS_2, Constants.ShipmentTypes.AssemblyMaster, Factory);
			var consol2 = FreightTestHelper.GetConsol<CommonConsol>("CON_2", Factory);
			consol2.Shipments.AddRange(mAS_2, sUB_2);

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			consol.Shipments.AddRange(bCN_MAS, bCN_SUB, cLD_MAS, cLD_SUB, sTD, sUB_2);

			var collection = new TopLevelShipmentCollection(consol.Shipments, consol);
			FreightTestHelper.AssertShipmentCollection(collection, bCN_MAS, cLD_MAS, sTD, sUB_2);

			collection.ShouldShowChildShipments = true;
			FreightTestHelper.AssertShipmentCollection(collection, bCN_MAS, bCN_SUB, cLD_MAS, cLD_SUB, sTD, sUB_2);

			collection.ShouldShowChildShipments = false;
			FreightTestHelper.AssertShipmentCollection(collection, bCN_MAS, cLD_MAS, sTD, sUB_2);
		}
	}
}
