using CargoWise.Customs.PL.MessageContracts.DataProviders;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.Customs.Business.MessageProcessors.UCMP;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business;

sealed class FaultInterchangeUnpackingStrategy(DataProviderFactory dataProviderFactory)
	: XmlInterchangeUnpackingStrategy<ICommonFault>(dataProviderFactory)
{
	protected override EDIInterchangeUnpackerResult ProcessDataCore(
		EDIInterchange interchange,
		EDIInterchange outgoingInterchange,
		EnterpriseEDIMessage outgoingMessage,
		ICommonFault fault,
		ISimpleLogger logger)
	{
		var faultMessage = interchange.ContainedMessages.AddNew(outgoingMessage.GetType());
		faultMessage.EM_GB = outgoingMessage.EM_GB;
		faultMessage.EM_MessageType = outgoingMessage.EM_MessageType;
		faultMessage.EM_MessageSubType = EDIMessageSubType.Fault;
		faultMessage.EM_ApplicationCode = outgoingMessage.EM_ApplicationCode;
		faultMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Receive;
		faultMessage.EM_Status = EDIMessageStatusList.Codes.Queued;
		faultMessage.EM_IsActive = true;
		faultMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
		faultMessage.SetEM_MessageTextSource(new TextReaderSource(interchange.GetEI_BodyTextReader().CopyAndDispose()));
		faultMessage.EM_IsTestMessage = outgoingMessage.EM_IsTestMessage;
		interchange.Logs.AddNew(Events.InterchangeAcknowledged, $"Created fault message {faultMessage.EM_MessageNum} for transmit message {outgoingMessage.EM_MessageNum}.");

		return new EDIInterchangeUnpackerResult([faultMessage]);
	}
}
