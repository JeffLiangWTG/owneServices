using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodModeAndTypeGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var modeAndTypeGrouped = tradePeriods.GroupBy(x => new { x.TradeDetail.PA_TradeMode, x.TradeDetail.PA_TradeType });
			foreach (var modeAndTypeGrouping in modeAndTypeGrouped)
			{
				var mode = modeAndTypeGrouping.Key.PA_TradeMode;
				var type = modeAndTypeGrouping.Key.PA_TradeType;
				var description = Res.GetString("b9b6ffe0-5a91-4853-93fb-dbefef641a79", "Mode:{0}  Type:{1}", mode, type);
				yield return new Grouping(description, modeAndTypeGrouping);
			}
		}
	}
}
