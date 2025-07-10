using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class FinaliseOrdersActionMethodApplicator : FinalizeDocketsActionMethodApplicator<WhsOrder>
	{
		public FinaliseOrdersActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("fa3db675-f712-49bf-9467-d8c8c36766ba", "Finalize Orders"), factory)
		{
		}

		protected override bool CanTryToFinaliseDocketCore(WhsOrder targetInOtherFactory, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var result = true;
			var pick = targetInOtherFactory.Pick;
			if (pick == null)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, targetInOtherFactory.Description, targetLink, Res.GetString("59649f93-e30b-47a2-bd08-7ba30d2a6ac7", "is not picked. It must be picked before it can be finalized."));
			}
			return result;
		}

		protected override bool ProcessSaveException(Exception exception, WhsOrder target, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var result = false;
			if (exception.IsWhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectTriggerException())
			{
				result = true;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					target.Description,
					targetLink,
					WhsExceptionHandler.WhsDocketLineAndWhsPickLine_TransactionAndPickedQtyIsCorrectMsg_ForServiceTask);
			}
			return result;
		}
	}
}
