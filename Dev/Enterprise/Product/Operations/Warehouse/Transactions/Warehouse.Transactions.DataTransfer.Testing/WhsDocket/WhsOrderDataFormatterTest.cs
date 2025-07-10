using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer.Testing
{
	public class WhsOrderDataFormatterTest : WhsDocketDataFormatterTest
	{
		protected override void SetupDataForXsdDocket()
		{
			base.SetupDataForXsdDocket();
			var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)XsdDocket.DocketDetail.Item;
			customerOrderDetail.OrderType = "order type";
		}

		protected override void AssertFormattedXsdDocketData()
		{
			base.AssertFormattedXsdDocketData();
			var customerOrderDetail = (Xsd.WhsCustomerOrderDetail)XsdDocket.DocketDetail.Item;
			AssertEquals(customerOrderDetail.OrderType.ToUpper(), customerOrderDetail.OrderType);
		}

		protected override void SetupDataForXsdDocketLine()
		{
			base.SetupDataForXsdDocketLine();
			var customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)XsdLine.Item;
			customerOrderLineDetail.ConsigneeOrBuyerProductCode = "product code";
			customerOrderLineDetail.ConsigneeOrBuyerProductDescription = "product description";
		}

		protected override void AssertFormattedXsdDocketLineData()
		{
			base.AssertFormattedXsdDocketLineData();
			var customerOrderLineDetail = (Xsd.WhsCustomerOrderLineDetail)XsdLine.Item;
			AssertEquals(customerOrderLineDetail.ConsigneeOrBuyerProductCode.ToUpper(), customerOrderLineDetail.ConsigneeOrBuyerProductCode);
			AssertEquals(customerOrderLineDetail.ConsigneeOrBuyerProductDescription.ToUpper(), customerOrderLineDetail.ConsigneeOrBuyerProductDescription);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			XsdDocket.DocketDetail.Item = new Xsd.WhsCustomerOrderDetail();
			XsdLine.Item = new Xsd.WhsCustomerOrderLineDetail();
		}

		protected override WhsDocketDataFormatter GetNewDocketDataFormatter()
		{
			return new WhsOrderDataFormatter();
		}

		#endregion
	}
}