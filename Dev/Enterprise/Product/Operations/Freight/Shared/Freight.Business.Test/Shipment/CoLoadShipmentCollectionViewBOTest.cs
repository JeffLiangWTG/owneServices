using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(CoLoadShipmentCollectionView))]
	sealed class CoLoadShipmentCollectionViewBOTest : BusinessObjectCollectionViewTestCase<CoLoadShipmentCollectionView>
	{
		protected override CoLoadShipmentCollectionView GetCollectionToTest()
		{
			CommonConsol parent = Factory.New<CommonConsol>();
			return new CoLoadShipmentCollectionView(parent.Shipments, parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var masterShipment = CommonShipment.New(Factory);
			Collection.ParentConsol.Shipments.Add(masterShipment);

			var removableShipmentToAdd = CommonShipment.New(Factory);
			removableShipmentToAdd.JS_JS_ColoadMasterShipment = masterShipment.PK;
			removableShipmentToAdd.JS_IsForwardRegistered = false;
			removableShipmentToAdd.JS_IsCFSRegistered = false;

			return removableShipmentToAdd;
		}

		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			Collection.AddRange(bizO1, bizO2);
			Factory.Save();

			Collection.Remove(bizO1);
			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Element 1 was only removed, but not deleted", !bizO1.IsDeleted);
		}

		public override void TestAddNew()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			ConsolShipmentCollection allShipments = new ConsolShipmentCollection(consol);
			CoLoadShipmentCollectionView coLoad = new CoLoadShipmentCollectionView(allShipments, consol);
			CommonShipment coLoadShipment = allShipments.AddNew();
			AssertEquals(" Collection count equals 1 ", 1, allShipments.Count);
		}
	}
}
