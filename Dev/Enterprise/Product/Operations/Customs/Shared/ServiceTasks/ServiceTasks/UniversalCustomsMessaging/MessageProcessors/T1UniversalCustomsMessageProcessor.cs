using System.Collections.Generic;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public sealed class T1UniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
	{
		public T1UniversalCustomsMessageProcessor(ZInt uckDelayTimeInMilliseconds, ZInt ucqDelayTimeInMilliseconds, bool shouldMessageBeProcessedInASeparateFactory)
		{
			this.uckDelayTimeInMilliseconds = uckDelayTimeInMilliseconds;
			this.ucqDelayTimeInMilliseconds = ucqDelayTimeInMilliseconds;
			this.shouldMessageBeProcessedInASeparateFactory = shouldMessageBeProcessedInASeparateFactory;
		}
		readonly ZInt uckDelayTimeInMilliseconds;
		readonly ZInt ucqDelayTimeInMilliseconds;
		readonly bool shouldMessageBeProcessedInASeparateFactory;

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			if (uckDelayTimeInMilliseconds != 0)
			{
				Thread.Sleep(uckDelayTimeInMilliseconds);
			}

			if (message.EM_MessageSubType.EqualsIgnoringCase(EDIMessage.Status.Discarded)
				&& message.EM_ExternalReferenceNumber is ZString externalReferenceNumber
				&& !externalReferenceNumber.IsEmpty)
			{
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, (NoResString)externalReferenceNumber);
			}

			var branchPK = message.EM_GB;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;
			var jobNumber = ZString.Empty;

			var factory = message.Factory;
			var outgoingMessageQuery = new ZQuery(EDIMessageSchema.EM_MessageNum, message.EM_MessageNum);
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_ApplicationCode, message.EM_ApplicationCode);
			outgoingMessageQuery.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
			outgoingMessageQuery.OrderBy = EDIMessage.Schema.EM_SystemCreateTimeUtc;
			if (factory.LoadTop1<EDIMessage>(outgoingMessageQuery) is EDIMessage outgoingMessage)
			{
				branchPK = outgoingMessage.EM_GB;
				linkUniqueID = outgoingMessage.EM_LinkUniqueID;
				linkTableName = outgoingMessage.EM_LinkTable;
				if (linkUniqueID.IsValid && linkTableName.EqualsIgnoringCase(JobDeclarationSchema.Constants.TableName)
					&& factory.Load<BaseJobDeclaration>(linkUniqueID) is BaseJobDeclaration declaration)
				{
					jobNumber = declaration.JobNumber;
					branchPK = declaration.JE_GB;
				}
			}

			return new LinkedBusinessObjectMetaData(linkTableName, linkUniqueID, branchPK, jobNumber);
		}

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		{
			return linkedBusinessObjectBranchPk;
		}

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			var jobNumber = linkedBusinessObjectMetaData.JobNumber;

			if (jobNumber.IsEmpty)
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			var keys = new HashSet<string> { jobNumber };
			AddIfNotEmpty(keys, message.EM_ApplicationReference);
			AddIfNotEmpty(keys, message.EM_MessageOwner);
			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		bool IUniversalCustomsMessageProcessor.ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => shouldMessageBeProcessedInASeparateFactory;

		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
		{
			if (ucqDelayTimeInMilliseconds != 0)
			{
				Thread.Sleep(ucqDelayTimeInMilliseconds);
			}
			message.EM_Status = EDIMessage.Status.Received;
		}

		void AddIfNotEmpty(HashSet<string> additionalKeys, ZString key)
		{
			if (!key.IsEmpty)
			{
				additionalKeys.Add(key);
			}
		}
	}
}
