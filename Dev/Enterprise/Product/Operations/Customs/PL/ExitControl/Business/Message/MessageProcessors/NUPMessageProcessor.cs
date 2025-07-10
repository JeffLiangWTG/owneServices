using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.PL;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.PL.ExitControl.Business;

public class NUPMessageProcessor(LoggingInformation logger) : ExitControlMessageProcessorBase<IUpo>(logger)
{
	protected override string MessageFriendlyNameCore => $"{Messaging.Business.EDIInterchange.ApplicationCodes.PLCustomsExitControl}/{Constants.PUESC.SystemMessages.NUP}";

	protected override bool IsFailureNotification => true;

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportMessageErrors;

	protected override string EmailSubject(BaseEDIMessage message, IUpo messageDataProvider) =>
		$"{Constants.InterpretationStrings.MessageTitles.NUP}{UPOInterpreterBase<CusExitReport>.GetTransmitDocumentTypeForTitle(messageDataProvider)} {messageDataProvider.CorrelationIdentifier}.";

	protected override Type MessageInterpreterType => typeof(NUPMessageInterpreter<CusExitReport>);

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage message, IUpo messageDataProvider)
	{
		var transmitMessage = GetTransmitMessage(message, messageDataProvider);
		if (transmitMessage is null)
		{
			return ProcessingResult.Fail;
		}

		transmitMessage.EM_Status = EDIMessageStatusList.Codes.Error;

		if (!IsInitiatingExitControlMessage(transmitMessage))
		{
			return ProcessingResult.Succeed;
		}

		var exitReport = (CusExitReport)message.EM_LinkedObject;
		exitReport.CER_Status = AESEntryStatusList.Codes.ExitOfGoodsIsUnsatisfactory;
		GetLinkedEntryHeaders(exitReport).ForEach(entryHeader => entryHeader.CH_ExitedStatus = ExportExitStatus.Codes.ExitOfGoodsIsUnsatisfactory);
		return ProcessingResult.Succeed;
	}

	static IEnumerable<EU.Business.Declaration.CusEntryHeader> GetLinkedEntryHeaders(CusExitReport exitReport) =>
		exitReport.Header.Declaration is { } jobDeclaration
			? exitReport switch
			{
				{ Consignment: { CXC_MovementReference.IsEmpty : false } consignmentWithMrn } =>
					jobDeclaration.CustomsEntryHeaders.Where(entryHeader => entryHeader.MovementReferenceNumber == consignmentWithMrn.CXC_MovementReference),
				{ Consignment: { CXC_LocalReference.IsEmpty: false } consignmentWithLrn } =>
					jobDeclaration.CustomsEntryHeaders.Where(entryHeader => entryHeader.CH_BGMReference == consignmentWithLrn.CXC_LocalReference),
				_ => []
			}
			: [];

	static bool IsInitiatingExitControlMessage(BaseEDIMessage message) => (string)message.EM_MessageSubType
		is ExitControlMessageCodes.Descriptions.CC507
		or ExitControlMessageCodes.Descriptions.CC507B
		or ExitControlMessageCodes.Descriptions.CC570
		or ExitControlMessageCodes.Descriptions.CC615;
}
