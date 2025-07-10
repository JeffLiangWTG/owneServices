using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodUnlocoToUnlocoGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, period.TradeDetail.Parent.OW_OriginID);
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, period.TradeDetail.Parent.OW_DestinationID);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var unlocoToUnlocoGrouped = tradePeriods.GroupBy(x => new
			{
				x.TradeDetail.Parent.OriginUnlocoCode,
				x.TradeDetail.Parent.DestinationUnlocoCode,
			});

			foreach (var unlocoToUnlocoGrouping in unlocoToUnlocoGrouped)
			{
				var description =
					FallbackToUnknownStringIfEmpty(unlocoToUnlocoGrouping.Key.OriginUnlocoCode)
						+ " -> "
						+ FallbackToUnknownStringIfEmpty(unlocoToUnlocoGrouping.Key.DestinationUnlocoCode);
				yield return new Grouping(description, unlocoToUnlocoGrouping);
			}
		}
	}
}
