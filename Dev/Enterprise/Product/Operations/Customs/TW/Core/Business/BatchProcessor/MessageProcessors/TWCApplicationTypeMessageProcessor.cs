using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TW.Business.Business;
using Enterprise.Customs.TW.Business.MessageProcessors;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.TW.Business.TWCMessageProcessorFactory;

namespace Enterprise.Customs.TW.Business.BatchProcessor
{
	public abstract class TWCApplicationTypeMessageProcessor(LoggingInformation logger) : ApplicationTypeMessageProcessor(logger), IMessageProcessor
	{
		protected override string MessageFriendlyNameCore => Res.GetString("FFC936BA-4FFF-4500-A414-EBC4461DDAB8", "TW Message");

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TaiwanCustoms;

		protected override bool RequiresPreProcessingCore => true;

		protected override ZQuery MessageFilterCore => AddNotInEDIMessageQueueStateQuery(base.MessageFilterCore);

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			if (message.EM_Status.EqualsIgnoringCase(EDIMessage.Status.Queued))
			{
				var twMessage = message as TWMessage;
				var errorInMessage = twMessage.ErrorText;
				if (errorInMessage.IsEmpty)
				{
					var linkedObjectResult = TryFindLinkedObject(twMessage);
					var discardReason = linkedObjectResult.DiscardReason;
					if (string.IsNullOrEmpty(discardReason))
					{
						if (linkedObjectResult.LinkedObject is BusinessObject linkedObject)
						{
							message.EM_LinkedObject = linkedObject;
						}

						message.EM_Status = TWMessage.Status.PreProcessedOK;
					}
					else
					{
						message.EM_Status = TWMessage.Status.Discarded;
						Logger.Log(discardReason);
					}
				}
				else
				{
					twMessage.EM_Status = EDIMessage.Status.Failed;
					Logger.LogError(GetFailedToParseTheMessage(twMessage, errorInMessage));
				}
			}
		}

		protected override void ProcessMessageCore(EDIMessage message)
		{
			var status = message.EM_Status;
			if (!status.EqualsIgnoringCase(EDIMessage.Status.Discarded) && !status.EqualsIgnoringCase(EDIMessage.Status.Failed))
			{
				var twMessage = message as TWMessage;
				if (ProcessMessageMain(twMessage) && this is not TWMessageProcessor)
				{
					AddLogsOnMessageSuccessfullyProcessed(twMessage);
				}
			}
		}

		protected abstract bool ProcessMessageMain(TWMessage message);

		protected virtual void AddLogsOnMessageSuccessfullyProcessed(TWMessage message)
		{
			if (message.EM_LinkedObject is IStmALogProvider logProvider)
			{
				logProvider.AddEventWithReference(Events.MessageReceived, message.EM_MessageType);
			}
		}

		public abstract (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason) TryFindLinkedObject(TWMessage message);

		ProcessingResult<ZGuid> IMessageProcessor.GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;

		ProcessingResult<LinkedBusinessObjectMetaData> IMessageProcessor.GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			if (message is TWMessage twMessage)
			{
				var linkedObjectResult = TryFindLinkedObject(twMessage);
				if (linkedObjectResult.LinkedObject is { } linkedObject)
				{
					return ProcessingResult.New(new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, linkedObjectResult.BranchPK, GetJobNumber(linkedObject)), linkedObjectResult.DiscardReason);
				}
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, linkedObjectResult.DiscardReason);
			}
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, new UnsupportedTypeMessageProcessor(logger).GetDiscardReason(message));
		}

		ZString GetJobNumber(BusinessObject linkedObject)
		{
			return linkedObject switch
			{
				CusEntryHeader entryheader => entryheader.DeclarationReference,
				CusInBondHeader inBondHeader => inBondHeader.BH_JobReference,
				ManifestBase.AsycudaManifestHeader manifestHeader => manifestHeader.AMA_JobReference,
				ManifestBase.AsycudaBill manifestBill => manifestBill.Header.AMA_JobReference,
				CusTWControllingMessageHeader controllingMessageHeader => controllingMessageHeader.TW1_FunctionalReferenceId,
				CusEntryNumber entryNumber => entryNumber.CE_EntryNum,
				_ => ZString.Empty,
			};
		}

		ProcessingResult<SerializationKeysResult> IMessageProcessor.GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			return linkedBusinessObjectMetaData.JobNumber.IsEmpty
			? SerializationKeysResult.SerialProcessingInReceivedOrder
			: new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { linkedBusinessObjectMetaData.JobNumber });
		}

		internal protected static MultilingualString GetUnableToFindTheLinkedJobMessage(EDIMessage message) =>
			ResString.GetMultilingualString("144ECDFD-D3BC-4E81-9A7B-84094EB35207", "Unable to find the linked job for {2} Message: #{0}/{1}", message.EM_MessageNum, message.Interchange?.EI_InterchangeNum, message.EM_MessageType);

		internal protected static string GetFailedToParseTheMessage(TWMessage message, ZString errorInMessage) =>
			Res.GetString("EEA05A6C-AC1B-4110-A1C5-2A4B4907B3FD", "Message {0}: Failed to parse the message as {1} message. Reason: {2}", message.EM_MessageNum, message.EM_MessageType, errorInMessage);

		protected void LogNotFindEntryHeaderLogWarning(TWMessage message)
		{
			Logger.LogWarning(Res.GetString("1CC36088-D2A9-4420-B5EF-7846889D0DAB", "Can not find the corresponding Entry Header for Message Number: {0}, Entry Number: {1}, Entry Type: {2}", message.EM_MessageNum, message.EntryNumber, message.EntryType));
		}

		static ZQuery AddNotInEDIMessageQueueStateQuery(ZQuery query) =>
			new ZDBOnlyQuery(typeof(EDIMessage))
				.AddToFilter(query)
				.AddFilterAndZSQLParameterCollection("EM_PK NOT IN (SELECT EQS_EM FROM dbo.EDIMessageQueueState WITH (INDEX(NR_RX__EQS_EM), FORCESEEK) WHERE EQS_ApplicationCode = EM_ApplicationCode)", new ZSqlParameterCollection());
	}
}
