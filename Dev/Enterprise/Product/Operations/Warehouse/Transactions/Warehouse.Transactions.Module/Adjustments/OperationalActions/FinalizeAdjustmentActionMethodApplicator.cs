using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class FinaliseAdjustmentsActionMethodApplicator : FinalizeDocketsActionMethodApplicator<WhsAdjustment>
	{
		public FinaliseAdjustmentsActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("a7f93bfb-0c93-4e4b-ae5b-85f90122c0cb", "Finalize Adjustments"), factory)
		{
		}

		protected override bool ProcessSaveException(Exception exception, WhsAdjustment target, IOperationalActionSectionLog log, LogControllerLink targetLink)
			=> false;
	}
}
