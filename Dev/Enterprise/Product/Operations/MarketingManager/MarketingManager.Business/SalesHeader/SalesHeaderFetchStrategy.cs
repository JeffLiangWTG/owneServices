using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MarketingManager.Business
{
	public class SalesHeaderFetchStrategy : BusinessObjectFetchStrategy
	{
		public SalesHeaderFetchStrategy(SalesHeader salesHeader)
			: base(salesHeader)
		{
			this.salesHeader = salesHeader;
		}

		readonly SalesHeader salesHeader;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			if (columns.Any(x =>
				x.ColumnName == SalesHeader.Schema.TotalEstimatedAnnualValue ||
				x.ColumnName == SalesHeader.Schema.TotalEstimatedMonthlyAverage))
			{
				AddProspectFetchHint();
			}
		}

		void AddProspectFetchHint()
		{
			var prospectiveDetails = Factory.Load<OrgTradeDetail>(new ZQuery(OrgTradeDetailSchema.PA_OW, salesHeader.EntitySales.Select(x => x.PK)));
			foreach (var detail in prospectiveDetails)
			{
				Factory.AddFetchHint(OrgTradeProspectSchema.PAP_PA, detail.PK);
				Factory.AddFetchHint(OrgTradePeriodSchema.PAS_PA, detail.PK);
			}
		}
	}
}
