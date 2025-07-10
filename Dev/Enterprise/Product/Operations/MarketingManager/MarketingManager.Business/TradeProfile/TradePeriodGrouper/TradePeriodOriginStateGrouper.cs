using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodOriginStateGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(ViewLocationSchema.Constants.TableName, period.TradeDetail.Parent.OW_OriginID);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var originStateGrouped = tradePeriods.GroupBy(x => new
			{
				x.TradeDetail.Parent.OriginStateCode,
				x.TradeDetail.Parent.OriginStateDescription,
				x.TradeDetail.Parent.OriginCountryCode,
			});

			foreach (var grouping in originStateGrouped)
			{
				var description =
					Res.GetString("4cdefa24-58a3-456c-9fb1-75080df749be", "Origin: {0}",
						GetStateGroupDescription(grouping.Key.OriginStateDescription, grouping.Key.OriginCountryCode));

				yield return new Grouping(description, grouping);
			}
		}
	}
}
