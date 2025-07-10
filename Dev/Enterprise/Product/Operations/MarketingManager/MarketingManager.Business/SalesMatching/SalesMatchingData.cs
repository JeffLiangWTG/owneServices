using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class SalesMatchingData : NonPersistentBusinessObject
	{
		internal SalesMatchingData()
		{
			// Do not use this constructor - required for collection binding
		}

		public SalesMatchingData(OrgSales sales, OrgTradeDetail tradeDetail)
		{
			Argument.NotNull(sales, "sales");

			this.sales = sales;
			this.tradeDetail = tradeDetail;
		}

		public OrgSales Sales
		{
			get { return sales; }
		}
		readonly OrgSales sales;

		public OrgTradeDetail TradeDetail
		{
			get { return tradeDetail; }
		}
		readonly OrgTradeDetail tradeDetail;

		public ISalesValue LowestSalesValue
		{
			get { return (ISalesValue)TradeDetail ?? Sales; }
		}

		#region OriginDescription

		[ResourceStringData("SalesMatchingData|OriginDescription", Caption = "Origin")]
		public ZString OriginDescription
		{
			get { return ZString.Format("{0} ({1})", Sales.OriginCode, Sales.OriginLocationType); }
		}

		#endregion

		#region DestinationDescription

		[ResourceStringData("SalesMatchingData|DestinationDescription", Caption = "Destination")]
		public ZString DestinationDescription
		{
			get { return ZString.Format("{0} ({1})", Sales.DestinationCode, Sales.DestinationLocationType); }
		}

		#endregion

		public ZString SupplierCode { get; set; }
		public ZString BuyerCode { get; set; }
		public ZString EstimatedValueCurrency { get; set; }
		public ZDecimal EstimatedValue { get; set; }

		public ZInt Rank { get; set; }
		public bool IsOriginExactMatch { get; set; }
		public bool IsDestinationExactMatch { get; set; }
	}
}
