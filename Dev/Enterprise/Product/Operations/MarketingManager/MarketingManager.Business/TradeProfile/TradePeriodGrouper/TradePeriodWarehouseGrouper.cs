using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodWarehouseGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(WhsWarehouseSchema.Constants.TableName, period.TradeDetail.Parent.OW_WW);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var warehouseGrouped = tradePeriods.GroupBy(x => x.TradeDetail.Parent.OW_WW);
			foreach (var grouping in warehouseGrouped)
			{
				var warehouseDescription = grouping.First().TradeDetail.Parent.WarehouseDescription;
				yield return new Grouping(warehouseDescription, grouping);
			}
		}
	}
}
