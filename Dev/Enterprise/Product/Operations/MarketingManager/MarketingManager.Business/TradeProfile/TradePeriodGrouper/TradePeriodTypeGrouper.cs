using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodTypeGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var typeGrouped = tradePeriods.GroupBy(x => x.TradeDetail.PA_TradeType);
			foreach (var typeGrouping in typeGrouped)
			{
				yield return new Grouping(typeGrouping.Key, typeGrouping);
			}
		}
	}
}
