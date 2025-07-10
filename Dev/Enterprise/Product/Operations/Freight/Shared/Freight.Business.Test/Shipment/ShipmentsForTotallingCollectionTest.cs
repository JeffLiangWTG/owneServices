using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	sealed class ShipmentsForTotallingCollectionTest : TestCaseWithFactory
	{
		public void TestCollection()
		{
			CommonShipment bcnLeadShipment = CommonShipment.New(Factory);
			CommonShipment bcnSubShipment = CommonShipment.New(Factory);

			bcnLeadShipment.JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			bcnSubShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			CommonConsol bcnConsol = Factory.New<CommonConsol>();
			bcnConsol.Shipments.Add(bcnLeadShipment);
			bcnLeadShipment.CoLoadShipments.Add(bcnSubShipment);

			ShipmentsForTotallingCollection bcnTestCollection = new ShipmentsForTotallingCollection(bcnConsol.Shipments, bcnConsol);
			Assert("BCN Lead Shipment should be in collection", bcnTestCollection.Contains(bcnLeadShipment));
			Assert("BCN Sub Shipment should be in collection", bcnTestCollection.Contains(bcnSubShipment));

			CommonShipment coLoadMasterShipment = CommonShipment.New(Factory);
			CommonShipment coLoadSubShipment = CommonShipment.New(Factory);

			coLoadMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			coLoadSubShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			CommonConsol coLoadConsol = Factory.New<CommonConsol>();
			coLoadConsol.Shipments.Add(coLoadMasterShipment);
			coLoadMasterShipment.CoLoadShipments.Add(coLoadSubShipment);

			ShipmentsForTotallingCollection coLoadTestCollection = new ShipmentsForTotallingCollection(coLoadConsol.Shipments, coLoadConsol);
			Assert("Co-Load Master Shipment should be in collection", coLoadTestCollection.Contains(coLoadMasterShipment));
			Assert("Co-Load Sub Shipment should not be in collection", !coLoadTestCollection.Contains(coLoadSubShipment));

			CommonShipment asSuperMasterShipment = CommonShipment.New(Factory);
			CommonShipment asMasterShipment = CommonShipment.New(Factory);
			CommonShipment stdShipment = CommonShipment.New(Factory);

			asSuperMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			asMasterShipment.JS_ShipmentType = Constants.ShipmentTypes.AssemblyMaster;
			stdShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;

			stdShipment.JS_JS_ColoadMasterShipment = asMasterShipment.PK;
			asMasterShipment.JS_JS_ColoadMasterShipment = asSuperMasterShipment.PK;

			CommonConsol asmConsol = Factory.New<CommonConsol>();
			asmConsol.Shipments.Add(asSuperMasterShipment);

			ShipmentsForTotallingCollection asTestCollection = new ShipmentsForTotallingCollection(asmConsol.Shipments, asmConsol);

			AssertEquals("Consol contains all 3 shipments", asmConsol.Shipments.Count, 3);
			Assert("Assembly super-master is in collection", asTestCollection.Contains(asSuperMasterShipment));
			Assert("Assembly master is not in collection", !asTestCollection.Contains(asMasterShipment));
			Assert("Standard shipment is not in collection", !asTestCollection.Contains(stdShipment));
		}

		public void TestIsThisPartOfTheCollection()
		{
			var bCN_MAS = FreightTestHelper.GetShipment<CommonShipment>("BCN_MAS", Constants.ShipmentTypes.BuyersConsolLead, Factory);
			var bCN_SUB = FreightTestHelper.GetShipment("BCN_SUB", bCN_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var scn_MAS = FreightTestHelper.GetShipment<CommonShipment>("SCN_MAS", Constants.ShipmentTypes.ShippersConsolLead, Factory);
			var scn_SUB = FreightTestHelper.GetShipment("SCN_SUB", scn_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var cLD_MAS = FreightTestHelper.GetShipment<CommonShipment>("CLD_MAS", Constants.ShipmentTypes.CoLoadMaster, Factory);
			var cLD_SUB = FreightTestHelper.GetShipment("CLD_SUB", cLD_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var aSM_MAS = FreightTestHelper.GetShipment<CommonShipment>("ASM_MAS", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var aSM_SUB = FreightTestHelper.GetShipment("ASM_SUB", aSM_MAS, Constants.ShipmentTypes.StandardHouse, Factory);
			var sTD = FreightTestHelper.GetShipment<CommonShipment>("STD", Constants.ShipmentTypes.StandardHouse, Factory);

			var mAS_2 = FreightTestHelper.GetShipment<CommonShipment>("MAS_2", Constants.ShipmentTypes.AssemblyMaster, Factory);
			var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS_2, Constants.ShipmentTypes.AssemblyMaster, Factory);
			var consol2 = FreightTestHelper.GetConsol<CommonConsol>("CON_2", Factory);
			consol2.Shipments.AddRange(mAS_2, sUB_2);

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			consol.Shipments.AddRange(bCN_MAS, bCN_SUB, scn_MAS, scn_SUB, cLD_MAS, cLD_SUB, aSM_MAS, aSM_SUB, sTD, sUB_2);

			var collection = new ShipmentsForTotallingCollection(consol.Shipments, consol);
			collection.Rebuild();
			FreightTestHelper.AssertShipmentCollection(collection, bCN_MAS, bCN_SUB, scn_MAS, scn_SUB, cLD_MAS, aSM_MAS, sTD, sUB_2);
		}
	}
}
