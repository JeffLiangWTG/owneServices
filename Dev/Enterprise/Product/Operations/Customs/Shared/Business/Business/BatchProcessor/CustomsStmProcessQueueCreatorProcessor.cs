using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public class CustomsStmProcessQueueCreatorProcessor : IProcessor
	{
		public CustomsStmProcessQueueCreatorProcessor(BusinessObject bizObj, ZString applicationCode, ZString triggerActionCode)
		{
			BizObj = Argument.NotNull(bizObj, nameof(bizObj));
			ApplicationCode = applicationCode;
			TriggerActionCode = triggerActionCode;
		}

		public BusinessObject BizObj { get; private set; }
		public ZString ApplicationCode { get; private set; }
		public ZString TriggerActionCode { get; private set; }

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			token.ThrowIfCancellationRequested();
			var queuedItem = CustomsStmProcessQueueLoader.LoadOrCreate(BizObj, ApplicationCode, TriggerActionCode);
			if (queuedItem != null)
			{
				notifications.AddWarning(ZString.Format("A record has been generated in StmProcessQueue table for {0}, PK={1}, Action Code = {2}.", BizObj.HumanReadableName, queuedItem.PK, TriggerActionCode));
			}
		}
	}
}
