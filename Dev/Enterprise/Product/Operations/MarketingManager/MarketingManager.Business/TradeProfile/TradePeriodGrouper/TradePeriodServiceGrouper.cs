using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodServiceGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var serviceGrouped = tradePeriods.GroupBy(x => x.TradeDetail.Parent.OW_Service);
			foreach (var serviceGrouping in serviceGrouped)
			{
				yield return new Grouping(serviceGrouping.Key, serviceGrouping);
			}
		}
	}
}
