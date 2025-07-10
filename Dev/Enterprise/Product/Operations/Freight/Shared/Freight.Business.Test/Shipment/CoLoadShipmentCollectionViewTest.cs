namespace Enterprise.Freight.Business.Testing
{
	sealed class CoLoadShipmentCollectionViewTest : BaseFreightTest
	{
		public void TestCoLoadShipmentCollectionView()
		{
			var sTD = FreightTestHelper.GetShipment<CommonShipment>("STD", Core.Constants.ShipmentTypes.StandardHouse, Factory);
			var aSM_MAS = FreightTestHelper.GetShipment<CommonShipment>("ASM_MAS", Core.Constants.ShipmentTypes.AssemblyMaster, Factory);
			var aSM_SUB = FreightTestHelper.GetShipment("ASM_SUB", aSM_MAS, Core.Constants.ShipmentTypes.StandardHouse, Factory);

			var mAS_2 = FreightTestHelper.GetShipment<CommonShipment>("MAS_2", Core.Constants.ShipmentTypes.AssemblyMaster, Factory);
			mAS_2.Consols.AddNew();
			var sUB_2 = FreightTestHelper.GetShipment("SUB_2", mAS_2, Core.Constants.ShipmentTypes.StandardHouse, Factory);

			var consol = FreightTestHelper.GetConsol<CommonConsol>("CON", Factory);
			consol.Shipments.AddRange(sTD, aSM_MAS, aSM_SUB, sUB_2);

			var collection = new CoLoadShipmentCollectionView(consol.Shipments, consol);
			collection.Rebuild();
			FreightTestHelper.AssertShipmentCollection(collection, aSM_SUB);
		}
	}
}
