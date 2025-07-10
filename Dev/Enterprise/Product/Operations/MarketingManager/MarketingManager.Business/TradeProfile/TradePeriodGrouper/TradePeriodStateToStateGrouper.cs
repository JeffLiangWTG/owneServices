using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodStateToStateGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var tradeDetail in tradePeriods)
			{
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, tradeDetail.TradeDetail.Parent.OW_OriginID);
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, tradeDetail.TradeDetail.Parent.OW_DestinationID);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var stateToStateGrouped = tradePeriods.GroupBy(x => new
			{
				x.TradeDetail.Parent.OriginStateCode,
				x.TradeDetail.Parent.OriginStateDescription,
				x.TradeDetail.Parent.OriginCountryCode,
				x.TradeDetail.Parent.DestinationStateCode,
				x.TradeDetail.Parent.DestinationStateDescription,
				x.TradeDetail.Parent.DestinationCountryCode,
			});

			foreach (var grouping in stateToStateGrouped)
			{
				var description =
						GetStateGroupDescription(grouping.Key.OriginStateDescription, grouping.Key.OriginCountryCode)
						+ " -> "
						+ GetStateGroupDescription(grouping.Key.DestinationStateDescription, grouping.Key.DestinationCountryCode);

				yield return new Grouping(description, grouping);
			}
		}
	}
}
