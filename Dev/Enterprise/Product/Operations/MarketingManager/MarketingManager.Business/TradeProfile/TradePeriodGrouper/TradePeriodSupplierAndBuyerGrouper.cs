using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodSupplierAndBuyerGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(OrgHeaderSchema.Constants.TableName, period.TradeDetail.Parent.OW_OH_Buyer);
				factory.AddFetchHint(OrgHeaderSchema.Constants.TableName, period.TradeDetail.Parent.OW_OH_Supplier);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var factory = tradePeriods.First().Factory;
			var firstSales = tradePeriods.First().TradeDetail.Parent;
			var product = firstSales.Product;
			if (product != null && !product.IsBuyerAllowed(firstSales) && !product.IsSupplierAllowed(firstSales))
			{
				yield break;
			}

			var buyerAndTypeGrouped = tradePeriods.GroupBy(x => new { x.TradeDetail.Sales.OW_OH_Buyer, x.TradeDetail.Sales.OW_OH_Supplier });
			foreach (var buyerAndTypeGrouping in buyerAndTypeGrouped)
			{
				var buyer = factory.Load<OrgHeader>(buyerAndTypeGrouping.Key.OW_OH_Buyer);
				var buyerCode = buyer != null ? buyer.OH_Code : (ZString)UnknownString;
				var supplier = factory.Load<OrgHeader>(buyerAndTypeGrouping.Key.OW_OH_Supplier);
				var supplierCode = supplier != null ? supplier.OH_Code : (ZString)UnknownString;
				var description = Res.GetString("24278af5-6327-472a-98f9-02a450b59f09", "Buyer:{0}  Supplier:{1}", buyerCode, supplierCode);
				yield return new Grouping(description, buyerAndTypeGrouping);
			}
		}
	}
}
