using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodOriginCountryGrouper : TradePeriodGrouper
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
			var factory = tradePeriods.First().Factory;
			var originCountryGrouped = tradePeriods.GroupBy(x => new { x.TradeDetail.Parent.OriginCountryCode, x.TradeDetail.Parent.OriginCountryDescription });
			foreach (var grouping in originCountryGrouped)
			{
				var description = Res.GetString("04ff8487-1a22-49cf-95ff-b80f4b2bd70a", "Origin: {0}",
					GetCountryGroupDescription(grouping.Key.OriginCountryCode, grouping.Key.OriginCountryDescription));
				yield return new Grouping(description, grouping);
			}
		}
	}
}
