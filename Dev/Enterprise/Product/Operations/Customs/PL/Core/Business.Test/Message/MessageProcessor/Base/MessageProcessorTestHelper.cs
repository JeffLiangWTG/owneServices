using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

public static class MessageProcessorTestHelper
{
	public static CusEntryHeader CreateEntryHeaderWithEntryNumber(this BusinessObjectFactory factory, string entryNumberMrn)
	{
		var entryHeader = factory.NewWithValidTestData<CusEntryHeader>();
		var entryNumber = factory.CreateEntryNumber(entryNumberMrn);
		entryNumber.CE_ParentID = entryHeader.PK;

		return entryHeader;
	}

	public static CusEntryNumber CreateEntryNumber(this BusinessObjectFactory factory, string entryNumberMrn)
	{
		var entryNumber = factory.New<CusEntryNumber>();
		entryNumber.CE_EntryNum = entryNumberMrn;
		entryNumber.CE_ParentTable = CusEntryHeader.Schema.TableName;
		entryNumber.CE_Category = CusEntryNumber.Categories.CustomsPermitClearanceNumber;
		entryNumber.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Poland;
		entryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

		return entryNumber;
	}

	public record TransmittedIncomingMessages(
		BusinessObject LinkedObject,
		EDIInterchange TransmittedInterchange,
		BaseEDIMessage TransmittedMessage,
		EDIInterchange IncomingInterchange,
		BaseEDIMessage IncomingMessage);

	public static TransmittedIncomingMessages CreateTransmittedIncomingMessages<TEDIMessage>(
		this BusinessObjectFactory factory,
		ZString applicationCode,
		BusinessObject linkedObject,
		ZString messageType,
		ZString transmittedMessageSubType,
		ZString incomingMessageSubType)
		where TEDIMessage : BaseEDIMessage
	{
		var (transmitInterchange, transmitMessage) = factory.CreateInterchangeAndMessageForTest<EDIInterchange, TEDIMessage>();

		transmitMessage.EM_ApplicationCode = applicationCode;
		transmitMessage.EM_MessageType = messageType;
		transmitMessage.EM_MessageSubType = transmittedMessageSubType;
		transmitMessage.EM_MessageText = "TEST";
		transmitMessage.EM_IsActive = true;
		transmitMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitMessage.EM_Status = EDIMessage.Status.Sent;
		transmitMessage.EM_LinkedObject = linkedObject;
		transmitMessage.EM_IsTestMessage = true;

		transmitInterchange.EI_ApplicationCode = applicationCode;
		transmitInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		transmitInterchange.EI_Status = EDIInterchange.Status.Sent;

		var (incomingInterchange, incomingMessage) = transmitMessage.Factory.CreateInterchangeAndMessageForTest<EDIInterchange, TEDIMessage>(
			sessionGuid: transmitMessage.Interchange.EI_SessionGUID);
		incomingMessage.EM_MessageType = transmitMessage.EM_MessageType;
		incomingMessage.EM_MessageSubType = EDIMessageSubType.Fault;
		incomingMessage.EM_ApplicationCode = transmitMessage.EM_ApplicationCode;
		incomingMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		incomingMessage.EM_Status = EDIMessageStatusList.Codes.PreProcessedOK;
		incomingMessage.EM_IsActive = true;
		incomingMessage.EM_LinkedObject = transmitMessage.EM_LinkedObject;
		incomingMessage.EM_MessageText = "TEST";
		incomingMessage.EM_IsTestMessage = transmitMessage.EM_IsTestMessage;
		incomingMessage.EM_MessageNum = "EM_MessageNum";
		incomingMessage.EM_IsTestMessage = true;

		incomingInterchange.EI_ApplicationCode = incomingMessage.EM_ApplicationCode;
		incomingInterchange.EI_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		incomingInterchange.EI_Status = EDIInterchange.Status.Received;
		incomingInterchange.EI_SessionGUID = transmitInterchange.EI_SessionGUID;

		return new TransmittedIncomingMessages(
			LinkedObject: linkedObject,
			TransmittedInterchange: transmitInterchange,
			TransmittedMessage: transmitMessage,
			IncomingInterchange: incomingInterchange,
			IncomingMessage: incomingMessage);
	}
}
