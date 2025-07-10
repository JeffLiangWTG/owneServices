using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class TradePeriodSupplierPartGrouper : TradePeriodGrouper
	{
		protected override void AddFetchHintsCore(BusinessObjectFactory factory, IEnumerable<OrgTradePeriod> tradePeriods)
		{
			foreach (var period in tradePeriods)
			{
				factory.AddFetchHint(OrgSupplierPartSchema.Constants.TableName, period.TradeDetail.PA_OP);
			}
		}

		public override IEnumerable<Grouping> GetGroupings(IEnumerable<OrgTradePeriod> tradePeriods)
		{
			var partGrouped = tradePeriods.GroupBy(x => x.TradeDetail.PA_OP);
			foreach (var partGrouping in partGrouped)
			{
				var supplierPartDesc = partGrouping.First().TradeDetail.SupplierPartDescription;
				yield return new Grouping(supplierPartDesc, partGrouping);
			}
		}
	}
}
