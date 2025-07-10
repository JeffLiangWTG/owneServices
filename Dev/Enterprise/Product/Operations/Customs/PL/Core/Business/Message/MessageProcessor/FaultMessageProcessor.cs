using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;
using EntryHeaderStatus = Enterprise.Customs.Common.Shared.MessageStatusList.Codes;

namespace Enterprise.Customs.PL.Business;

public class FaultMessageProcessor(LoggingInformation logger) : BaseMessageProcessor<ICommonFault>(logger)
{
	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.PLCustoms;

	protected override LocatorBase Locator => new LocatorPLC();

	protected override bool IsFailureNotification => true;

	protected override string MessageFriendlyNameCore => Constants.EDIMessageSubType.Fault;

	protected override BusinessObject GetLinkedObject(BaseEDIMessage message, ICommonFault messageDataProvider) => message.EM_LinkedObject;

	protected override ProcessingResult ProcessMessageCore(BaseEDIMessage faultMessage, ICommonFault fault)
	{
		var transmitMessage = faultMessage.Interchange != null
			? faultMessage.Factory.FindTransmittedMessageBySessionGuid(Constants.AllSupportedApplications, faultMessage.Interchange.EI_SessionGUID)
			: null;
		if (transmitMessage != null)
		{
			transmitMessage.EM_Status = EDIMessageStatusList.Codes.Failed;
			LogHelper.Log(Events.MessageRejected, $"Updated corresponding transmit message status to [{EDIMessageStatusList.Codes.Failed}].", incomingMessage: faultMessage, transmitMessage: transmitMessage);
			faultMessage.EM_LinkedObject = transmitMessage.EM_LinkedObject;
		}

		switch (faultMessage.EM_LinkedObject)
		{
			case null:
				LogHelper.Log(Events.ErrorReport, $"Linked object not found! {fault.FaultName}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}].", serviceLog: Logger, incomingMessage: faultMessage, transmitMessage: transmitMessage);
				return ProcessingResult.Fail;

			case CusPollingTransaction cusPollingTransaction:
				cusPollingTransaction.ProcessInboundFaultMessage(fault, Logger, faultMessage, transmitMessage);
				break;

			default:
				if (!TryUpdateLinkedObject(faultMessage, fault, transmitMessage))
				{
					LogHelper.Log(Events.ErrorReport, $"Unsupported LinkedObject type: {faultMessage.EM_LinkedObject.GetType()}! {fault.FaultName}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}].", serviceLog: Logger, incomingMessage: faultMessage, transmitMessage: transmitMessage);
					return ProcessingResult.Fail;
				}
				break;
		}

		return ProcessingResult.Succeed;
	}

	protected virtual bool TryUpdateLinkedObject(BaseEDIMessage faultMessage, ICommonFault fault, EnterpriseEDIMessage transmitMessage)
	{
		if (faultMessage.EM_LinkedObject is not CusEntryHeader cusEntryHeader)
		{
			return false;
		}

		cusEntryHeader.CH_Status = EntryHeaderStatus.ErrorReplace;
		LogHelper.Log(Events.InterchangeAcknowledged, $"Updated corresponding CusEntryHeader status to [{EntryHeaderStatus.ErrorReplace}]. {fault.FaultName}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}].", incomingMessage: faultMessage, transmitMessage: transmitMessage);
		return true;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportMessageErrors;
}
