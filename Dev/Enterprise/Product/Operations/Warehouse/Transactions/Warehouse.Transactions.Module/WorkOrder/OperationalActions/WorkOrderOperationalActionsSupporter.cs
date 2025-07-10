using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WorkOrderOperationalActionSupporter : WhsOperationalActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsWorkOrder;

		public override BusinessContext BusinessContext => BusinessContext.WhsWorkOrder;

		public override Type RootType => typeof(WhsWorkOrder);
	}
}