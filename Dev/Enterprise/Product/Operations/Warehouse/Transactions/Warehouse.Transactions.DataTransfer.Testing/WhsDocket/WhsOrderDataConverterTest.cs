using CargoWise.Types;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class WhsOrderDataConverterTest : WhsDocketDataConverterTest
	{
		protected override void AssertHeaderOtherDetail(Xsd.WhsDocket xsdWhsDocket)
		{
			Xsd.WhsCustomerOrderDetail customerOrderDetail = (Xsd.WhsCustomerOrderDetail)xsdWhsDocket.DocketDetail.Item;
			AssertNotNull("CustomerOrderDetail", customerOrderDetail);
			AssertEquals("ORD", customerOrderDetail.OrderType);
			AssertEquals(new ZDateTime(2006, 10, 4), customerOrderDetail.DateRequired);
			AssertEquals(ZDecimal.Zero, customerOrderDetail.TotalBillToInvoiceAmount);

			AssertOrgDetail(customerOrderDetail.Consignee.AddressReference.Organisation, "KILABABNE", "KILLARNEY ABATTOIR Pty Ltd", "AUBNE");
			AssertOrgDetail(customerOrderDetail.GoodsBilledTo.AddressReference.Organisation, "DANPACWLG", "Alcan Packaging Danaflex", "NZWLG");
		}

		protected override void AssertLineOtherDetail(Xsd.WhsDocketLine xsdWhsDocketLine)
		{
			AssertEquals("ATTR1", xsdWhsDocketLine.LineAttributes.CustomAttribute1);
			AssertEquals("ATTR2", xsdWhsDocketLine.LineAttributes.CustomAttribute2);
			AssertEquals("ATTR3", xsdWhsDocketLine.LineAttributes.CustomAttribute3);

			var customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)xsdWhsDocketLine.Item;
			AssertNotNull("CustomerOrderLineDetail", customerOrderLineDetail);
			AssertEquals("BUYERPRODUCTCODE", customerOrderLineDetail.ConsigneeOrBuyerProductCode);
			AssertEquals("BUYERPRODUCTDESC", customerOrderLineDetail.ConsigneeOrBuyerProductDescription);
			AssertEquals(14.5m, customerOrderLineDetail.Pricing.RecommendedUnitPrice);
			AssertEquals(20.0m, customerOrderLineDetail.Pricing.UnitDiscount);
			AssertEquals(2.8m, customerOrderLineDetail.Pricing.UnitDiscountAmount);
			AssertEquals(11.6m, customerOrderLineDetail.Pricing.UnitPriceAfterDiscount);
			AssertEquals(200.0m, customerOrderLineDetail.Pricing.ExtendedPrice);
		}

		protected override WhsDocketDataConverter GetDataConverter()
		{
			return new WhsOrderDataConverter(Notify, Factory);
		}

		protected override string[] Line1
		{
			get
			{
				return new string[] { "WOL", "1", "", "X8434", "SK10 PLAIN MARAFLEX 380MM TUBING", "BUYERPRODUCTCODE", "BUYERPRODUCTDESC", "", "", "", "6800.000", "6500.000", "M", "", "", "14.50", "20", "2.80", "11.6", "200.00", "", "", "24799", "6500", "01.2335.00", "ATTR1", "ATTR2", "ATTR3", "SERRN2", "20061005", "20061004", "Please despatch 6500.00 M (13 Cartons) from pallet 24799" };
			}
		}
	}
}
