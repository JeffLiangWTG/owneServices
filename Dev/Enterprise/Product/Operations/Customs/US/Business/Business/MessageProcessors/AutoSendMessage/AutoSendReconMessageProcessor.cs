using System.Linq;
using System.Threading;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class AutoSendReconMessageProcessor : IProcessor
	{
		public AutoSendReconMessageProcessor(JobDeclaration declaration)
		{
			reconDeclaration = declaration.ReconDeclaration ?? ReconDeclaration.Get(declaration);
		}
		readonly ReconDeclaration reconDeclaration;

		void IProcessor.Process(INotifications notifications, CancellationToken token)
		{
			var canSendOriginalMessage = true;
			var declaration = reconDeclaration.ReconWrappedJobDeclaration;
			if (reconDeclaration.OriginalEntries.Count == 0)
			{
				canSendOriginalMessage = false;
				notifications.AddWarning(ZString.Format($"System cannot send a reconciliation Add message for Job:{declaration.JE_DeclarationReference}, please enter at least one entry record under Recon Declaration > Entries."));
			}
			else if (!reconDeclaration.CanSendOriginal)
			{
				canSendOriginalMessage = false;
				notifications.AddWarning(ZString.Format($"System cannot send a reconciliation Add message for Job:{declaration.JE_DeclarationReference}, as the entry has already been added."));
			}
			else if (reconDeclaration.IsWaitingForRespones)
			{
				canSendOriginalMessage = false;
				notifications.AddWarning(ZString.Format($"System cannot send a reconciliation Add message for Job:{declaration.JE_DeclarationReference}, please check whether the job is waiting for response from customs."));
			}
			else if (reconDeclaration.ReconEntryNumber.IsEmpty && reconDeclaration is IAllocateNumberSupporter supporter)
			{
				declaration.ReloadImportEntryNumber();
				if (reconDeclaration.ReconEntryNumber.IsEmpty && !supporter.LockNumberAllocationMutex)
				{
					canSendOriginalMessage = false;
					notifications.AddWarning(ZString.Format($"System cannot send a reconciliation Add message for Job:{declaration.JE_DeclarationReference}, as {supporter.GetNumberAllocationMutexLockInfo()} is in the process of allocating Entry Number for this job; system cannot send the data as it will result in a different Entry Number being allocated."));
				}
			}

			if (canSendOriginalMessage)
			{
				var reconIReconciliation = new ReconDeclarationIReconciliation(reconDeclaration);
				if (reconIReconciliation.AggregateReconciliationIndicator && !reconIReconciliation.IsNoChangeAggregate && !reconIReconciliation.EntryLineGroups.Any())
				{
					notifications.AddWarning(ZString.Format($"System cannot send a reconciliation Add message for Job:{declaration.JE_DeclarationReference}, as this job is marked as aggregate with changes, but there are no changed lines."));
				}
				else
				{
					try
					{
						if (declaration.LockSendCustomsMessageMutex)
						{
							var sendingAction = new ACEReconMessageSendingAction(reconIReconciliation, Messaging.Business.UpdateActionCode.Add);
							sendingAction.CertificationSignature = true;
							var messageManager = new ReconMessageManager(sendingAction);
							messageManager.PopulateMessage();
							notifications.Add(new Notification(CargoWise.ComponentModel.NotificationType.Information, ZString.Format($"Reconciliation Add message has been sent to customs for {declaration.JE_DeclarationReference}.")));
						}
						else
						{
							var mutexInfo = declaration.GetSendCustomsMessageMutexInfo();
							notifications.AddError(ZString.Format($"System cannot send a reconciliation Add message for Job:{declaration.JE_DeclarationReference}, as {mutexInfo} is trying to send the same message for this job. Please wait unitl the lock has been released before trying to send the message again."));
						}
					}
					finally
					{
						declaration.UnlockSendCustomsMessageMutex();
					}
				}
			}
		}
	}
}
