using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.ServiceTasks.Orders;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Testing.Orders.Converters
{
	class OrderLineConverterTest : TestCaseWithFactory
	{
		public void TestFillPackLine()
		{
			var converter = new OrderLineConverter();
			var packLine = Factory.New<ForwardingPackLine>();
			var orderLine = Factory.New<OrderLine>();

			orderLine.JO_ItemPrice = 3m;
			orderLine.JO_RN_NKCountryOfOrigin = "TH";
			orderLine.JO_AdditionalInformation = "test additional.";
			orderLine.JO_HSCode = "HSC";
			orderLine.JO_F3_NKPackType = Core.Constants.PkgUnit.Drum;

			converter.FillPackLine(packLine, orderLine, 18.9m);

			CombineAssertions(() =>
			{
				AssertEquals(18.9m * 3.0m, packLine.JL_LinePrice);
				AssertEquals("", packLine.JL_RN_NKOrigin);
				AssertEquals("", packLine.JL_HarmonisedCode);
				AssertEquals("", packLine.JL_DetailedDescription);

				AssertEquals(1, packLine.Products.Count);
				AssertEquals(orderLine.PK, packLine.Products[0].D2_JO);
				AssertEquals(18.9m, packLine.Products[0].D2_ProductQuantity);
				AssertEquals(Core.Constants.PkgUnit.Drum, packLine.Products[0].D2_ProductUnitOfQty);
			});
		}
	}
}
