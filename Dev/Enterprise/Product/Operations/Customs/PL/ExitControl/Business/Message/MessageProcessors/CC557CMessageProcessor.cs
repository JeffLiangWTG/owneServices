using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.AES;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class CC557CMessageProcessor(LoggingInformation logger) : ExitControlMessageProcessorBase<ICC557C>(logger)
{
	protected override string MessageFriendlyNameCore => ExitControlMessageCodes.Descriptions.CC557;

	protected override bool IsFailureNotification => true;

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override string EmailSubject(BaseEDIMessage message, ICC557C messageDataProvider) =>
		FormattableString.Invariant($"{ExitControlConstants.InterpretationStrings.MessageTitles.CC557} - {messageDataProvider.MRN.IfNullOrEmpty(() => messageDataProvider.CorrelationIdentifier)}");

	protected override Type MessageInterpreterType => typeof(CC557CMessageInterpreter);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, ICC557C messageDataProvider)
	{
		if (GetTransmitMessage(message, messageDataProvider) is not { } transmitMessage)
		{
			return ProcessingResult.Fail;
		}

		transmitMessage.EM_Status = EDIMessageStatusList.Codes.Rejected;

		if (!IsInitiatingExitControlMessage(transmitMessage))
		{
			return ProcessingResult.Succeed;
		}

		var exitReport = (CusExitReport)message.EM_LinkedObject;
		exitReport.CER_Status = AESEntryStatusList.Codes.Rejected;
		GetLinkedEntryHeaders(exitReport).ForEach(entryHeader => entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.Rejected);
		return ProcessingResult.Succeed;
	}

	static IEnumerable<EU.Business.Declaration.CusEntryHeader> GetLinkedEntryHeaders(CusExitReport exitReport) =>
		exitReport.Header.Declaration is { } jobDeclaration
			? exitReport switch
			{
				{ Consignment: { CXC_MovementReference.IsEmpty: false } consignmentWithMrn } => jobDeclaration.CustomsEntryHeaders.Where(entryHeader => entryHeader.MovementReferenceNumber == consignmentWithMrn.CXC_MovementReference),
				{ Consignment: { CXC_LocalReference.IsEmpty: false } consignmentWithLrn } => jobDeclaration.CustomsEntryHeaders.Where(entryHeader => entryHeader.CH_BGMReference == consignmentWithLrn.CXC_LocalReference),
				_ => []
			}
			: [];

	static bool IsInitiatingExitControlMessage(BaseEDIMessage message) => (string)message.EM_MessageSubType is ExitControlMessageCodes.Descriptions.CC507
		or ExitControlMessageCodes.Descriptions.CC507B
		or ExitControlMessageCodes.Descriptions.CC570
		or ExitControlMessageCodes.Descriptions.CC615;
}
