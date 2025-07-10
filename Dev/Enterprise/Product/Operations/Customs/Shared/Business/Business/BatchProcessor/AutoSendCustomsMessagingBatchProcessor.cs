using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.ASYCUDA;
using static Enterprise.Integration.Customs.CA;
using static Enterprise.Integration.Customs.EUExitControl;
using static Enterprise.Integration.Customs.US.ISF;
using static Enterprise.Integration.Customs.US.USAMS;

namespace Enterprise.Customs.Business.BatchProcessor
{
	public class AutoSendCustomsMessagingBatchProcessor(LoggingInformation logger) : BaseCustomsStmProcessQueueBatchProcessor(logger)
	{
		protected override string ProcessQueueApplicationCode => CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging;

		protected override void ProcessQueuedItemCore(StmProcessQueue queuedItem)
		{
			if (queuedItem == null)
			{
				return;
			}

			var bizObj = queuedItem.Factory.Load(queuedItem.SW_ReferenceTableCode, queuedItem.SW_ReferenceID);
			if (bizObj != null)
			{
				Logger.Log($"Processing: {bizObj.HumanReadableName}");
			}

			var actionCode = queuedItem.SW_ActionCode;

			var messageProcessor = bizObj switch
			{
				IBaseAutoSendingMessageSupporter supporter => GetProcessorForAutoSendingMessageSupporter(supporter, actionCode, bizObj),
				ISendGlobalManifestMessageSupporter globalManifest when actionCode == WorkflowTriggerActionTypeConstants.Codes.SendGlobalManifest => globalManifest.CreateSendGlobalManifestMessageProcessor(),
				_ => null
			};

			messageProcessor?.Process(Logger, CancellationToken.None);

			IProcessor GetProcessorForAutoSendingMessageSupporter(IBaseAutoSendingMessageSupporter autoSendingSupporter, ZString actionCode, BusinessObject bizObj)
			{
				using (DisposableEnvironment.ForBranch(autoSendingSupporter.RegistryBranchPK))
				{
					return autoSendingSupporter switch
					{
						IJobDeclarationAutoSendingMessageSupporter declaration => GetMessageProcessorForJobDeclarationAutoSending(declaration, queuedItem, bizObj),
						ICustomsManifestMessageSupporter manifest when actionCode == WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage => manifest.CreateManifestMessageProcessor(),
						ICusISFAutoSendingMessageSupporter isf when actionCode == WorkflowTriggerActionTypeConstants.Codes.SendISFMessage => isf.CreateISFMessageWorkflowTriggerProcessor(),
						ISupportAutoSendExitReportTransferMessage exitReport when actionCode == WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage => exitReport.GetSendExitReportTransferMessageProcessor(bizObj),
						NO.IEmmaMessageGenerationProcessorProvider emma when actionCode == WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator => emma.GetEmmaMessageGenerationActionProcessor((NO.ICusEntryHeader)bizObj),
						_ => null
					};
				}
			}
		}

		IProcessor GetMessageProcessorForJobDeclarationAutoSending(IJobDeclarationAutoSendingMessageSupporter declarationSupporter, StmProcessQueue queuedItem, BusinessObject bizObj)
		{
			var actionCode = queuedItem.SW_ActionCode;
			if (actionCode == WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage)
			{
				if (declarationSupporter.SupportEntryDeclarationMessage)
				{
					return declarationSupporter.CreateEntryDeclarationMessageProcessor();
				}

				Logger.LogError(ZString.Format("Unable to send message for {0}. {1}", bizObj.HumanReadableName, declarationSupporter.GetReasonForNotSupportEntryDeclarationMessage));
			}
			else if (actionCode == WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage)
			{
				if (declarationSupporter.SupportReleaseMessage)
				{
					return declarationSupporter.CreateReleaseMessageProcessor(queuedItem.SW_EventCode);
				}

				Logger.LogError(ZString.Format("Unable to send message for {0}. {1}", bizObj.HumanReadableName, declarationSupporter.GetReasonForNotSupportReleaseMessage));
			}
			else if (actionCode == WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message)
			{
				var scheduleB3MessageSupporter = GetScheduleB3MessageSupporter(declarationSupporter as IJobDeclaration);
				if (scheduleB3MessageSupporter is { SupportScheduleB3Message: true })
				{
					return scheduleB3MessageSupporter.CreateScheduleB3MessageProcessor();
				}

				Logger.LogError(ZString.Format("Unable to Schedule CAD message for {0}. {1}", bizObj.HumanReadableName, scheduleB3MessageSupporter.NotSupportScheduleB3MessageReason));
			}

			return null;
		}

		IScheduleB3MessageSupporter GetScheduleB3MessageSupporter(IJobDeclaration declaration)
		{
			return declaration == null ? null : ObjectFactory.New<IScheduleB3MessageSupporter>(declaration);
		}
	}
}
