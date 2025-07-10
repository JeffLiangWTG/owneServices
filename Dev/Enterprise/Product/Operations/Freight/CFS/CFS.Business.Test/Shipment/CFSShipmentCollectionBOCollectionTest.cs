using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(CFSShipmentCollection))]
	public class CFSShipmentCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestTranshipToOtherCFSNotResetWhenAddingToDomesticLoadList()
		{
			var consol1 = Factory.New<CFSLoadListConsol>();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "AUSYD";

			var consol2 = Factory.New<CFSLoadListConsol>();
			consol2.JK_RL_NKLoadPort = "AUMEL";
			consol2.JK_RL_NKDischargePort = "AUSYD";

			var shipment = Factory.New<CFSShipment>();
			shipment.JS_TranshipToOtherCFS = true;

			consol1.Shipments.Add(shipment);
			consol2.Shipments.Add(shipment);

			Assert(shipment.JS_TranshipToOtherCFS);
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
			Assert("Element was in Database.  Element should not be deleted", !bizO1.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			CFSLoadListConsol parent = Factory.New<CFSLoadListConsol>();
			return new CFSShipmentCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<CFSShipment>();
		}
	}
}
