using System;
using System.Linq;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Integration;
using static Enterprise.Customs.PL.Business.Constants;
using static Enterprise.Customs.PL.Business.Constants.InterpretationStrings;
using ApplicationCodes = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;
using MessageStatusList = Enterprise.Customs.Common.EU.MessageStatusList;

namespace Enterprise.Customs.PL.Business;

public class NUPMessageProcessor(LoggingInformation logger) : ImpExpMessageProcessorBase<IUpo>(logger)
{
	readonly string[] messageCodesToReject = [
		AESMessageCodes.Descriptions.CC515,
		AESMessageCodes.Descriptions.CC570,
		AESMessageCodes.Descriptions.PW515,
	];

	protected override string EmailSubject(BaseEDIMessage message, IUpo dataProvider)
		=> $"{MessageTitles.NUP}{UPOInterpreterBase<CusEntryHeader>.GetTransmitDocumentTypeForTitle(dataProvider)} {dataProvider.CorrelationIdentifier}.";

	protected override Type MessageInterpreterType => typeof(NUPMessageInterpreter<CusEntryHeader>);

	protected override string MessageFriendlyNameCore => $"{ApplicationCodes.PLCustoms}/{PUESC.SystemMessages.NUP}";

	protected override bool IsFailureNotification => true;

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IUpo messageDataProvider)
	{
		var transmitMessage = message.Factory.FindTransmittedMessageBySessionGuid(AllSupportedApplications, message.Interchange.EI_SessionGUID);
		if (transmitMessage == null)
		{
			return ProcessingResult.Fail;
		}

		transmitMessage.EM_Status = EDIMessage.Status.Error;

		if (messageCodesToReject.Contains(transmitMessage.EM_MessageSubType.ToString())
			&& transmitMessage.EM_LinkedObject is CusEntryHeader cusEntryHeader)
		{
			cusEntryHeader.CH_EntryStatus = MessageStatusList.Codes.SentAndRejected;
		}

		return ProcessingResult.Succeed;
	}
}
