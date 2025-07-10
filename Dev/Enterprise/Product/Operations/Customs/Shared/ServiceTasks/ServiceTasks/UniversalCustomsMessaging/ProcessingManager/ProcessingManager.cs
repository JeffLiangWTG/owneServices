using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Utils;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.Semaphores.Common;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public abstract class ProcessingManager : CommonProcessingManager
	{
		protected ProcessingManager(string serviceTaskCode)
		{
			this.serviceTaskCode = Argument.NotNull(serviceTaskCode, nameof(serviceTaskCode));
		}
		readonly string serviceTaskCode;

		public override void Dispose()
		{
			BaseMessageProcessor?.Dispose();
			base.Dispose();
		}

		protected sealed override void ExecuteBatchCore(CancellationToken token)
		{
			if (MessageProcessor != null)
			{
				try
				{
					var failedMessagesManager = new FailedMessagesManager(RetryType.CurrentExecution, ShouldRetryOnException);
					var totalItemsProcessed = 0;
					individualMessagesToBeProcessedCount = 0;
					BaseMessageProcessor<EDIMessage>.DisposableBatch batch;
					var hasExceededLimit = false;
					var messagesPerExecution = MessagesPerExecution;
					do
					{
						token.ThrowIfCancellationRequested();
						using (batch = BaseMessageProcessor.RetrieveNextProcessableMessages(DequeueMessages, failedMessagesManager))
						{
							messagesAfterLastSave = 0;
							totalItemsProcessed += batch.ItemsInBatchCount;
							if (batch.Length > 0)
							{
								ProcessBatch(batch, failedMessagesManager);
							}
						}
						if (!(totalItemsProcessed < messagesPerExecution))
						{
							hasExceededLimit = true;
							break;
						}
					}
					while (!batch.IsLastBatch);
					if (hasExceededLimit)
					{
						RequeueCurrentApplicationCode();
					}
				}
				finally
				{
					semaphoreHandle?.Dispose();
					semaphoreHandle = null;
					BaseMessageProcessor.Dispose();
					BaseMessageProcessor = null;
					MessageProcessor = null;
				}
			}
		}
		int individualMessagesToBeProcessedCount;

		protected abstract int MessagesPerExecution { get; }

		protected override void OnApplicationCodeChanged(string oldApplicationCode)
		{
			BaseMessageProcessor?.Dispose();
			semaphoreHandle?.Dispose();
			var semaphore = new UCMPSemaphoreType(serviceTaskCode, ApplicationCode, MaxConcurrentHandles);
			semaphoreHandle = EnvProxy.Instance.SemaphoreProvider.CreateSemaphoreHandle(semaphore);
			MessageProcessor = semaphoreHandle?.Success ?? false ? GetMessageProcessor(ApplicationCode) : null;
			if (MessageProcessor == null)
			{
				BaseMessageProcessor = null;
				semaphoreHandle?.Dispose();
			}
			else
			{
				BaseMessageProcessor = new MessageProcessorExploder(Logger, MessageProcessor, GetNewFactory, GetQueuedQuery);
			}
		}

		protected int MaxConcurrentHandles
		{
			get
			{
				if (!maxConcurrentHandles.HasValue)
				{
					var query = new ZQuery(StmScheduleTaskSchema.S5_ScheduleType, serviceTaskCode);
					query.AddToFilter(StmScheduleTaskSchema.S5_TypeOfDocument, UCMServiceTask.ServiceTaskCategory);
					query.AddToFilter(StmScheduleTaskSchema.S5_ParentTableCode, Core.Constants.ServiceTask.ParentTableCode);
					query.OrderBy = StmScheduleTaskSchema.Constants.S5_SystemCreateTimeUtc;
					var taskSchedule = new BusinessObjectFactory().LoadTop1<ServiceTaskSchedule>(query);
					var maxCount = 1;
					if (taskSchedule != null)
					{
						maxCount = taskSchedule.SecondaryProcessesMaxCount + 1;
					}
					switch (maxCount)
					{
						case 1:
						case 2:
							maxConcurrentHandles = 1;
							break;
						default:
							var percentage = CustomsDataRegistry.Instance.UCMResourcePercentageUsage.Value / 100m;
							if (percentage < 0m)
							{
								percentage = 0.01m;
							}
							else if (percentage > 1m)
							{
								percentage = 1m;
							}
							maxConcurrentHandles = Math.Min((ZInt)Math.Ceiling(percentage * maxCount), maxCount - 1);
							break;
					}
				}
				return maxConcurrentHandles.Value;
			}
		}
		int? maxConcurrentHandles;

		ISemaphoreHandle semaphoreHandle;
		protected MessageProcessorExploder BaseMessageProcessor { get; private set; }
		protected UniversalCustomsApplicationTypeMessageProcessor MessageProcessor { get; private set; }

		protected virtual bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => MessageProcessor.ShouldMessageBeProcessedInASeparateFactory(message);
		protected abstract UniversalCustomsApplicationTypeMessageProcessor GetMessageProcessor(string applicationCode);

		protected bool ShouldRetryOnException(int currentExceptionsCount, EDIMessage message, Exception lastException, int maxRetryAttempt, string additionalErrorReportMessage = null)
		{
			if (currentExceptionsCount > maxRetryAttempt)
			{
				ErrorReporter.ReportOnce($"Messages should eventually be failed. Message Logs: {string.Join(System.Environment.NewLine, Logger.Logs)}", lastException);
				return false;
			}

			var shouldRetry = BaseMessageProcessor.ShouldRetryOnException(currentExceptionsCount, message, lastException, maxRetryAttempt, additionalErrorReportMessage);
			if (!shouldRetry)
			{
				return false;
			}

			if (lastException is ZCannotSaveException cannotSaveException)
			{
				return cannotSaveException.ShouldReprocess;
			}

			if (lastException is ZSaveConcurrencyException
				|| IsSQLTimeoutOrDeadlockException(lastException)
				|| lastException is TransactionException
				|| (lastException is ZSaveException saveException && saveException.CanRecover)
				|| (lastException is SqlException sqlEx && new DbErrorMatch(sqlEx).ExceptionType == DbErrorType.GeneralNetworkError))
			{
				return true;
			}

			return false;
		}

		bool IsSQLTimeoutOrDeadlockException(Exception e)
		{
			return e.IsExceptionPresentIncludingInner<SqlException>(s => s.IsTimeoutExpired() || s.IsInnermostDeadlock() || s.IsLockTimeoutExpired());
		}

		protected virtual bool ShouldSetupMessageBranchEnvironment => false;

		void ProcessBatch(BaseMessageProcessor<EDIMessage>.DisposableBatch messages, FailedMessagesManager failedMessagesManager)
		{
			IDisposable branchSwitching = null;
			try
			{
				var indexOfLastMessage = messages.Length - 1;
				bool? currentSeparateFactory = null;
				for (int i = 0; i < messages.Length; i++)
				{
					var message = messages[i];

					using (PerformanceStatisticsCollector.StartMonitoring("ProcessMessage", message.EM_ApplicationCode + "-" + message.EM_MessageType + "-" + message.EM_MessageSubType))
					{
						if (ShouldProcess(message))
						{
							messagesAfterLastSave++;
							bool shouldMessageBeProcessedInASeparateFactory = false;
							HandleSave(message, failedMessagesManager, () =>
							{
								shouldMessageBeProcessedInASeparateFactory =
									ShouldMessageBeProcessedInASeparateFactory(message);
								if (currentSeparateFactory.HasValue)
								{
									if (shouldMessageBeProcessedInASeparateFactory && !currentSeparateFactory.Value)
									{
										SavePreviousMessageChanges(messages, failedMessagesManager, i);
									}

									currentSeparateFactory = shouldMessageBeProcessedInASeparateFactory;
								}
								else
								{
									currentSeparateFactory = shouldMessageBeProcessedInASeparateFactory;
								}
							});
							if (ShouldSetupMessageBranchEnvironment)
							{
								var branchPK = message.EM_GB;
								if (branchPK != GlbBranch.CurrentBranch.PK)
								{
									if (branchSwitching != null)
									{
										SavePreviousMessageChanges(messages, failedMessagesManager, i);
										branchSwitching.Dispose();
									}
									branchSwitching = DisposableEnvironment.ForBranch(branchPK.ToGuid());
								}
							}
							ProcessMessage(shouldMessageBeProcessedInASeparateFactory, message, i == indexOfLastMessage, failedMessagesManager);
						}
					}
				}

				messages.Notify();
				ProcessBatchCleanUp();
			}
			catch (BatchProcessorOperationCancelledException)
			{
				throw;
			}
			catch (Exception e) when (!e.IsCriticalException() || e.Find<SqlLockLostException>() != null)
			{
				if (messagesAfterLastSave > 1)
				{
					Log(FormattableString.Invariant($"Exception processing a group of {messagesAfterLastSave} messages: [{e.Message}]"), LogType.Warning);
				}
				individualMessagesToBeProcessedCount = messagesAfterLastSave;
			}
			finally
			{
				branchSwitching?.Dispose();
			}
		}

		void SavePreviousMessageChanges(BaseMessageProcessor<EDIMessage>.DisposableBatch messages, FailedMessagesManager failedMessagesManager, int i)
		{
			var previousMessage = messages[i - 1];
			HandleSave(previousMessage, failedMessagesManager, () =>
			{
				if (ShouldSaveMessage(false, previousMessage, true))
				{
					SaveAfterProcessingMessages(previousMessage.Factory);
				}
			});
		}

		protected virtual void ProcessBatchCleanUp() { }

		void ProcessMessage(bool shouldMessageBeProcessedInASeparateFactory, EDIMessage message, bool isLastMessage, FailedMessagesManager failedMessagesManager)
		{
			HandleSave(message, failedMessagesManager, () =>
			{
				EDIMessage messageToProcess;
				using (GetMessageToProcess(shouldMessageBeProcessedInASeparateFactory, message, out messageToProcess))
				{
					ProcessMessage(messageToProcess);
					if (ShouldSaveMessage(shouldMessageBeProcessedInASeparateFactory, messageToProcess, isLastMessage))
					{
						SaveAfterProcessingMessages(messageToProcess.Factory);
					}
				}
			});
		}

		void HandleSave(EDIMessage message, FailedMessagesManager failedMessagesManager, Action action)
		{
			try
			{
				action();
			}
			catch (Exception e) when (!e.IsCriticalException() && !e.IsOutOfDiskSpaceException() && !e.IsUnableToCreateTempFileException())
			{
				HandleSaveExceptionBeforeRetrying(e);
				if (messagesAfterLastSave == 1)
				{
					LoggingExceptionProcessingMessage(message, e);
					var errorFactory = new BusinessObjectFactory { NameForDebugging = "Message Processing Mark Exception" };
					errorFactory.SuspendValidation();
					failedMessagesManager.MarkMessageAsHavingException(errorFactory, message, e, EDIMessage.Status.Failed, BaseMessageProcessor.IsMessageQueued, Logger, hasIncrementedCount: false, MessageProcessor.PostProcessOnException);
				}
				throw;
			}
			finally
			{
				if (individualMessagesToBeProcessedCount > 0)
				{
					--individualMessagesToBeProcessedCount;
				}
			}
		}

		void SaveAfterProcessingMessages(BusinessObjectFactory factory)
		{
			factory.Save();
			messagesAfterLastSave = 0;
		}
		protected int messagesAfterLastSave;

		protected virtual IDisposable GetMessageToProcess(bool shouldMessageBeProcessedInASeparateFactory, EDIMessage message, out EDIMessage messageToProcess)
		{
			IDisposable result = null;
			messageToProcess = message;
			if (shouldMessageBeProcessedInASeparateFactory)
			{
				var newFactory = new BusinessObjectFactory { NameForDebugging = Messaging.Business.BaseMessageProcessor.FactoryNameForDebugging, RefreshEnabled = false };
				newFactory.SuspendValidation();
				result = newFactory.AddDisposableService();
				messageToProcess = newFactory.Load<EDIMessage>(message.PK);
			}
			return result;
		}

		protected virtual bool ShouldSaveMessage(bool shouldMessageBeProcessedInASeparateFactory, EDIMessage message, bool isLastMessage) => isLastMessage || shouldMessageBeProcessedInASeparateFactory || messagesAfterLastSave == MessagesPerSave;

		int MessagesPerSave => individualMessagesToBeProcessedCount > 0 ? 1 : 50;

		protected virtual bool ShouldProcess(EDIMessage message) => true;

		protected void LoggingExceptionProcessingMessage(EDIMessage message, Exception ex)
		{
			Log(Res.GetString("40FE8394-DB28-4E6F-A9BB-6D0A6D2E57DD", "Exception processing message ({0}): [{1}]", GetMessageLoggingDetail(message), ex.Message), LogType.Warning);
		}

		protected void HandleSaveExceptionBeforeRetrying(Exception e)
		{
			if (e is ZSaveException)
			{
				try
				{
					var notifier = new NullNotificationHandler();
					ZExceptionReporting.HandleSaveException(e, notifier);
					if (!notifier.Message.IsEmpty)
					{
						Log(notifier.Message, LogType.Warning);
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					// Continue handling initial exception
				}
			}
		}

		protected virtual ZQuery GetQueuedQuery() => new ZQuery(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
		protected virtual BusinessObjectFactory GetNewFactory() => new BusinessObjectFactory();

		protected abstract void ProcessMessage(EDIMessage message);

		protected abstract BaseMessageProcessor<EDIMessage>.DisposableBatch DequeueMessages(ZQuery query);

		protected sealed class MessageProcessorExploder : BaseMessageProcessor<EDIMessage>
		{
			readonly UniversalCustomsApplicationTypeMessageProcessor messageProcessor;
			readonly Func<BusinessObjectFactory> getNewFactory;
			readonly Func<ZQuery> getQueuedQuery;

			public MessageProcessorExploder(LoggingInformation logger, UniversalCustomsApplicationTypeMessageProcessor messageProcessor, Func<BusinessObjectFactory> getNewFactory, Func<ZQuery> getQueuedQuery)
				: base(logger)
			{
				this.messageProcessor = messageProcessor;
				this.getNewFactory = getNewFactory;
				this.getQueuedQuery = getQueuedQuery;
			}

			public new List<ApplicationTypeMessageProcessor> MessageProcessors => base.MessageProcessors;
			public new DisposableBatch CreateEmptyBatch() => base.CreateEmptyBatch();
			public new BusinessObjectFactory GetNewFactory() => base.GetNewFactory();
			public new bool IsMessageQueued(EDIMessage message) => base.IsMessageQueued(message);

			protected override BusinessObjectFactory GetNewFactoryCore() => getNewFactory();
			protected override List<ApplicationTypeMessageProcessor> GetMessageProcessors() => new List<ApplicationTypeMessageProcessor>(new[] { messageProcessor });
			public override ApplicationTypeMessageProcessor GetApplicationTypeProcessorCore(EDIMessage message) => messageProcessor;
			protected override int MessagesPerSaveCore => 1;
			protected override ZQuery ValidBranchesForMessageFilter => new ZQuery();
			protected override ZQuery GetQueuedQuery() => getQueuedQuery();
			protected override EDIMessageOrder MessageOrder => EDIMessageOrder.CreateTime;
			protected override void SortProcessableMessageEvenFurther(EDIMessage[] messages) { }
		}

		class NullNotificationHandler : INotificationHandler
		{
			ZString message;

			public ZString Message { get => message; }

			public void ReportInformation(string message, string caption)
			{
				// Continue handling exception
			}

			public void ReportError(string message, string caption, string errorContext = null, Exception exception = null)
			{
				this.message = message + "\n" + caption;
				// Continue handling exception
			}
		}
	}
}
