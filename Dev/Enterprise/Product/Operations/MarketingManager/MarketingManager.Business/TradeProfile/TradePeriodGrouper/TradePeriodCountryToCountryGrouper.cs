using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodCountryToCountryGrouper : TradePeriodGrouper
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
			var countryToCountryGrouped = tradePeriods.GroupBy(x => new
			{
				x.TradeDetail.Parent.OriginCountryCode,
				x.TradeDetail.Parent.OriginCountryDescription,
				x.TradeDetail.Parent.DestinationCountryCode,
				x.TradeDetail.Parent.DestinationCountryDescription,
			});

			foreach (var grouping in countryToCountryGrouped)
			{
				var description =
						GetCountryGroupDescription(grouping.Key.OriginCountryCode, grouping.Key.OriginCountryDescription)
						+ " -> "
						+ GetCountryGroupDescription(grouping.Key.DestinationCountryCode, grouping.Key.DestinationCountryDescription);
				yield return new Grouping(description, grouping);
			}
		}
	}
}
