using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodCompanyGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var companyGrouped = tradePeriods.GroupBy(x => x.TradeDetail.Sales.OW_GC);
			foreach (var companyGrouping in companyGrouped)
			{
				var salesCompany = companyGrouping.First().TradeDetail.Sales.Company;
				var groupKey = salesCompany != null
					? $"{salesCompany.GC_Code}[{salesCompany.GC_RN_NKCountryCode}]"
					: Res.GetString("c5a46d4c-398d-40e2-bec8-d3fae1914bc2", "(Unknown Company)");
				yield return new Grouping(groupKey, companyGrouping);
			}
		}
	}
}
