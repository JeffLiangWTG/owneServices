using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderDataFormatter : WhsDocketDataFormatter
	{
		public override void FormatXsdDocketData(Xsd.WhsDocket xsdDocket)
		{
			base.FormatXsdDocketData(xsdDocket);
			if (xsdDocket.DocketDetail.Item is Xsd.WhsCustomerOrderDetail)
			{
				var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)xsdDocket.DocketDetail.Item;
				customerOrderDetail.OrderType = GetFormattedStringValue(customerOrderDetail.OrderType);
			}
		}

		public override void FormatXsdDocketLineData(Xsd.WhsDocketLine xsdLine)
		{
			base.FormatXsdDocketLineData(xsdLine);
			if (xsdLine.Item is Xsd.WhsCustomerOrderLineDetail)
			{
				var customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)xsdLine.Item;
				customerOrderLineDetail.ConsigneeOrBuyerProductCode = GetFormattedStringValue(customerOrderLineDetail.ConsigneeOrBuyerProductCode);
				customerOrderLineDetail.ConsigneeOrBuyerProductDescription = GetFormattedStringValue(customerOrderLineDetail.ConsigneeOrBuyerProductDescription);
			}
		}
	}
}
