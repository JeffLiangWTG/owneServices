using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output;
using Enterprise.Customs.US.Messaging.Business.Processor;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business.MessageProcessor;
using BaseEDIMessage = Enterprise.Messaging.Business.EDIMessage;

[assembly: UniversalCustomsMessageProcessor(BaseEDIMessage.ApplicationCodes.USCustomsImport, typeof(Enterprise.Customs.US.Business.USIUniversalCustomsMessageProcessor))]
namespace Enterprise.Customs.US.Business
{
	public sealed class USIUniversalCustomsMessageProcessor : IUniversalCustomsMessageProcessor
	{
		public static string GetInBondJobNumberKey(ZString entryFilerCode, ZString inBondNumber, ZGuid companyPK) => $"{InBond}:{entryFilerCode}-{inBondNumber}|{companyPK}";
		public static string GetEntryJobNumberKey(ZString entryFilerCode, ZString entryNumber, ZGuid companyPK) => $"{Entry}:{entryFilerCode}-{entryNumber}|{companyPK}";
		public static string GetStatementJobNumberKey(ZString entryFilerCode, ZString statementNumber, ZGuid companyPK) => $"{Statement}:{entryFilerCode}-{statementNumber}|{companyPK}";

		// Reference files update should be processed in the order they were received
		const string ADCVDCase = "ADCVDCase";
		const string BrokerDownload = "BrokerDownload";
		const string Carrier = "Carrier";
		const string Country = "Country";
		const string Declaration = "Declaration";
		const string Entry = "Entry";
		const string ExchangeRate = "ExchangeRate";
		const string FIRMS = "FIRMS";
		const string ForeignPort = "ForeignPort";
		const string InBond = "InBond";
		const string Quota = "Quota";
		const string ReferenceFile = "ReferenceFile";
		const string RegionDistrictPort = "RegionDistrictPort";
		const string Statement = "Statement";
		const string Tariff = "Tariff";
		const string Visa = "Visa";

		public bool ShouldMessageBeProcessedInASeparateFactory(BaseEDIMessage message) => false;

		#region IUniversalCustomsMessageProcessor.GetLinkedBusinessObjectMetaData

		public ProcessingResult<LinkedBusinessObjectMetaData> GetLinkedBusinessObjectMetaData(BaseEDIMessage message, LoggingInformation logger)
		{
			var branchPK = message.EM_GB;
			var linkUniqueID = ZGuid.Empty;
			var linkTableName = ZString.Empty;
			var jobNumber = ZString.Empty;

			if (message is CBPEDIMessage cbpMessage)
			{
				var messageType = message.EM_MessageType.ToUpperInvariant();
				var messageBlockApplicationCode = cbpMessage.GetMessageBlockApplicationCode();
				if (UCMPMessageProcessorFactory.GetKeysForBlockingParallelProcessingProvider(messageType) is IKeysForBlockingParallelProcessingProvider provider)
				{
					return provider.GetLinkedBusinessObjectMetaData(cbpMessage, logger);
				}
				else
				{
					var originalMessage = cbpMessage.OriginalMessage;
					if (originalMessage != null)
					{
						branchPK = originalMessage.EM_GB;
						linkUniqueID = originalMessage.EM_LinkUniqueID;
						linkTableName = originalMessage.EM_LinkTable;
						var linkedObject = originalMessage.EM_LinkedObject;
						if (linkedObject is IMessageAttachee messageAttachee)
						{
							jobNumber = messageAttachee.TopLevelBusinessObject is IJobNumber topLevelBusinessObject ? topLevelBusinessObject.JobNumber : messageAttachee.TopLevelBizObjReferenceNumber;
						}
						else if (linkedObject is IJobNumber job)
						{
							jobNumber = job.JobNumber;
						}
					}
				}
				if (jobNumber.IsEmpty)
				{
					jobNumber = GetGroupName(cbpMessage, messageType);
				}

				return new LinkedBusinessObjectMetaData(linkTableName, linkUniqueID, branchPK, jobNumber);
			}
			else
			{
				var userLogStrings = string.Empty;
				if (logger?.UserLogStrings?.Count > 0)
				{
					userLogStrings += $"{string.Join(System.Environment.NewLine, logger.UserLogStrings.Cast<string>())}";
				}
				return ProcessingResult.New(LinkedBusinessObjectMetaData.Empty, UCMPMessageProcessorFactory.GetMessageProcessorCannotProcessMessage(message, "CBP", userLogStrings));
			}
		}

		string GetGroupName(CBPEDIMessage message, string messageType)
		{
			switch (messageType)
			{
				case ApplicationIdentifierCodeList.Codes.BrokerManifestDownload:
					return BrokerDownload;
				case ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse:
					return Declaration;
				case ApplicationIdentifierCodeList.Codes.CurrencyUpdate:
					return ExchangeRate;
				case ACEApplicationIdentifierCodeList.Codes.ExtractReferenceResponse:
					switch (message.MessageBlock.MessageBlocks.FirstOrDefault())
					{
						case ERFEF106:
						case Messaging.Business.MessageBuildingBlocks.Output.ERFF106:
							return Carrier;
						case Messaging.Business.MessageBuildingBlocks.Output.ERFF102:
							return Country;
						case ERFEError:
						case ERFF111:
						case ERFF211:
							return FIRMS;
						case Messaging.Business.MessageBuildingBlocks.Output.ERFF104:
							return ForeignPort;
						case Messaging.Business.MessageBuildingBlocks.Output.ERFF101:
							return RegionDistrictPort;
						case HTSV1:
							return Tariff;
					}
					return ReferenceFile;
				case ApplicationIdentifierCodeList.Codes.ExtractReferenceFilesResponse:
					switch (message.MessageBlock.MessageBlocks.FirstOrDefault())
					{
						case Messaging.Business.MessageBuildingBlocks.Output.ERFF108:
							return ExchangeRate;
						case Messaging.Business.MessageBuildingBlocks.Output.ERFF110:
						case HTSV1:
							return Tariff;
					}
					return ReferenceFile;
				case ApplicationIdentifierCodeList.Codes.HarmonizedSystemUpdate:
				case ApplicationIdentifierCodeList.Codes.QueryHarmonizedSystem:
				case ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleExtractReferenceQueryResponse:
				case ACEApplicationIdentifierCodeList.Codes.HarmonizedTariffScheduleQueryTransactionQueryResponse:
					return Tariff;
				case ACEApplicationIdentifierCodeList.Codes.StatementRequestRerouteResponse:
				case ACEApplicationIdentifierCodeList.Codes.StatementUpdateResponse:
				case ACEApplicationIdentifierCodeList.Codes.ACHDebitAuthorizationEntrySummaryPresentation:
				case ApplicationIdentifierCodeList.Codes.ABIStatementACHPaymentRerouteResponse:
				case ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentation:
				case ApplicationIdentifierCodeList.Codes.PeriodicDailyStatementACHDebitAuthorizationEntrySummaryPresentationResponse:
					return Statement;
				case ACEApplicationIdentifierCodeList.Codes.ADCVDCaseInformationQueryResponse:
					return ADCVDCase;
				case ApplicationIdentifierCodeList.Codes.QueryQuotaResponse:
					return Visa;
				case ACEApplicationIdentifierCodeList.Codes.QuotaQueryResponse:
					return Quota;
				case ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse:
				case ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse:
					return InBond;
				default:
					return string.Empty;
			}
		}

		#endregion

		public ProcessingResult<ZGuid> GetBranch(BaseEDIMessage message, LoggingInformation logger, ZGuid linkedBusinessObjectBranchPk) => linkedBusinessObjectBranchPk;

		public ProcessingResult<SerializationKeysResult> GetSerializationKeysResult(BaseEDIMessage message, LoggingInformation logger, LinkedBusinessObjectMetaData linkedBusinessObjectMetaData)
		{
			if (linkedBusinessObjectMetaData.JobNumber.IsEmpty)
			{
				return SerializationKeysResult.SerialProcessingInReceivedOrder;
			}

			var keys = new HashSet<string> { linkedBusinessObjectMetaData.JobNumber };

			if (message is CBPEDIMessage cbpMessage)
			{
				var messageType = message.EM_MessageType.ToUpperInvariant();
				var messageBlockApplicationCode = cbpMessage.GetMessageBlockApplicationCode();
				if (UCMPMessageProcessorFactory.GetKeysForBlockingParallelProcessingProvider(messageType) is IKeysForBlockingParallelProcessingProvider provider)
				{
					var keysFromProvider = provider.GetSerializationKeysResult(cbpMessage, logger, linkedBusinessObjectMetaData);
					if (keysFromProvider.ReturnValue.ResultType == SerializationKeysResult.SerializationKeysResultType.KeysProvided)
					{
						keys.UnionWith(keysFromProvider.ReturnValue.Keys);
					}
				}
				else
				{
					var originalMessage = cbpMessage.OriginalMessage;
					if (originalMessage != null && originalMessage.EM_LinkedObject is IMessageAttachee messageAttachee)
					{
						var entryFilerCode = ZString.Empty;
						var entryNumber = ZString.Empty;
						var jobReferenceNumber = ZString.Empty;
						if (messageAttachee is IMessageAttacheeInDeclaration messageAttacheeInDeclaration)
						{
							entryFilerCode = messageAttacheeInDeclaration.EntryFilerCode;
							entryNumber = messageAttacheeInDeclaration.EntryNumber;
							jobReferenceNumber = messageAttacheeInDeclaration.JobReferenceNumber;
						}

						if (!entryFilerCode.IsEmpty && !entryNumber.IsEmpty)
						{
							keys.Add(GetEntryJobNumberKey(entryFilerCode, entryNumber, (message.Factory.Load<GlbBranch>(message.EM_GB) ?? message.Branch ?? GlbBranch.CurrentBranch).GB_GC));
						}

						var jobNumber = linkedBusinessObjectMetaData.JobNumber;
						if (!jobNumber.IsEmpty && !jobReferenceNumber.IsEmpty && jobNumber != jobReferenceNumber)
						{
							keys.Add(jobReferenceNumber);
						}
					}
				}
			}
			return new SerializationKeysResult(SerializationKeysResult.SerializationKeysResultType.KeysProvided, keys);
		}

		#region IUniversalCustomsMessageProcessor.ProcessMessage

		public void ProcessMessage(BaseEDIMessage message, LoggingInformation logger, IUniversalCustomsMessageProcessorHelper helper)
		{
			GetMessageProcessorFactory(message.EM_MessageType.ToUpperInvariant(), logger).ProcessMessage(message);
		}

		MessageProcessorFactory GetMessageProcessorFactory(ZString messageType, LoggingInformation logger)
		{
			MessageProcessorFactory result;
			if (ReferenceFileMessageTypes.Contains(messageType))
			{
				result = new USRMessageProcessorFactory(logger);
			}
			else if (ImporterSecurityFilingMessageTypes.Contains(messageType))
			{
				result = ((MessageProcessorFactory)Activator.CreateInstance(ObjectFactory.GetType<IISFMessageProcessorFactory>(), new object[] { logger }));
			}
			else
			{
				result = new ABIMessageProcessorFactory(logger);
			}
			return result;
		}

		HashSet<ZString> ReferenceFileMessageTypes
		{
			get
			{
				if (referenceFileMessageTypes == null)
				{
					referenceFileMessageTypes = MessageProcessorFactory.GetReferenceFileMessageTypes().ToHashSet();
				}
				return referenceFileMessageTypes;
			}
		}
		HashSet<ZString> referenceFileMessageTypes;

		HashSet<ZString> ImporterSecurityFilingMessageTypes
		{
			get
			{
				if (importerSecurityFilingMessageTypes == null)
				{
					importerSecurityFilingMessageTypes = MessageProcessorFactory.GetImporterSecurityFilingMessageTypes().ToHashSet();
				}
				return importerSecurityFilingMessageTypes;
			}
		}
		HashSet<ZString> importerSecurityFilingMessageTypes;

		#endregion
	}
}
