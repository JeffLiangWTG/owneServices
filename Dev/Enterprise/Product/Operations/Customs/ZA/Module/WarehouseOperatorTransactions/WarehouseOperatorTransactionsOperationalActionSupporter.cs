using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ZA.Module
{
	class WarehouseOperatorTransactionsOperationalActionSupporter : OperationalActionSupporter
	{
		public override Type RootType => typeof(CusWHSOperatorTransaction);

		public override CargoWise.Definitions.BusinessContext BusinessContext => CargoWise.Definitions.BusinessContext.WhsOperatorTransact;

		public override SecurityCheckpoint BaseCheckpoint => (SecurityCheckpoint)Env.Security.SecurityInstance.FindCheckPoint(ZASecurityCheckpoints.ZAWarehouseOperatorTransactions);

		protected override void PopulateMethods(OperationalActionMethodList list)
		{
			base.PopulateMethods(list);
			list.Add(ActionMethodProviderIDs.WarehouseOperatorTransactions);
		}
	}
}
