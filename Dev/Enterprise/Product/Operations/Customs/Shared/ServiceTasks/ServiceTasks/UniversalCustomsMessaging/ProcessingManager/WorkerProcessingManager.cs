using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	sealed class WorkerProcessingManager : ProcessingManager
	{
		public WorkerProcessingManager() : base(UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker) { }

		protected override bool ShouldProcess(EDIMessage message)
		{
			var result = true;
			if (message.EM_RetryCount > eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value)
			{
				FailMessage(message, $"Retry attempts EM_RetryCount exceeded the maximum limit {eAdaptorRegistry.Instance.RetryAttemptsOnUniversalXMLProcessingRecoverableErrors.Value}.");
				result = false;
			}
			else
			{
				message.EM_RetryCount++;
			}
			return result;
		}

		void FailMessage(EDIMessage message, string error)
		{
			var isolatedFactory = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "Failed Message Factory" };
			using (isolatedFactory.AddDisposableService())
			{
				isolatedFactory.SuspendValidation();
				var failedMessage = isolatedFactory.Load<EDIMessage>(message.PK);
				failedMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
				isolatedFactory.Save();
			}
			LoggingFailedProcessingMessage(message, error);
		}

		void LoggingStartingProcessingMessage(EDIMessage message)
		{
			Log(Res.GetString("5D1B6B63-86B5-4D9A-AA7D-BE88A9B0E9C6", "Starting processing Message ({0})", GetMessageLoggingDetail(message)), LogType.Information);
		}

		void LoggingFinishedProcessingMessage(EDIMessage message)
		{
			Log(Res.GetString("93580563-0FD5-4074-8EDC-FB4AF018D1E7", "Finished processing Message ({0})", GetMessageLoggingDetail(message)), LogType.Information);
		}

		void LoggingFailedProcessingMessage(EDIMessage message, string error)
		{
			Log(Res.GetString("C72F6E36-9C7B-4EC9-8071-0662546E7384", "Failed processing Message ({0}). {1}", GetMessageLoggingDetail(message), error), LogType.Warning);
		}

		protected override bool ShouldSetupMessageBranchEnvironment => true;
		protected override int MessagesPerExecution => CustomsDataRegistry.Instance.UCQMessagesPerExecution.Value;
		protected override void ProcessMessage(EDIMessage message)
		{
			LoggingStartingProcessingMessage(message);
			using (new FactorySaveAlerter(participants =>
				{
					if (participants.OfType<BusinessObjectFactory>().Any(f => f.NameForDebugging.Equals(Messaging.Business.BaseMessageProcessor.FactoryNameForDebugging)))
					{
						ExceptionReporter.Instance.ReportDeveloperException(
							$"Unexpected Factory.Save in [{UniversalCustomsMessagingConstants.ServiceTaskCodes.Worker}.{ApplicationCode}]",
							(NoResString)"UCMP is responsible for all Factory Save calls. Factory.Save should not be called by subscribers.", null);
					}
				}))
			{
				MessageProcessor.ProcessMessage(message);
			}
			LoggingFinishedProcessingMessage(message);
			if (lastProcessingTime == null || lastProcessingTime.Value < ZDateTime.UtcNow.AddMinutes(-3))
			{
				lastProcessingTime = ZDateTime.UtcNow;
				MessageProcessor.GrEngine.NudgeMaster();
			}
		}
		ZDateTime? lastProcessingTime;

		protected override void ProcessBatchCleanUp()
		{
			base.ProcessBatchCleanUp();
			if (lastProcessingTime.HasValue)
			{
				lastProcessingTime = null;
				MessageProcessor.GrEngine.NudgeMaster();
			}
		}

		protected override ZQuery GetQueuedQuery() => new ZQuery(EDIMessageSchema.EM_Status, new[] { EDIMessage.Status.Queued, EDIMessage.Status.PreProcessedOK });

		protected override UniversalCustomsApplicationTypeMessageProcessor GetMessageProcessor(string applicationCode)
		{
			var customsMessageProcessor = UniversalCustomsMessagingSubscribers.GetMessageProcessor(applicationCode);
			return customsMessageProcessor == null ? null : new WorkerUniversalCustomsApplicationTypeMessageProcessor(Logger, applicationCode, customsMessageProcessor, MaxConcurrentHandles);
		}

		new WorkerUniversalCustomsApplicationTypeMessageProcessor MessageProcessor => (WorkerUniversalCustomsApplicationTypeMessageProcessor)base.MessageProcessor;

		protected override BaseMessageProcessor<EDIMessage>.DisposableBatch DequeueMessages(ZQuery query)
		{
			var processor = MessageProcessor;
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
			return DequeueNextBatchWithExceptionHandling(processor.GrEngine, BaseMessageProcessor.GetNewFactory(), query);
		}

		BaseMessageProcessor<EDIMessage>.DisposableBatch DequeueNextBatchWithExceptionHandling(EDIMessageGrEngine grEngine, BusinessObjectFactory factory, ZQuery filter)
		{
			var batch = grEngine.Dequeuer.LoadBatch(factory, Logger);
			try
			{
				var unfilteredSet = batch.Load<EDIMessage>(factory);
				var messages = new List<EDIMessage>();
				var queuedStms = new List<EDIMessageQueueState>();
				var unqueuedStms = new List<EDIMessageQueueState>();
				foreach (var pair in batch.Join(unfilteredSet, m => m.MessagePK, s => s.PK, (queue, message) => (Queue: queue, Message: message)))
				{
					if (pair.Message.MatchesFilter(filter))
					{
						queuedStms.Add(pair.Queue);
						messages.Add(pair.Message);
					}
					else
					{
						unqueuedStms.Add(pair.Queue);
					}
				}

				grEngine.Dequeuer.Notify(unqueuedStms.Select(s => new QueueStateResult<EDIMessageQueueState>(s, QueueStateResultType.Failed)));

				return new BaseMessageProcessor<EDIMessage>.DisposableBatch(messages.ToArray(), new MessageStatusChangeNotifier<EDIMessageQueueState>(queuedStms.ToArray(), grEngine.Dequeuer, batch));
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				try
				{
					batch.Dispose();
				}
				catch (Exception panicEx) when (!panicEx.IsCriticalException())
				{
					ExceptionReporter.Instance.ReportException("1205B12A-A56C-4F37-B7B9-9DFD6B4B1ECA", panicEx);
				}
				throw;
			}
		}
	}
}
