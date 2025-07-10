using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodDestinationStateGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var tradeDetail in tradePeriods)
			{
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, tradeDetail.TradeDetail.Parent.OW_DestinationID);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var destinationStateGrouped = tradePeriods.GroupBy(x => new
			{
				x.TradeDetail.Parent.DestinationStateCode,
				x.TradeDetail.Parent.DestinationStateDescription,
				x.TradeDetail.Parent.DestinationCountryCode,
			});

			foreach (var grouping in destinationStateGrouped)
			{
				var description =
					Res.GetString("3127ea35-4b1a-4d35-af44-0f5cfa6035a4", "Destination: {0}",
						GetStateGroupDescription(grouping.Key.DestinationStateDescription, grouping.Key.DestinationCountryCode));

				yield return new Grouping(description, grouping);
			}
		}
	}
}
