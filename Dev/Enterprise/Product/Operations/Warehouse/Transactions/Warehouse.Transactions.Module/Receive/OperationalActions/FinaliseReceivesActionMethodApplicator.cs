using System;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class FinaliseReceivesActionMethodApplicator : FinalizeDocketsActionMethodApplicator<WhsReceive>
	{
		public FinaliseReceivesActionMethodApplicator(BusinessObjectFactory factory)
			: base(Res.GetString("2711616f-dcc4-4d18-8761-52098d9bdce0", "Finalize Receives"), factory)
		{
		}

		protected override bool CanTryToFinaliseDocketCore(WhsReceive targetInOtherFactory, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var result = true;
			if (targetInOtherFactory.IsCreatedFromPickByBOM)
			{
				result = false;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning, OutputTextFormat, targetInOtherFactory.Description, targetLink, Res.GetString("37c0b4eb-7a1d-4bc6-a2be-60b2144baf88", "You cannot finalize receives created from Pick Orders."));
			}
			return result;
		}

		protected override bool ProcessSaveException(Exception exception, WhsReceive target, IOperationalActionSectionLog log, LogControllerLink targetLink)
		{
			var result = false;
			if (exception.IsWhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockTriggerException())
			{
				result = true;
				log.NotifyFormat(OperationalActionLogErrorLevel.Warning,
					OutputTextFormat,
					target.Description,
					targetLink,
					WhsExceptionHandler.WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStockMsgForServiceTask);
			}
			return result;
		}
	}
}
