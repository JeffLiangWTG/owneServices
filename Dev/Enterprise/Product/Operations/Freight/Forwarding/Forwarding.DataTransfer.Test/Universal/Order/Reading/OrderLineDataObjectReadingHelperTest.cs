using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using UniversalOrderLine = Enterprise.UniversalDataBuss.DataObjects.Universal.OrderLine;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class OrderLineDataObjectReadingHelperTest : OrganizationAddressTestHelper
	{
		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_LineNumberMustBeGeaterThanZero()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ORGASYD";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "ORD03";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			Factory.SaveForTesting();

			var orderLineDataObject = new UniversalOrderLine(DefaultDataObjectWriterStrategy.TestInstance);

			orderLineDataObject.LineNumber = 0;
			AssertEquals("The line number must be greater than 0.", new OrderLineDataObjectReadingHelper(orderLineDataObject, Logger, Factory, orderBO, false).GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(null, false));

			orderLineDataObject.LineNumber = 1;
			AssertEquals(ZString.Empty, new OrderLineDataObjectReadingHelper(orderLineDataObject, Logger, Factory, orderBO, false).GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(null, false));
		}

		public void TestGetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_DuplicateLineReference()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "ORGASYD";

			var orderBO = Factory.NewWithValidTestData<Order>();
			orderBO.JD_OrderNumber = "ORD03";
			orderBO.JD_OrderNumberSplit = 1;
			orderBO.BuyerPK = buyer.PK;

			var orderLine1BO = orderBO.OrderLines.AddNew();
			orderLine1BO.JO_LineNo = 4;
			orderLine1BO.JO_SubLineNo = 1;
			orderLine1BO.JO_Description = "CAR";
			orderLine1BO.JO_LineReference = "RF001";

			var orderLine2BO = orderBO.OrderLines.AddNew();
			orderLine2BO.JO_LineNo = 5;
			orderLine2BO.JO_SubLineNo = 1;
			orderLine2BO.JO_Description = "CAR";
			orderLine2BO.JO_LineReference = "RF002";

			Factory.SaveForTesting();

			var orderLineDataObject = new UniversalOrderLine(DefaultDataObjectWriterStrategy.TestInstance);
			orderLineDataObject.LineNumber = 5;
			orderLineDataObject.SubLineNumber = 1;
			orderLineDataObject.LineReference = "RF001";

			AssertEquals("Matching order line's (order line 5 and sub-line 1) Line Reference cannot be updated to RF001 because RF001 already exists on another order line.",
				new OrderLineDataObjectReadingHelper(orderLineDataObject, Logger, Factory, orderBO, false).GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderLine2BO, false));

			orderLine2BO = orderBO.OrderLines.AddNew();
			orderLine2BO.JO_LineNo = 6;
			orderLine2BO.JO_SubLineNo = 1;
			orderLine2BO.JO_Description = "CAR";
			orderLine2BO.JO_LineReference = "RF002";
			orderLineDataObject.LineNumber = 6;
			AssertEquals("New order line cannot be created because Line Reference RF001 already exists on another order line.",
				new OrderLineDataObjectReadingHelper(orderLineDataObject, Logger, Factory, orderBO, false).GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(orderLine2BO, false));
		}
	}
}
