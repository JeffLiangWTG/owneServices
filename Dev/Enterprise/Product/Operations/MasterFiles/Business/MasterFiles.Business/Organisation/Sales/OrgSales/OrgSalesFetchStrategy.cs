using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class OrgSalesFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public OrgSalesFetchStrategy(OrgSales sales)
			: base(sales)
		{
		}

		#region FetchForView

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);

			var sales = (OrgSales)BusinessObject;
			if (columns.Any(col => col.ColumnName == OrgSalesSchema.Constants.OW_OriginID))
			{
				Factory.AddFetchHint(ViewLocationSchema.Constants.TableName, sales.OW_OriginID);
			}

			if (columns.Any(col => col.ColumnName == OrgSalesSchema.Constants.OW_DestinationID))
			{
				Factory.AddFetchHint(ViewLocationSchema.Constants.TableName, sales.OW_DestinationID);
			}

			Factory.AddFetchHint(typeof(OrgTradeDetail), OrgTradeDetailSchema.PA_OW, sales.PK);
		}

		#endregion
	}
}
