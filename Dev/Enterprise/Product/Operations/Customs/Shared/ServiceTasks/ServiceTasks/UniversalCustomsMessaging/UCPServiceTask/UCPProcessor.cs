using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks
{
	public class UCPProcessor : IUniversalCustomsMessagingInterchangeProcessor
	{
		public UCPProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsEDIMessagePacker messagePacker, CancellationToken token)
		{
			this.logger = logger;
			this.applicationCode = applicationCode;
			this.token = token;
			this.messagePacker = messagePacker;
		}

		readonly LoggingInformation logger;
		readonly string applicationCode;
		readonly CancellationToken token;
		readonly IUniversalCustomsEDIMessagePacker messagePacker;

		public void Process()
		{
			var maxRetryCount = CustomsDataRegistry.Instance.UCPInterchangePackingMaxRetryCount.Value;
			var numberPerBatch = CustomsDataRegistry.Instance.UCPInterchangesPerBatch.Value;
			int messagesLengthPerBatch;
			do
			{
				token.ThrowIfCancellationRequested();
				var messagesInQueue = GetQueuedOutboundMessages(applicationCode, numberPerBatch);
				var messagesToPack = GetValidMessagesToPack(messagesInQueue, maxRetryCount);
				messagesLengthPerBatch = messagesToPack.Count();
				if (messagesLengthPerBatch > 0)
				{
					var messageGroups = messagesToPack.GroupBy(i => i.EM_GB);
					foreach (var group in messageGroups)
					{
						token.ThrowIfCancellationRequested();
						using (DisposableEnvironment.ForBranch(group.Key.ToGuid()))
						{
							logger.Log($"Start to process {group.Count()} {applicationCode} message(s) for {GlbCompany.CurrentCompany.GC_Code}/{GlbBranch.CurrentBranch.GB_Code}.");
							var factory = GetFactory((NoResString)"Messages and Interchanges Saving in batch");
							var ediMessageAndInterchangeToSave = new Dictionary<EDIMessage, EDIInterchange>();
							var interchangesNeedToSave = ProcessMessagesInBatch(factory, logger, group.ToArray(), messagePacker, ediMessageAndInterchangeToSave);

							if (interchangesNeedToSave > 0)
							{
								try
								{
									factory.Save();
									logger.Log($"{interchangesNeedToSave} {applicationCode} message(s) and packed Interchanges saved in batch.");
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									logger.LogWarning($"Unable to save {interchangesNeedToSave} {applicationCode} message(s) and interchange(s) in batch. Each message and interchange bundle will be saved separately with separate factory.");
									ProcessMessagesSeparately(ediMessageAndInterchangeToSave);
								}
							}
						}
					}
				}
			} while (messagesLengthPerBatch != 0);

			logger.Log($"No {applicationCode} message(s) to deal with.");
		}

		protected virtual BusinessObjectFactory GetFactory(string description) => new BusinessObjectFactory() { RefreshEnabled = true, NameForDebugging = description };

		static EDIMessage[] GetQueuedOutboundMessages(ZString applicationCode, int numberPerBatch)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(EDIMessage));
			var validTransmitDateMessageFilter = new ZQuery(EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.Equal, null);
			validTransmitDateMessageFilter.AddToFilter(JoinCondition.Or, EDIMessageSchema.EM_HeldUntilDate, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.UtcNow);
			query.AddToFilter(validTransmitDateMessageFilter);
			query.AddToFilter(EDIMessageSchema.EM_ApplicationCode, applicationCode);
			query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			query.AddToFilter(EDIMessageSchema.EM_Status, EDIMessage.Status.Queued);
			query.AddToFilter(EDIMessageSchema.EM_IsActive, true);
			query.OrderBy = EDIMessageSchema.EM_SystemCreateTimeUtc.Name;
			query.TableIndexHints.Add(new TableIndexHint(EDIMessageSchema.Constants.Indexes.NR_RX__EM_ApplicationCode_EM_ReceiveTransmit_EM_SystemCreateTimeUtc_EM_MessageNum));
			query.MaximumRows = numberPerBatch;
			return factory.Load<EDIMessage>(query);
		}

		IEnumerable<EDIMessage> GetValidMessagesToPack(EDIMessage[] messages, int maxRetryCount)
		{
			var result = new List<EDIMessage>();
			foreach (var message in messages)
			{
				if (!messagePacker.AllowEmptyMessageBody && message.EM_MessageText.IsEmpty && message.EM_MessageData == null)
				{
					FailMessage(message, EDIMessage.Status.Discarded, (NoResString)"Message Body is required to generate an EDIInterchange.");
					continue;
				}

				if (message.EM_RetryCount > maxRetryCount)
				{
					FailMessage(message, EDIMessage.Status.Failed, $"Max retry attempts {maxRetryCount} Reached.");
					continue;
				}

				result.Add(message);
			}

			return result;
		}

		int ProcessMessagesInBatch(BusinessObjectFactory factory, LoggingInformation logger, IEnumerable<EDIMessage> messages, IUniversalCustomsEDIMessagePacker interchangePacker, Dictionary<EDIMessage, EDIInterchange> ediMessageAndInterchangeToSave)
		{
			int interchangesNeedToSave = 0;
			foreach (var message in messages)
			{
				var messageToProcess = (EDIMessage)factory.ImportFromAnotherFactory(message);
				var interchangeToSave = factory.New<EDIInterchange>();
				ZString errorReason;
				try
				{
					errorReason = interchangePacker.Pack(messageToProcess, interchangeToSave, logger);
				}
				catch (Exception ex)
				{
					errorReason = ex.Message;
				}

				if (errorReason.IsEmpty)
				{
					messageToProcess.EM_EI = interchangeToSave.PK;
					messageToProcess.EM_Status = EDIMessage.Status.ProcessedOK;
					messageToProcess.Logs.AddNew(AutoEvents.InterchangeReady, ZDateTimeOffset.Now);
				}
				else
				{
					interchangeToSave.ContainedMessages.RemoveAll();

					messageToProcess.EM_RetryCount++;
					messageToProcess.EM_EI = ZGuid.Empty;

					interchangeToSave.Delete();
					interchangeToSave = null;

					logger.LogWarning($"Failed to pack {messageToProcess.EM_ApplicationCode} EDIMessage {messageToProcess.EM_MessageNum}. EDIMessage Retry count {messageToProcess.EM_RetryCount}. ErrorReason: {errorReason}");
				}

				ediMessageAndInterchangeToSave.Add(messageToProcess, interchangeToSave);
				interchangesNeedToSave++;
			}

			return interchangesNeedToSave;
		}

		void ProcessMessagesSeparately(Dictionary<EDIMessage, EDIInterchange> ediMessageAndInterchangeToSave)
		{
			foreach (var ediMessageAndInterchange in ediMessageAndInterchangeToSave)
			{
				try
				{
					var newFactory = GetFactory((NoResString)"Message and Interchange Saving Separately");
					var messageToSave = (EDIMessage)newFactory.ImportFromAnotherFactory(ediMessageAndInterchange.Key);
					messageToSave.HasChanges = true;
					if (ediMessageAndInterchange.Value != null)
					{
						var interchangeToSave = (EDIInterchange)newFactory.ImportFromAnotherFactory(ediMessageAndInterchange.Value);
						newFactory.Save();
						logger.Log($"The {messageToSave.EM_ApplicationCode} EDIMessage {messageToSave.EM_MessageNum} EM_Statue updated and packed Interchange {interchangeToSave.EI_InterchangeNum} saved.");
					}
					else
					{
						newFactory.Save();
						logger.Log($"The {messageToSave.EM_ApplicationCode} EDIMessage {messageToSave.EM_MessageNum} and packed failed, EM_RetryCount increased.");
					}
				}
				catch (Exception exSingle) when (!exSingle.IsCriticalException())
				{
					RetryMessage(ediMessageAndInterchange.Key);
				}
			}
		}

		void FailMessage(EDIMessage failedMessage, ZString status, ZString errorReason)
		{
			var isolatedFactory = GetFactory((NoResString)"Failed Message Packing Factory");
			var isolatedMessage = isolatedFactory.Load<EDIMessage>(failedMessage.PK);
			isolatedMessage.EM_Status = status;
			isolatedMessage.Notes.AddNew(true, Res.GetString("D9B719BA-BE77-4A88-AB1B-3A70DD604253", "Pack EDIMessage Error"), errorReason);
			isolatedFactory.Save();
			logger.LogWarning($"Failed to pack {failedMessage.EM_ApplicationCode} EDIMessage {failedMessage.EM_MessageNum}. {errorReason}");
		}

		void RetryMessage(EDIMessage retryMessage)
		{
			var isolatedFactory = GetFactory((NoResString)"Retry Message Packing Factory");
			var isolatedMessage = isolatedFactory.Load<EDIMessage>(retryMessage.PK);
			isolatedMessage.EM_Status = EDIMessage.Status.Queued;
			logger.LogWarning($"Unable to save {retryMessage.EM_ApplicationCode} message {retryMessage.EM_MessageNum} and interchange Separately. Message Retry count {retryMessage.EM_RetryCount}.");
			isolatedMessage.EM_RetryCount++;
			isolatedFactory.Save();
		}
	}
}

