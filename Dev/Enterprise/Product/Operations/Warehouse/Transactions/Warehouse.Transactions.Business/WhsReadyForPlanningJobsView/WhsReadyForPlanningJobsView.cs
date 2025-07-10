using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReadyForPlanningJobsView(BusinessObjectFactory factory, DataRow row) : AutoWhsReadyForPlanningJobsView(factory, row)
	{
		public override bool IsSavedByFactory => false;
	}
}
