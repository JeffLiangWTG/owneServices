using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.Business.Testing
{
	[TestedType(typeof(QuickPODCollection))]
	public class QuickPODCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QuickPODCollection>
	{
		#region Test Overrides

		protected override QuickPODCollection GetCollectionToTest()
		{
			QuickPODs quickPODs = new QuickPODs(Factory);
			return new QuickPODCollection(quickPODs);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			QuickPODs quickPODs = new QuickPODs(Factory);
			return new QuickPOD(quickPODs);
		}

		#endregion

		public void TestContainsHouseBill()
		{
			CommonShipment shipment1 = Factory.New<CommonShipment>();
			CommonShipment shipment2 = Factory.New<CommonShipment>();
			CommonShipment shipment3 = Factory.New<CommonShipment>();
			shipment1.JS_HouseBill = "1010";
			shipment2.JS_HouseBill = "2020";

			QuickPODs quickPODs = new QuickPODs(Factory);
			QuickPODCollection quickPODCollection = new QuickPODCollection(quickPODs);
			QuickPOD pod1 = quickPODCollection.AddNew();
			QuickPOD pod2 = quickPODCollection.AddNew();

			try
			{
				pod1.HouseBill = "1010";
				pod2.HouseBill = "2020";

				Assert("Should contain 1010", quickPODCollection.ContainsShipment(shipment1.PK, ZGuid.Empty));
				Assert("Should contain 2020", quickPODCollection.ContainsShipment(shipment2.PK, ZGuid.Empty));
				Assert("Should not contain 1010 if pod1 is excluded", !quickPODCollection.ContainsShipment(shipment1.PK, pod1.PK));
				Assert("Should not contain 2020 if pod2 is excluded", !quickPODCollection.ContainsShipment(shipment2.PK, pod2.PK));
				Assert("Should contain 2020, only pod1 is excluded", quickPODCollection.ContainsShipment(shipment2.PK, pod1.PK));
				Assert("Should not contain 3030", !quickPODCollection.ContainsShipment(shipment3.PK, ZGuid.Empty));
			}
			finally
			{
				pod1.Shipment.ShipmentJobHeader.Dispose();
				pod2.Shipment.ShipmentJobHeader.Dispose();
			}
		}
	}
}
