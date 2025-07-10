using System;
using System.Globalization;
using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class MessageReleasingManager : CommonProcessingManager
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		protected override void ExecuteBatchCore(CancellationToken token)
		{
			var connection = Db.Connection;
			const int MaxLoopsWithoutChanges = 3;
			var changed = 0;
			var totalChanged = 0;
			var loopsWithNoChanges = 0;
			var itemsLoaded = 0;
			var messagesPerExecution = CustomsDataRegistry.Instance.UCIMessagesPerExecution.Value;
			var grEngine = new EDIMessageGrEngine(Logger, ApplicationCode, GrEngineServiceSetting.Flipper);
			var lockKey = ApplicationCode + nameof(MessageReleasingManager);
			var setup = grEngine.Setup;
			var factory = setup.Factory;
			var hoursBeforeDelete = CustomsDataRegistry.Instance.ParallelUCMQueueHistoryInHours.Value;

			var lockResult = connection.RunLocked(lockKey, (_) =>
			{
				try
				{
					Log(string.Format(CultureInfo.InvariantCulture, "Releasing messages for {0}.", ApplicationCode), LogType.Information);
					var hasExceededLimit = false;
					do
					{
						token.ThrowIfCancellationRequested();
						var result = EnqueueGrEngine(grEngine, () => connection.HasAquiredLock(lockKey) ? null : "Cannot proceed with saving as lock was lost due to connection issues; will try again in another run.");
						changed = result.NewEntities.Count + result.OldEntities.Count;
						totalChanged += changed;
						itemsLoaded = grEngine.Enqueuer.ItemsLoaded;

						if (changed > 0)
						{
							grEngine.NudgeWorker();
							grEngine.NudgeKeyGen();
						}
						else if (itemsLoaded > 0)
						{
							if (loopsWithNoChanges == 0)
							{
								Log(string.Format(CultureInfo.InvariantCulture, "Could not release any of the [{0}] items remaining. Waiting for items to be processed.", grEngine.Enqueuer.ItemsLoaded), LogType.Debug); // SuppresCodeSmell Reason = Logs are English only.
							}

							++loopsWithNoChanges;
						}
						else
						{
							loopsWithNoChanges = 0;
						}

						if (connection.HasAquiredLock(lockKey))
						{
							factory.DeleteOldProcessed(Logger, hoursBeforeDelete);
						}
						else
						{
							break;
						}
						if (!(totalChanged < messagesPerExecution))
						{
							hasExceededLimit = true;
							break;
						}
					}
					while (loopsWithNoChanges < MaxLoopsWithoutChanges && itemsLoaded > 0);
					if (hasExceededLimit)
					{
						RequeueCurrentApplicationCode();
					}
					Log(string.Format(CultureInfo.InvariantCulture, "Messages Released: {0} Messages Loaded: {1} ", totalChanged, itemsLoaded), LogType.Information);
				}
				catch (OperationCanceledException ex)
				{
					Log(ex.Message, LogType.Debug);
				}
			});
			switch (lockResult)
			{
				case LockedProcessResult.AlreadyBeingProcessed:
					Log(string.Format(CultureInfo.InvariantCulture, "Another task is currently processing messages for {0}.", ApplicationCode), LogType.Debug);
					break;
				case LockedProcessResult.Error:
					Log(string.Format(CultureInfo.InvariantCulture, "Unable to get lock for {0} due to repeated connection issues; will try again in another run.", ApplicationCode), LogType.Debug);
					break;
			}
		}

		GrEngineEnqueuer<EDIMessage, EDIMessageQueueState>.LoadEntitiesResult EnqueueGrEngine(EDIMessageGrEngine<EDIMessage, EDIMessageQueueState> grEngine, Func<string> getReasonForNotSaving)
		{
			return grEngine.Enqueuer.Enqueue(new BusinessObjectFactory(), new Lazy<ZQuery>(() => GetGrEngineWatermarkQuery(grEngine)), Logger, getReasonForNotSaving: getReasonForNotSaving);
		}

		ZQuery GetGrEngineWatermarkQuery(EDIMessageGrEngine<EDIMessage, EDIMessageQueueState> grEngine)
		{
			var earliestMessageWithoutKeyGenerated = new ZDBOnlyQuery(typeof(EDIMessage));

			var notInEm = string.Format(CultureInfo.InvariantCulture, @"EM_PK NOT IN (
SELECT EQS_EM
FROM dbo.EDIMessageQueueState WITH (INDEX(NR_RX__EQS_EM), FORCESEEK)
WHERE EQS_ApplicationCode = {3} AND EQS_Status IN ('{0}', '{1}', '{2}'))",
				QueueStatusCodes.Codes.Blocked,
				QueueStatusCodes.Codes.PreKey,
				QueueStatusCodes.Codes.Queued,
				EDIMessageQueueStateFactory.ApplicationCodeParam);
			earliestMessageWithoutKeyGenerated.AddFilterAndZSQLParameterCollection(notInEm, new ZSqlParameterCollection(ZSqlParameter.New(EDIMessageQueueStateFactory.ApplicationCodeParam, ApplicationCode, EDIMessageQueueStateSchema.EQS_ApplicationCode)));
			earliestMessageWithoutKeyGenerated.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCode);
			earliestMessageWithoutKeyGenerated.AddToFilter(EDIMessageSchema.EM_Status, new[] { EDIMessage.Status.Queued, EDIMessage.Status.PreProcessedOK });
			earliestMessageWithoutKeyGenerated.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Receive);
			earliestMessageWithoutKeyGenerated.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			earliestMessageWithoutKeyGenerated.AddToFilter(BaseMessageProcessor.ValidTransmitDateMessageFilterUsingUtcTime);
			earliestMessageWithoutKeyGenerated.OrderBy = EDIMessageSchema.Constants.EM_SystemCreateTimeUtc + ", " + EDIMessageSchema.Constants.EM_MessageNum;

			var factory = new BusinessObjectFactory();
			var collection = new DynamicBusinessObjectCollection(factory);
			var filterParameterisedText = earliestMessageWithoutKeyGenerated.ParameterisedText;
			collection.Load($@"
SELECT TOP 1 EM_SystemCreateTimeUtc
FROM dbo.EDIMessage WITH (INDEX({EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum})) 
WHERE {filterParameterisedText.ParameterisedQueryText}
ORDER BY EM_SystemCreateTimeUtc, EM_MessageNum", filterParameterisedText.Parameters);
			var parentSystemCreateTimeUtc = collection.Count == 1 ? collection[0][EDIMessageSchema.EM_SystemCreateTimeUtc] : null;
			if (parentSystemCreateTimeUtc != null && parentSystemCreateTimeUtc is DateTime systemCreateTimeUtc)
			{
				return new ZQuery(EDIMessageQueueStateSchema.EQS_ParentSystemCreateTimeUtc, SQLComparisonOperator.LessThanOrEqualTo, systemCreateTimeUtc);
			}
			else
			{
				return new ZQuery();
			}
		}
	}
}
