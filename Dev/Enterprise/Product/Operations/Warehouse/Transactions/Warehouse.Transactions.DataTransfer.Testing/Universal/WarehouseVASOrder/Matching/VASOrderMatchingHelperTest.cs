using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Universal.Testing
{
	class VASOrderMatchingHelperTest : WhsUniversalTestCase
	{
		#region TestGetMatchingVASOrder

		public void TestGetMatchingVASOrder()
		{
			var client = Data.CreateClientOrgCRAHOLSYDInDB();
			var vasOrder1 = Helper.CreateWhsVASOrder(Data.ServiceArea, client);
			vasOrder1.WVO_CustomerReferenceNo = "123";

			var vasOrder2 = Helper.CreateWhsVASOrder(Data.ServiceArea, client);
			vasOrder2.WVO_CustomerReferenceNo = "456";

			var vasOrder3 = Helper.CreateWhsVASOrder(Data.ServiceArea, Data.Orgs.INTHEMSYD);
			vasOrder3.WVO_CustomerReferenceNo = "123";

			AssertNull("No DataObject Supplied, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, null, client.PK));

			var vasOrderDataObject = new Shipment();
			AssertNull("No Order DataObject Supplied, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, client.PK));

			vasOrderDataObject.Order = new Order();
			AssertNull("No Customer Reference Supplied, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, client.PK));

			vasOrderDataObject.Order.OrderNumber = "123";
			AssertEquals("Should match correct VAS Order.", vasOrder1, VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, client.PK));
			AssertNull("No Client Supplied, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, ZGuid.Empty));

			vasOrderDataObject.Order.OrderNumber = "";
			AssertNull("No Customer Reference Supplied, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, client.PK));

			vasOrderDataObject.Order.OrderNumber = "456";
			AssertEquals("Should match correct VAS Order.", vasOrder2, VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, client.PK));

			vasOrderDataObject.Order.OrderNumber = "123";
			AssertEquals("Should match correct VAS Order.", vasOrder3, VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, Data.Orgs.INTHEMSYD.PK));

			vasOrderDataObject.Order.OrderNumber = "IDONOTEXIST";
			AssertNull("No VAS Order has this Customer Reference, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, client.PK));

			vasOrderDataObject.Order.OrderNumber = "123";
			AssertNull("No VAS Order has this Client, should not match any VAS Order.", VASOrderMatchingHelper.GetMatchingVASOrder(Factory, vasOrderDataObject, Helper.CreateClient().PK));
		}

		#endregion

		protected override TestDataForUniversal GetNewTestData() => new TestDataForUniversal(Factory, Logger, DataContextType.WarehouseOrder);
	}
}
