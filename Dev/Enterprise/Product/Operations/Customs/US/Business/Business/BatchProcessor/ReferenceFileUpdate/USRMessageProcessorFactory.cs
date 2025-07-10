using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;

namespace Enterprise.Customs.US.Business
{
	class USRMessageProcessorFactory : MessageProcessorFactory
	{
		public USRMessageProcessorFactory(LoggingInformation logger)
			: base(logger, EDIMessage.ApplicationCodes.USCustomsImport, "US Customs Reference File Update Message Processor")
		{
		}

		protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => GetReferenceFileMessageTypes();

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		protected override void SetHeldUntilDate(Enterprise.Messaging.Business.EDIMessage message, IEnumerable<Enterprise.Messaging.Business.EDIMessage> unprocessedMessages)
		{
			switch (message.EM_MessageType)
			{
				case ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse:
					SetHeldAndSave(message, unprocessedMessages, new ZString[] { ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse });
					break;
				case ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse:
				case ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse:
					SetHeldAndSave(message, unprocessedMessages, new ZString[]
					{
						ApplicationIdentifierCodeList.Codes.AntidumpingCountervailingDutyQueryResponse,
						ApplicationIdentifierCodeList.Codes.ExtractADDCVDCaseFileResponse
					});
					break;
				case ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate:
				case ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem:
				case ApplicationIdentifierCodeList.Codes.QueryQuotaResponse:
				case ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse:
				case ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse:
				case ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse:
					SetHeldAndSave(message, unprocessedMessages, new ZString[]
					{
						ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate,
						ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem,
						ApplicationIdentifierCodeList.Codes.QueryQuotaResponse,
						ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse,
						ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse,
						ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse
					});
					break;
				default:
					base.SetHeldUntilDate(message, unprocessedMessages);
					break;
			}
		}

		void SetHeldAndSave(Enterprise.Messaging.Business.EDIMessage message, IEnumerable<Enterprise.Messaging.Business.EDIMessage> unprocessedMessages, ZString[] messageTypes)
		{
			message.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(2);
			var factories = new List<BusinessObjectFactory>();
			foreach (var unprocessedMessage in unprocessedMessages.Where(x => messageTypes.Contains(x.EM_MessageType)))
			{
				unprocessedMessage.EM_HeldUntilDate = ZDateTime.UtcNow.AddMinutes(2);
				if (!factories.Contains(unprocessedMessage.Factory))
				{
					factories.Add(unprocessedMessage.Factory);
				}
			}
			factories.ForEach(x => x.Save());
		}

		protected override bool IsDataLockSupported
		{
			get { return true; }
		}
	}
}
