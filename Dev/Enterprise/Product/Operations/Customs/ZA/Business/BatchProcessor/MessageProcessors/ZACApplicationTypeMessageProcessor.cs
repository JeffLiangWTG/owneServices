using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.ZA.Business.MessageProcessor;
using Enterprise.Freight.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ZA.Business.BatchProcessor.MessageProcessors
{
	public abstract class ZACApplicationTypeMessageProcessor : ApplicationTypeMessageProcessor, IMessageProcessor
	{
		protected ZACApplicationTypeMessageProcessor(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ApplicationCodeCore => ZAMessage.ApplicationCodes.SouthAfricanCustoms;
		protected override ZQuery MessageFilterCore => AddNotInEDIMessageQueueStateQuery(base.MessageFilterCore);

		protected override void PreProcessMessageCore(EDIMessage message)
		{
			if (message.EM_Status.EqualsIgnoringCase(EDIMessage.Status.Queued))
			{
				var linkedObjectResult = TryFindLinkedObject(message);
				var discardReason = linkedObjectResult.DiscardReason;
				if (string.IsNullOrEmpty(discardReason))
				{
					if (linkedObjectResult.LinkedObject is BusinessObject linkedObject)
					{
						message.EM_LinkedObject = linkedObject;
					}

					message.EM_Status = ZAMessage.Status.PreProcessedOK;
				}
				else
				{
					message.EM_Status = ZAMessage.Status.Discarded;
					Logger.Log(discardReason);
				}
			}
		}

		protected sealed override void ProcessMessageCore(EDIMessage message)
		{
			if (message.EM_Status.EqualsIgnoringCase(EDIMessage.Status.Discarded))
			{
				return;
			}
			ProcessMessageMain(message);
		}

		protected abstract void ProcessMessageMain(EDIMessage message);

		protected abstract (ZGuid BranchPK, BusinessObject LinkedObject, MultilingualString DiscardReason, MessageHelper Helper) TryFindLinkedObject(EDIMessage message);

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			var linkedObjectResult = TryFindLinkedObject(message);
			if (linkedObjectResult.LinkedObject is { } linkedObject)
			{
				return ProcessingResult.New(new LinkedBusinessObjectMetaData(linkedObject.TableName, linkedObject.PK, linkedObjectResult.BranchPK, ZString.Empty), linkedObjectResult.DiscardReason);
			}
			return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, linkedObjectResult.DiscardReason);
		}

		public virtual ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		{
			return linkedBusinessObjectBranchPk;
		}

		public virtual ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var linkedObject = message.EM_LinkedObject;
			var keys = new HashSet<string>();
			if (linkedObject is ManifestBase.AsycudaManifestHeader manifestHeader)
			{
				keys.Add(manifestHeader.AMA_JobReference);
			}
			else if (linkedObject is ManifestBase.AsycudaBill manifestBill)
			{
				keys.Add(manifestBill.Header.AMA_JobReference);
				var billNumber = manifestBill.ABL_BillNumber;
				if (!billNumber.IsEmpty)
				{
					keys.Add(billNumber);
				}
			}
			else if (linkedObject is CusEntryHeader entryheader)
			{
				keys.Add(entryheader.DeclarationReference);
				var lrn = entryheader.CH_BGMReference;
				if (!lrn.IsEmpty)
				{
					keys.Add(lrn);
				}
			}
			else if (linkedObject is JobVoyage voyage)
			{
				keys.Add(voyage.PK.ToStringKey());
			}

			if (!keys.Any() || keys.All(string.IsNullOrEmpty))
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		internal protected static MultilingualString GetUnableToFindTheLinkedJobMessage(string messageType, EDIMessage message) =>
			ResString.GetMultilingualString("2C1DDA58-1B24-43F9-8E05-EE41644C51B2", "Unable to find the linked job for {2} Message: #{0}/{1}", message.EM_MessageNum, message.Interchange?.EI_InterchangeNum, messageType);

		internal static MultilingualString GetMessageProcessorCannotProcessMessage(string messageType, EDIMessage message) => ResString.GetMultilingualString("01596EE6-2096-454B-BDA5-313BA66EAE9E", "{0} Message Processor cannot process message type {1}", messageType, message.GetType().Name);

		static ZQuery AddNotInEDIMessageQueueStateQuery(ZQuery query) =>
			new ZDBOnlyQuery(typeof(EDIMessage))
				.AddToFilter(query)
				.AddFilterAndZSQLParameterCollection("EM_PK NOT IN (SELECT EQS_EM FROM dbo.EDIMessageQueueState WITH (INDEX(NR_RX__EQS_EM), FORCESEEK) WHERE EQS_ApplicationCode = EM_ApplicationCode)", new ZSqlParameterCollection());
	}
}
