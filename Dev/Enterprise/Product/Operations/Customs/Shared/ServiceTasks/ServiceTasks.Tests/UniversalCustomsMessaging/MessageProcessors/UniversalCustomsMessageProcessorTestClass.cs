using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.Testing
{
	class UniversalCustomsMessageProcessorTestClass : IUniversalCustomsMessageProcessor
	{
		public Func<EDIMessage, LoggingInformation, ProcessingResult<LinkedBusinessObjectMetaData>> GetLinkedBusinessObjectMetaDataForTesting =
			(_, _) => LinkedBusinessObjectMetaData.Empty;
		public Func<EDIMessage, LoggingInformation, ZGuid, ProcessingResult<ZGuid>> GetBranchForTesting =
			(_, _, _) => GlbBranch.CurrentBranch.PK;
		public Func<EDIMessage, LoggingInformation, LinkedBusinessObjectMetaData, ProcessingResult<SerializationKeysResult>> GetSerializationKeysResultForTesting =
			(_, _, _) => new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string> { "Unknown" });

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			return GetLinkedBusinessObjectMetaDataForTesting(message, logger);
		}

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		{
			return GetBranchForTesting(message, logger, linkedBusinessObjectBranchPk);
		}

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger,
			LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			return GetSerializationKeysResultForTesting(message, logger, linkedBusinessObjectMetaData);
		}

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => ShouldMessageBeProcessedInASeparateFactoryForTesting(message);
		public Func<EDIMessage, bool> ShouldMessageBeProcessedInASeparateFactoryForTesting = _ => false;

		public Action<EDIMessage, LoggingInformation, IUniversalCustomsMessageProcessorHelper> ProcessMessageForTesting;
		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper) => ProcessMessageForTesting?.Invoke(message, logger, helper);
	}
}
