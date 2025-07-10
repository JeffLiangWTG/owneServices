using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.PortHubs.Business
{
	class PortHubSelectionFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public PortHubSelectionFetchStrategy(PortHubSelection portHubSelection)
			: base(portHubSelection)
		{
		}

		PortHubSelection PortHubSelection => BusinessObject as PortHubSelection;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			if (columns.Any(c => c.ColumnName == PortHubSelection.Schema.DepotPK))
			{
				Factory.AddFetchHint(OrgAddressSchema.PK, PortHubSelection.TY_OA_DepotAddress);
			}

			base.FetchForViewCore(columns);
		}
	}
}
