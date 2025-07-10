using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(ShipmentsForTotallingCollection))]
	sealed class ShipmentsForTotallingCollectionBOTest : BusinessObjectCollectionViewTestCase<ShipmentsForTotallingCollection>
	{
		protected override ShipmentsForTotallingCollection GetCollectionToTest()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			return new ShipmentsForTotallingCollection(consol.Shipments, consol);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonShipment testRemovableShipment = CommonShipment.New(Factory);
			testRemovableShipment.JS_IsForwardRegistered = false;
			testRemovableShipment.JS_IsCFSRegistered = false;
			testRemovableShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			testRemovableShipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			return testRemovableShipment;
		}

		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			((CommonShipment)bizO1).JS_ShipmentType = Constants.ShipmentTypes.BuyersConsolLead;
			((CommonShipment)bizO2).JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			Collection.AddRange(bizO1, bizO2);
			Factory.Save();

			Collection.Remove(bizO1);
			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);
		}
	}
}
