using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodDestinationCountryGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, period.TradeDetail.Parent.OW_DestinationID);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var destinationCountryGrouped = tradePeriods.GroupBy(x => new { x.TradeDetail.Parent.DestinationCountryCode, x.TradeDetail.Parent.DestinationCountryDescription });
			foreach (var grouping in destinationCountryGrouped)
			{
				var description = Res.GetString("63ae15a8-1a86-4087-b925-2a82f8d649ae", "Destination: {0}",
					GetCountryGroupDescription(grouping.Key.DestinationCountryCode, grouping.Key.DestinationCountryDescription));
				yield return new Grouping(description, grouping);
			}
		}
	}
}
