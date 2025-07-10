using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Customs.NL.Business.Common;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;

namespace Enterprise.Customs.NL.Business;

public class MessageGenerator : IMessageGenerator<CusEntryHeader>
{
	public MessageGenerator(IEnumerable<JobDeclarationMessageSendingObject> objectsToSend)
	{
		ObjectsToSend = objectsToSend ?? Enumerable.Empty<JobDeclarationMessageSendingObject>();
	}

	IEnumerable<JobDeclarationMessageSendingObject> ObjectsToSend { get; }

	IBuilderResult IMessageGenerator<CusEntryHeader>.Generate(CusEntryHeader entryHeader)
	{
		var errorCollector = new ErrorCollector();

		var result = new BuilderResult(entryHeader, errorCollector.GetErrors(ErrorCollector.ErrorType.CRITICAL), AfterFullSuccess);
		var sendingObject = GetMessageSendingObjectForEntry(entryHeader);
		if (sendingObject != null)
		{
			result.Message = CreateEDIMessage(entryHeader, sendingObject);
		}
		return result;
	}

	ZString IMessageGenerator<CusEntryHeader>.MakePrettyForInterpretation(EDIMessage message)
	{
		var result = ZString.Empty;
		if (message is NLEDIMessage nlMessage)
		{
			result = nlMessage.GetOutgoingMessageInterpretation();
			if (result.IsEmpty)
			{
				result = Res.GetString("B882F390-5C8A-4F30-84BA-C53092F6ED16", "The generated message has no content.");
			}
		}
		return result;
	}

	void IMessageGenerator<CusEntryHeader>.PutReferenceNumberIntoMessageFromPlaceholder(EDIMessage message, ZString messageText, CusEntryHeader entryHeader)
	{
		message.EM_MessageText = messageText.Replace(NLEDIMessage.MessageNumberPlaceHolderHtml, message.EM_MessageNum);
		message.EM_MessageText = message.EM_MessageText.Replace(NLEDIMessage.WCOTypePlaceHolderHtml, GetWcoType(GetMessageSendingObjectForEntry(entryHeader)));
	}

	void AfterFullSuccess(IBuilderResult builderResult)
	{
		var entryHeader = (CusEntryHeader)builderResult.Owner;

		var sendingObject = GetMessageSendingObjectForEntry(entryHeader);
		if (sendingObject != null)
		{
			var wocType = GetWcoType(sendingObject);
			switch (wocType)
			{
				case NLConstants.WCoTypeCodes.ExportPresentation:
					entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._511;
					entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
					entryHeader.CH_EntryStatus = NLConstants.EntryStatusNew.PreLodged;
					break;
				case NLConstants.WCoTypeCodes.ExportAmendment:
					entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._513;
					entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
					break;
				case NLConstants.WCoTypeCodes.ExportInvalidationRequest:
					entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._514;
					entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
					break;
				case NLConstants.WCoTypeCodes.ExportDeclaration:
					entryHeader.CH_PhaseStatus = entryHeader.CH_PhaseStatus == CustomsEntryPhaseStatusList.Codes.SUP ? entryHeader.CH_PhaseStatus : CustomsEntryPhaseStatusList.Codes._515;
					entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
					break;
				case NLConstants.WCoTypeCodes.InformationOnNonExitedExport:
					entryHeader.CH_PhaseStatus = CustomsEntryPhaseStatusList.Codes._583;
					entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
					break;
				case NLConstants.WCoTypeCodes.ControlFindingsInformationExport:
					entryHeader.CH_Status = NLConstants.StatusNew.SentToCustoms;
					break;
			}
		}
	}

	JobDeclarationMessageSendingObject GetMessageSendingObjectForEntry(CusEntryHeader entry) => ObjectsToSend.FirstOrDefault(x => x.Header.PK.Equals(entry.PK));

	NLEDIMessage CreateEDIMessage(CusEntryHeader entryHeader, JobDeclarationMessageSendingObject sendingObject)
	{
		var nlEdiMessageType = GetEdiMessageType(sendingObject);
		var message = (NLEDIMessage)entryHeader.Messages.AddNew(nlEdiMessageType);
		message.EM_MessageType = NLEDIMessageTypes.Codes.DMS;
		message.EM_MessageSubType = sendingObject.MessageType;
		message.EM_IsTestMessage = NLCustomsRegistry.Instance.IsNLTestingSystem.Value;
		message.EM_Status = NLEDIMessage.Status.Queued;
		message.EM_ReceiveTransmit = NLEDIMessage.Direction.Transmit;
		message.EM_LinkedObject = sendingObject.Header;
		message.EM_ApplicationReference = sendingObject.Header.EntryNumber;
		message.EM_MessageText = GetMessageText(sendingObject);
		if (message.EM_MessageSubType == CustomsEntryPhaseStatusList.Codes.FBK)
		{
			message.EM_HeldUntilDate = ZDateTime.MaxSmallDateTime;
			var nlEntryHeader = entryHeader as Declaration.CusEntryHeader;
			nlEntryHeader.FallbackEntryNumberIssueDate = ZDateTime.Now;
		}

		if (sendingObject.IsMessageTypeCAN && !sendingObject.ReasonForInvalidation.IsEmpty)
		{
			message.CustomsMessageRemarks = $"{NLConstants.StatementTypes.Customs}|{sendingObject.ReasonForInvalidation}";
		}

		return message;
	}

	Type GetEdiMessageType(JobDeclarationMessageSendingObject sendingObject)
	{
		switch (sendingObject.MessageType)
		{
			case ImportSendMessageTypes.Codes.CAN:
				return typeof(CancelDeclarationEDIMessage);
			default:
				return typeof(NLEDIMessage);
		}
	}

	ZString GetMessageText(JobDeclarationMessageSendingObject sendingObject)
	{
		var result = DeclarationMessageBuilderLoader.Instance.GetMessageBuilder(sendingObject)
						 ?.GenerateXmlMessage()
						 .GetSerializedString() ??
					 string.Empty;
		return result.IsEmpty() ? result : XmlMessageHelper.RemoveEmptyXmlElements(result);
	}

	protected ZString GetWcoType(JobDeclarationMessageSendingObject sendingObject)
	{
		if (sendingObject.Declaration.IsImport)
		{
			switch (sendingObject.MessageType)
			{
				case ImportSendMessageTypes.Codes.CAN:
					return NLConstants.WCoTypeCodes.ImportInvalidationRequest;
				case ImportSendMessageTypes.Codes.PRE:
					return NLConstants.WCoTypeCodes.ImportPresentation;
				case ImportSendMessageTypes.Codes.DEC:
					return NLConstants.WCoTypeCodes.ImportDeclaration;
				default:
					return ZString.Empty;
			}
		}
		else
		{
			switch (sendingObject.MessageType)
			{
				case ExportSendMessageTypes.Codes.AMD:
					return NLConstants.WCoTypeCodes.ExportAmendment;
				case ExportSendMessageTypes.Codes.DEC:
				case ExportSendMessageTypes.Codes.SUP:
					return NLConstants.WCoTypeCodes.ExportDeclaration;
				case ExportSendMessageTypes.Codes.CAN:
					return NLConstants.WCoTypeCodes.ExportInvalidationRequest;
				case ExportSendMessageTypes.Codes.PRE:
					return NLConstants.WCoTypeCodes.ExportPresentation;
				case ExportSendMessageTypes.Codes.EXT:
					return NLConstants.WCoTypeCodes.InformationOnNonExitedExport;
				case ExportSendMessageTypes.Codes.CRE:
					return NLConstants.WCoTypeCodes.ControlFindingsInformationExport;
				default:
					return ZString.Empty;
			}
		}
	}
}
