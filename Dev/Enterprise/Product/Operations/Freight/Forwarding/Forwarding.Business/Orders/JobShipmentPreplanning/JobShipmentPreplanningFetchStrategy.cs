using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class JobShipmentPreplanningFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public JobShipmentPreplanningFetchStrategy(JobShipmentPreplanning preplanning)
			: base(preplanning)
		{
		}

		JobShipmentPreplanning JobShipmentPreplanning => BusinessObject as JobShipmentPreplanning;

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			if (columns.Any(c => c.ColumnName == nameof(JobShipmentPreplanning.PreAdviceLockedMessage)))
			{
				Factory.AddFetchHint(JobOrderHeaderSchema.JD_EF_ShipmentPrePlanning, BusinessObject.PK);
			}

			if (columns.Any(c => c.ColumnName == "BuyerPK"))
			{
				Factory.AddFetchHint(typeof(OrgAddress), JobShipmentPreplanning.EF_OA_BuyerAddress);
			}

			base.FetchForViewCore(columns);
		}
	}
}
