using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class OrderDeliveryLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestOrderAndLineNumberValidation()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			OrderLine order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_LineNo = 11;
			OrderLine order1Line2 = order1.OrderLines.AddNew();
			order1Line2.JO_LineNo = 12;

			OrderDeliveryLine line1 = preAdvice.OrderLines.AddNew();
			line1.OrderNumber = "33";
			line1.OrderNumber = "";
			line1.OrderLineNumber = "44";
			line1.OrderLineNumber = "";
			AssertHasErrors(line1.OrderNumberInfo);
			AssertHasErrors(line1.OrderLineNumberInfo);

			line1.OrderNumber = "1";
			line1.OrderLineNumber = "113";
			AssertNoErrors(line1.OrderNumberInfo);
			AssertHasErrors(line1.OrderLineNumberInfo);

			line1.OrderNumber = "1";
			line1.OrderLineNumber = "11";
			AssertNoErrors(line1.OrderNumberInfo);
			AssertNoErrors(line1.OrderLineNumberInfo);

			line1.OrderNumber = "22";
			AssertHasErrors(line1.OrderNumberInfo);
		}

		public void TestDuplicateValidation()
		{
			JobShipmentPreplanning preAdvice = Factory.New<JobShipmentPreplanning>();

			Order order1 = preAdvice.Orders.AddNew();
			order1.JD_OrderNumber = "1";
			OrderLine order1Line1 = order1.OrderLines.AddNew();
			order1Line1.JO_LineNo = 11;

			OrderDeliveryLine line1 = preAdvice.OrderLines.AddNew();
			line1.OrderNumber = "1";
			line1.OrderLineNumber = "11";

			OrderDeliveryLine line2 = preAdvice.OrderLines.AddNew();
			line2.OrderNumber = "1";
			line2.OrderLineNumber = "11";

			line2.RunPreSaveValidation();
			AssertHasErrors(line2.OrderNumberInfo);
			AssertHasErrors(line2.OrderLineNumberInfo);

			line1.RunPreSaveValidation();
			AssertHasErrors(line1.OrderNumberInfo);
			AssertHasErrors(line1.OrderLineNumberInfo);

			Order order1Split = preAdvice.Orders.AddNew();
			order1Split.JD_OrderNumber = "1";
			order1Split.JD_OrderNumberSplit = 1;
			line2.OrderNumberSplit = 1;
			line2.RunPreSaveValidation();
			AssertNoErrors(line2.OrderLineNumberInfo);
		}
	}
}
