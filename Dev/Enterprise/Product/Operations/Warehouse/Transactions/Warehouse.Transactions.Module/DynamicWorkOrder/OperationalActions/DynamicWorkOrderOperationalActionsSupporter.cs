using System;
using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class DynamicWorkOrderOperationalActionSupporter : WhsOperationalActionSupporter
	{
		public override SecurityCheckpoint BaseCheckpoint => Env.Security.WhsDynamicWorkOrder;

		public override BusinessContext BusinessContext => BusinessContext.WhsDynamicWorkOrder;

		public override Type RootType => typeof(WhsDynamicWorkOrder);
	}
}
