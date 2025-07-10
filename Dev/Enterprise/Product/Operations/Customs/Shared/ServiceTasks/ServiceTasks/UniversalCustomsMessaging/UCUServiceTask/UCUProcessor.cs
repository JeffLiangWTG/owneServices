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
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public class UCUProcessor(LoggingInformation logger, string applicationCode, IUniversalCustomsInterchangeUnpacker interchangeUnpacker, CancellationToken token) : IUniversalCustomsMessagingInterchangeProcessor
	{
		readonly LoggingInformation logger = logger;
		readonly string applicationCode = applicationCode;
		readonly CancellationToken token = token;
		readonly IUniversalCustomsInterchangeUnpacker interchangeUnpacker = interchangeUnpacker;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		public void Process()
		{
			var maxRetryCount = CustomsDataRegistry.Instance.UCUInterchangeUnpackingMaxRetryCount.Value;
			var numberPerBatch = CustomsDataRegistry.Instance.UCUInterchangesPerBatch.Value;
			int interchangesLengthPerBatch;
			do
			{
				token.ThrowIfCancellationRequested();
				var interchangesInQueue = GetQueuedInboundInterchanges(applicationCode, numberPerBatch);
				var interchangesToUnpack = GetValidInterchangesToUnpack(interchangesInQueue, maxRetryCount);
				interchangesLengthPerBatch = interchangesToUnpack.Count();
				if (interchangesLengthPerBatch > 0)
				{
					var interchangeGroups = interchangesToUnpack.GroupBy(i => i.EI_GB);
					foreach (var group in interchangeGroups)
					{
						token.ThrowIfCancellationRequested();
						using (DisposableEnvironment.ForBranch(group.Key.ToGuid()))
						{
							var groupArray = group.ToArray();
							logger.Log($"Start to process {groupArray.Length} {applicationCode} interchange(s) for {GlbCompany.CurrentCompany.GC_Code}/{GlbBranch.CurrentBranch.GB_Code}.");
							var factory = GetFactory("Interchanges and Messages Saving in batch");
							var ediInterchangeAndMessagesToSave = new Dictionary<EDIInterchange, ICollection<EDIMessage>>();
							var interchangesNeedToSave = ProcessInterchangesInBatch(factory, logger, groupArray, interchangeUnpacker, ediInterchangeAndMessagesToSave);

							if (interchangesNeedToSave > 0)
							{
								try
								{
									factory.Save();
									logger.Log($"{interchangesNeedToSave} {applicationCode} interchange(s) and unpacking message(s) saved in batch.");
								}
								catch (Exception ex) when (!ex.IsCriticalException())
								{
									logger.LogWarning($"Unable to save {interchangesNeedToSave} {applicationCode} interchanges and messages in batch. Each interchange and message(s) bundle will be saved separately with separate factory.");
									ProcessInterchangesSeparately(logger, ediInterchangeAndMessagesToSave);
								}
							}
						}
					}
				}
			} while (interchangesLengthPerBatch != 0);

			logger.Log($"No {applicationCode} interchange(s) to deal with.");
		}

		protected virtual BusinessObjectFactory GetFactory(string description) => new() { RefreshEnabled = false, NameForDebugging = description };

		EDIInterchange[] GetQueuedInboundInterchanges(ZString applicationCode, int numberPerBatch)
		{
			var factory = new BusinessObjectFactory();
			var query = new ZDBOnlyQuery(typeof(EDIInterchange));
			query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, applicationCode);
			query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchange.TransportType.xT);
			query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Queued);
			query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Receive);
			query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
			query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name;
			query.MaximumRows = numberPerBatch;
			return factory.Load<EDIInterchange>(query);
		}

		IEnumerable<EDIInterchange> GetValidInterchangesToUnpack(EDIInterchange[] interchanges, int maxRetryCount)
		{
			var result = new List<EDIInterchange>();
			foreach (var interchange in interchanges)
			{
				if (interchange.EI_BodyText.IsEmpty && interchange.EI_BodyData == null)
				{
					FailInterchange(interchange, logger, EDIInterchange.Status.Discarded, (NoResString)"The interchange has an empty message text, and cannot be unpacked to EdiMessage.");
					continue;
				}

				if (interchange.EI_RetryCount > maxRetryCount)
				{
					FailInterchange(interchange, logger, EDIInterchange.Status.Failed, $"Max retry attempts {maxRetryCount} Reached.");
					continue;
				}

				result.Add(interchange);
			}

			return result;
		}

		int ProcessInterchangesInBatch(BusinessObjectFactory factory, LoggingInformation logger, EDIInterchange[] interchanges, IUniversalCustomsInterchangeUnpacker interchangeUnpacker, Dictionary<EDIInterchange, ICollection<EDIMessage>> ediInterchangeAndMessagesToSave)
		{
			int interchangesNeedToSave = 0;
			var oppositeEdiInterchanges = LoadOppositeEdiInterchanges(factory, interchanges);
			var outgoingEDIMessagesDict = LoadOriginalOutgoingEDIMessages(factory, oppositeEdiInterchanges);

			foreach (var interchange in interchanges)
			{
				var interchangeToProcess = (EDIInterchange)factory.ImportFromAnotherFactory(interchange);

				try
				{
					var outgoingInterchange = GetOppositeOutgoingEDIInterchange(interchangeToProcess, oppositeEdiInterchanges);
					var outgoingEDIMessage = GetOriginalOutgoingEDIMessage(outgoingInterchange, outgoingEDIMessagesDict);
					var unpackResult = interchangeUnpacker.Unpack(interchangeToProcess, outgoingInterchange, outgoingEDIMessage, logger);

					if (unpackResult.IsSuccess)
					{
						HandleUnpackSuccess(ediInterchangeAndMessagesToSave, interchangeToProcess, unpackResult.EdiMessages, outgoingInterchange, outgoingEDIMessage);
					}
					else
					{
						HandleUnpackFailure(ediInterchangeAndMessagesToSave, interchangeToProcess, unpackResult.ErrorReason);
					}
				}
				catch (Exception ex)
				{
					HandleUnpackFailure(ediInterchangeAndMessagesToSave, interchangeToProcess, ex.Message);
				}

				interchangesNeedToSave++;
			}
			return interchangesNeedToSave;
		}

		void HandleUnpackSuccess(Dictionary<EDIInterchange, ICollection<EDIMessage>> ediInterchangeAndMessagesToSave, EDIInterchange interchangeToProcess, ICollection<EDIMessage> ediMessages, EDIInterchange outgoingInterchange, EDIMessage outgoingEDIMessage)
		{
			interchangeToProcess.EI_Status = EDIInterchange.Status.Received;

			var branchPk = outgoingEDIMessage?.EM_GB.IsValid == true
				? outgoingEDIMessage.EM_GB
				: outgoingInterchange?.EI_GB.IsValid == true
					? outgoingInterchange.EI_GB
					: interchangeToProcess.EI_GB.IsValid
						? interchangeToProcess.EI_GB
						: GlbBranch.CurrentBranch.PK;
			ediMessages.ForEach(x =>
			{
				if (!x.EM_GB.IsValid)
				{
					x.EM_GB = branchPk;
				}
			});

			if (outgoingEDIMessage != null)
			{
				foreach (var ediMessage in ediMessages)
				{
					ediMessage.EM_EM_RequestMessage = outgoingEDIMessage.PK;

					if (ediMessage.EM_LinkTable.IsEmpty && ediMessage.EM_LinkUniqueID.IsEmpty && !outgoingEDIMessage.EM_LinkTable.IsEmpty && !outgoingEDIMessage.EM_LinkUniqueID.IsEmpty)
					{
						ediMessage.EM_LinkTable = outgoingEDIMessage.EM_LinkTable;
						ediMessage.EM_LinkUniqueID = outgoingEDIMessage.EM_LinkUniqueID;
					}
				}
			}

			ediInterchangeAndMessagesToSave.Add(interchangeToProcess, ediMessages);
		}

		void HandleUnpackFailure(Dictionary<EDIInterchange, ICollection<EDIMessage>> ediInterchangeAndMessagesToSave, EDIInterchange interchangeToProcess, string errorReason)
		{
			ediInterchangeAndMessagesToSave.Add(interchangeToProcess, null);
			interchangeToProcess.EI_RetryCount++;
			logger.LogWarning($"Failed to unpack {interchangeToProcess.EI_ApplicationCode} Interchange {interchangeToProcess.EI_InterchangeNum}. Interchange Retry count {interchangeToProcess.EI_RetryCount}. ErrorReason: {errorReason}");
		}

		EDIInterchange[] LoadOppositeEdiInterchanges(BusinessObjectFactory factory, EDIInterchange[] interchanges)
		{
			if (interchanges.Length != 0)
			{
				var query = new ZQuery();
				query.AddToFilter(EDIInterchangeSchema.EI_ApplicationCode, interchanges.Select(h => h.EI_ApplicationCode));
				query.AddToFilter(EDIInterchangeSchema.EI_SessionGUID, interchanges.Select(h => h.EI_SessionGUID));
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(EDIInterchangeSchema.EI_TransportType, EDIInterchange.TransportType.xT);
				query.AddToFilter(EDIInterchangeSchema.EI_Status, EDIInterchange.Status.Sent);
				query.AddToFilter(EDIInterchangeSchema.EI_IsActive, true);
				query.OrderBy = EDIInterchangeSchema.EI_SystemCreateTimeUtc.Name + OrderByClause.Descending;
				return factory.Load<EDIInterchange>(query);
			}
			return default;
		}

		Dictionary<ZGuid, EDIMessage> LoadOriginalOutgoingEDIMessages(BusinessObjectFactory factory, EDIInterchange[] interchanges)
		{
			if (interchanges.Length != 0)
			{
				var query = new ZQuery();
				query.AddToFilter(EDIMessageSchema.EM_EI, interchanges.Select(h => h.PK));
				var ediMessages = factory.Load<EDIMessage>(query);
				return ediMessages.ToDictionary(x => x.EM_EI);
			}
			return default;
		}

		static EDIMessage GetOriginalOutgoingEDIMessage(EDIInterchange interchange, Dictionary<ZGuid, EDIMessage> outgoingEDIMessagesDict)
		{
			return interchange == null ? null : outgoingEDIMessagesDict.GetValueSafe(interchange.PK);
		}

		static EDIInterchange GetOppositeOutgoingEDIInterchange(EDIInterchange interchange, EDIInterchange[] oppositeEdiInterchanges)
		{
			return interchange == null ? null : oppositeEdiInterchanges.FirstOrDefault(x => x.EI_ApplicationCode == interchange.EI_ApplicationCode && x.EI_SessionGUID == interchange.EI_SessionGUID);
		}

		void ProcessInterchangesSeparately(LoggingInformation logger, Dictionary<EDIInterchange, ICollection<EDIMessage>> ediInterchangeAndMessagesToSave)
		{
			foreach (var ediInterchangeAndMessages in ediInterchangeAndMessagesToSave)
			{
				try
				{
					var newFactory = GetFactory((NoResString)"Interchange and Message(s) Saving Separately");
					var interchangeToSave = (EDIInterchange)newFactory.ImportFromAnotherFactory(ediInterchangeAndMessages.Key);
					foreach (var message in ediInterchangeAndMessages.Value)
					{
						var messagesToSave = (EDIMessage)newFactory.ImportFromAnotherFactory(message);
					}
					interchangeToSave.HasChanges = true;

					newFactory.Save();
					logger.Log($"The {interchangeToSave.EI_ApplicationCode} interchange {interchangeToSave.EI_InterchangeNum} and unpacking message(s) saved Separately.");
				}
				catch (Exception exSingle) when (!exSingle.IsCriticalException())
				{
					RetryInterchange(ediInterchangeAndMessages.Key, logger);
				}
			}
		}

		void FailInterchange(EDIInterchange failedInterchange, LoggingInformation logger, string status, ZString errorReason)
		{
			var isolatedFactory = GetFactory((NoResString)"Failed Interchange Unpacking Factory");
			var isolatedInterchange = isolatedFactory.Load<EDIInterchange>(failedInterchange.PK);
			isolatedInterchange.EI_Status = status;
			isolatedInterchange.Notes.AddNew(true, Res.GetString("51F9C33A-5DF5-4D25-802F-9702BFF06F78", "Unpack EDIInterchange Error"), errorReason);
			isolatedFactory.Save();
			logger.LogWarning($"Failed to unpack {failedInterchange.EI_ApplicationCode} Interchange {failedInterchange.EI_InterchangeNum}. {errorReason}");
		}

		void RetryInterchange(EDIInterchange retryInterchange, LoggingInformation logger)
		{
			var isolatedFactory = GetFactory((NoResString)"Retry Interchange Unpacking Factory");
			var isolatedInterchange = isolatedFactory.Load<EDIInterchange>(retryInterchange.PK);
			isolatedInterchange.EI_Status = EDIInterchange.Status.Queued;
			isolatedInterchange.EI_RetryCount++;
			isolatedFactory.Save();
			logger.LogWarning($"Unable to save {retryInterchange.EI_ApplicationCode} interchange {retryInterchange.EI_InterchangeNum} and message(s) Separately. Interchange Retry count {retryInterchange.EI_RetryCount}.");
		}
	}
}
