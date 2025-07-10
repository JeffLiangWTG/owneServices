using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodModeGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var modeGrouped = tradePeriods.GroupBy(x => x.TradeDetail.PA_TradeMode);
			foreach (var modeGrouping in modeGrouped)
			{
				yield return new Grouping(modeGrouping.Key, modeGrouping);
			}
		}
	}
}
