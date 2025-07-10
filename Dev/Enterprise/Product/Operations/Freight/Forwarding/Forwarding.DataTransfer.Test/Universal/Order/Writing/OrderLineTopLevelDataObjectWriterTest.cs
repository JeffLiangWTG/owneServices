using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	sealed class OrderLineTopLevelDataObjectWriterTest : OrganizationAddressTestHelper
	{
		#region TestOrderLineTopLevelMappings

		public void TestOrderLineTopLevelMappings()
		{
			var orderBO = Factory.New<Order>();
			orderBO.JD_OrderNumber = "ORDER1";
			orderBO.JD_OrderNumberSplit = new ZByte(2);
			orderBO.JD_BookingConfRef = "Booking Reference";

			var orderLineBO = orderBO.OrderLines.AddNew();
			orderLineBO.JO_LineNo = 2;
			orderLineBO.JO_SubLineNo = 3;

			var writer = new OrderLineTopLevelDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, orderLineBO)));
			var dataObject = writer.GetDataObject(orderLineBO);
			AssertNotNull(dataObject);

			AssertNotNull(dataObject.Order?.OrderLineCollection);
			AssertEquals(1, dataObject.Order.OrderLineCollection.Count);

			var orderLine = dataObject.Order.OrderLineCollection[0];
			AssertNotNull(orderLine);
			AssertEquals(2, orderLine.LineNumber);
			AssertEquals(3, orderLine.SubLineNumber);
		}

		#endregion
	}
}
