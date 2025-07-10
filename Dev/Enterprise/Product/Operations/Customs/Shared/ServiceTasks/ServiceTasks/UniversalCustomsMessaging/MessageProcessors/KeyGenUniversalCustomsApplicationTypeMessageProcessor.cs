using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.GraphEngine.ServiceTasks;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	sealed class KeyGenUniversalCustomsApplicationTypeMessageProcessor : UniversalCustomsApplicationTypeMessageProcessor
	{
		public KeyGenUniversalCustomsApplicationTypeMessageProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsMessageProcessor customsMessageProcessor)
			: base(logger, applicationCode, customsMessageProcessor)
		{
			this.GrEngine = new EDIMessageGrEngine(logger, applicationCode, GrEngineServiceSetting.KeyGen);
		}
		public readonly EDIMessageGrEngine GrEngine;

		protected override ZString[] StatusesToInclude => new ZString[] { EDIMessage.Status.Queued };
		protected sealed override bool RequiresPreProcessingCore => false;

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			var (linkedBusinessObjectMetaData, discardReason) = customsMessageProcessor.GetLinkedBusinessObjectMetaData(message, Logger) ?? LinkedBusinessObjectMetaData.Empty;
			if (discardReason.IsEmpty)
			{
				(var branchPk, discardReason) = customsMessageProcessor.GetBranch(message, Logger, linkedBusinessObjectMetaData?.BranchPk ?? ZGuid.Empty) ?? ZGuid.Empty;
				if (discardReason.IsEmpty)
				{
					(var serializationKeysResult, discardReason) = customsMessageProcessor.GetSerializationKeysResult(message, Logger, linkedBusinessObjectMetaData) ?? SerializationKeysResult.SerialProcessingInReceivedOrder;
					if (discardReason.IsEmpty)
					{
						switch (serializationKeysResult.ResultType)
						{
							case SerializationKeysResult.SerializationKeysResultType.KeysProvided:
								if (serializationKeysResult.Keys.IsNullOrEmpty() || serializationKeysResult.Keys.Any(x => string.IsNullOrEmpty(x)))
								{
									var key = $"Message (Application:{message.EM_ApplicationCode}, Type:{message.EM_MessageType}) specified result type {nameof(SerializationKeysResult.SerializationKeysResultType.KeysProvided)}, but did not specify any keys.";
									ErrorReporter.ReportOnce(key, $"{customsMessageProcessor.GetType().FullName}.{nameof(customsMessageProcessor.GetSerializationKeysResult)} method must return at least one key when {nameof(serializationKeysResult.ResultType)} = {nameof(SerializationKeysResult.SerializationKeysResultType.KeysProvided)}.");
								}

								break;
							case SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing:
								if (serializationKeysResult.Keys == null || serializationKeysResult.Keys.Any())
								{
									var key = $"Message (Application:{message.EM_ApplicationCode}, Type:{message.EM_MessageType}) specified result type {nameof(SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing)}, but did not return an empty set of keys.";
									ErrorReporter.ReportOnce(key, $"{customsMessageProcessor.GetType().FullName}.{nameof(customsMessageProcessor.GetSerializationKeysResult)} method must return an empty set of keys when {nameof(serializationKeysResult.ResultType)} = {nameof(SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing)}.");
								}

								break;
							case SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder:
								if (serializationKeysResult.Keys == null || !serializationKeysResult.Keys.SequenceEqual([SerializationKeysResult.ForceSerialProcessingKey]))
								{
									var key = $"Message (Application:{message.EM_ApplicationCode}, Type:{message.EM_MessageType}) specified result type {nameof(SerializationKeysResult.SerializationKeysResultType.SerialProcessingInReceivedOrder)}, but did not return an empty set of keys.";
									ErrorReporter.ReportOnce(key, $"{customsMessageProcessor.GetType().FullName}.{nameof(customsMessageProcessor.GetSerializationKeysResult)} method must return only the {nameof(SerializationKeysResult.ForceSerialProcessingKey)} key when {nameof(serializationKeysResult.ResultType)} = {nameof(SerializationKeysResult.SerializationKeysResultType.UnconstrainedParallelProcessing)}.");
								}

								break;
						}

						var hasDifferentBranchPk = branchPk.IsValid && branchPk != message.EM_GB;
						var hasDifferentLinkUniqueId = linkedBusinessObjectMetaData.LinkUniqueID.IsValid && linkedBusinessObjectMetaData.LinkUniqueID != message.EM_LinkUniqueID;
						var hasDifferentLinkTable = !linkedBusinessObjectMetaData.LinkTableName.IsEmpty && linkedBusinessObjectMetaData.LinkTableName != message.EM_LinkTable;
						UpdateMessageAndSave(message, (messageToSave) =>
						{
							if (hasDifferentBranchPk)
							{
								messageToSave.EM_GB = branchPk;
							}

							if (hasDifferentLinkUniqueId)
							{
								messageToSave.EM_LinkUniqueID = linkedBusinessObjectMetaData.LinkUniqueID;
							}

							if (hasDifferentLinkTable)
							{
								messageToSave.EM_LinkTable = linkedBusinessObjectMetaData.LinkTableName;
							}
							messageToSave.EM_Status = EDIMessage.Status.PreProcessedOK;
						}, () =>
						{
							GrEngine.PreEnqueuer.Enqueue(message, serializationKeysResult.Keys.ToArray());
						});
						return;
					}
				}
			}

			UpdateMessageAndSave(message, (messageToSave) =>
			{
				messageToSave.EM_Status = EDIMessage.Status.Discarded;
				messageToSave.Notes.AddNew(true, Res.GetString("{1C5247E6-59DE-4480-B6FB-02B5B336C399}", "Discard Reason"), discardReason);
				Logger.AddWarning(Res.GetString("F506DA60-DD53-408A-A1E3-EA5D8979BD4C", "Status set to Discarded due to the following reason: {0}", discardReason));
			});
		}

		void UpdateMessageAndSave(EDIMessage message, Action<EDIMessage> updateMessage, Action preCommitTransactionAction = null)
		{
			using (var transactionManager = Db.Connection.BeginTransactionWithManager())
			{
				var messageFactory = new BusinessObjectFactory { RefreshEnabled = false, NameForDebugging = "Key Gen Universal Customs Message Saving" };
				messageFactory.SuspendValidation();
				using (messageFactory.AddDisposableService())
				{
					EDIMessage messageToSave = null;
					if (((IBusinessObjectState)message).HasChangesNotIncludingChildren)
					{
						messageToSave = messageFactory.Load<EDIMessage>(message.PK);
						ErrorReporter.ReportOnce($"{customsMessageProcessor.GetType().FullName} should not modify the message during UCK processing.");
					}
					else
					{
						messageToSave = (EDIMessage)messageFactory.ImportFromAnotherFactory(message); // Need to copy the data before getting keys in case
					}

					updateMessage(messageToSave);
					messageFactory.Save();
				}
				preCommitTransactionAction?.Invoke();
				transactionManager.CommitTransaction();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Service task name")]
		protected override string MessageFriendlyNameCore => "Key Generation Universal Customs Messaging";
	}
}
