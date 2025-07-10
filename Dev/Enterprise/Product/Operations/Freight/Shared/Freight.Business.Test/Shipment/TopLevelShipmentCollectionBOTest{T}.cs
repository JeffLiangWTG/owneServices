using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.Freight.Business.Testing
{
	public abstract class TopLevelShipmentCollectionBOTest<T> : BusinessObjectCollectionViewTestCase<T> where T : TopLevelShipmentCollection
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			CommonShipment testRemovableShipment = CommonShipment.New(Factory);
			testRemovableShipment.JS_IsForwardRegistered = false;
			testRemovableShipment.JS_IsCFSRegistered = false;
			testRemovableShipment.JS_JS_ColoadMasterShipment = ZGuid.Empty;
			testRemovableShipment.JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			return testRemovableShipment;
		}

		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			((CommonShipment)bizO1).JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			((CommonShipment)bizO2).JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			Collection.AddRange(bizO1, bizO2);
			Factory.Save();

			Collection.Remove(bizO1);
			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);
		}

		public void TestAddingMultipleShipmentsUpdateConsolBindingOnlyOnce()
		{
			var bizO1 = GetNewElementToAddToTheCollection();
			var bizO2 = GetNewElementToAddToTheCollection();
			var bizO3 = GetNewElementToAddToTheCollection();
			((CommonShipment)bizO1).JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			((CommonShipment)bizO2).JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;
			((CommonShipment)bizO3).JS_ShipmentType = Constants.ShipmentTypes.CoLoadMaster;

			var collection = Collection;
			var shipmentCollection = (ManyToManyShipmentCollection)collection.CollectionToFilter;

			shipmentCollection.ParentConsolRefreshBindingCount = 0;
			(collection as IBusinessObjectCollection).AddRange(new[] { bizO1, bizO2, bizO3 });

			AssertEquals(1, shipmentCollection.ParentConsolRefreshBindingCount);
		}
	}
}
