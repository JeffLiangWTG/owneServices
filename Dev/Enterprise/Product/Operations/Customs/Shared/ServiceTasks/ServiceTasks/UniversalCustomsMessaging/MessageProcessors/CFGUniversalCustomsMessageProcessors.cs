using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.Integration;

[assembly: UniversalCustomsMessageProcessor(ApplicationCodeList.Codes.XHCredentialConfig, typeof(Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging.CFGUniversalCustomsMessageProcessors))]

namespace Enterprise.Customs.ServiceTasks.UniversalCustomsMessaging
{
	public sealed class CFGUniversalCustomsMessageProcessors : IUniversalCustomsMessageProcessorWithAdditionalErrorReportDetails
	{
		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(EDIMessage message, LoggingInformation logger)
		{
			var processor = GetCFGUniversalCustomsMessageProcessor(message.EM_MessageType);
			var linkedBusinessObjectMetaData = processor?.GetLinkedBusinessObjectMetaData(message, logger);
			return linkedBusinessObjectMetaData ?? new LinkedBusinessObjectMetaData(message.EM_LinkTable, message.EM_LinkUniqueID, message.EM_GB, ZString.Empty);
		}

		public ProcessingResult<ZGuid> GetBranch(EDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk)
		{
			return linkedBusinessObjectBranchPk;
		}

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(EDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			if (linkedBusinessObjectMetaData.JobNumber.IsEmpty)
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, new HashSet<string>
			{
				linkedBusinessObjectMetaData.JobNumber
			});
		}

		public bool ShouldMessageBeProcessedInASeparateFactory(EDIMessage message) => false;

		public void ProcessMessage(EDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
		{
			var processor = GetCFGUniversalCustomsMessageProcessor(message.EM_MessageType);
			processor?.ProcessMessage(message, logger);
		}

		public static ICFGUniversalCustomsMessageProcessor GetCFGUniversalCustomsMessageProcessor(string messageType) =>
						cFGUniversalCustomsMessageProcessorCreatorsByMessageType.Value.TryGetValue(messageType, out var cFGUniversalCustomsMessageProcessorCreator)
						? cFGUniversalCustomsMessageProcessorCreator.Create()
						: null;

		static Lazy<IReadOnlyDictionary<string, IDataCreator<ICFGUniversalCustomsMessageProcessor>>> cFGUniversalCustomsMessageProcessorCreatorsByMessageType =>
			new(() =>
				CFGUniversalCustomsDataRegistration<ICFGUniversalCustomsMessageProcessor, UniversalCustomsMessageCFGProcessorAttribute>
					.AssertUniqueMessageTypes(ConfigurationProvider())
					.ToDictionary(x => x.MessageType, x => x.Creator),
				LazyThreadSafetyMode.ExecutionAndPublication);

		string IUniversalCustomsMessageProcessorWithAdditionalErrorReportDetails.GetAdditionalErrorReportDetails(EDIMessage message) => message == null ? string.Empty : $" ({message.EM_MessageType})";

		internal static Func<IEnumerable<CFGUniversalCustomsDataRegistration<ICFGUniversalCustomsMessageProcessor, UniversalCustomsMessageCFGProcessorAttribute>>> ConfigurationProvider { get; set; } =
			CFGUniversalCustomsDataRegistration<ICFGUniversalCustomsMessageProcessor, UniversalCustomsMessageCFGProcessorAttribute>.GetAssemblyRegistrations;
	}
}
