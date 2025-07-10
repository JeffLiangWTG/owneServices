using CargoWise.Customs.PL.MessageContracts.Interfaces;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

class FaultMessageProcessor(LoggingInformation logger) : PL.Business.FaultMessageProcessor(logger)
{
	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.PLCustomsNCTS;

	protected override LocatorBase Locator => new LocatorNCTS();

	protected override bool TryUpdateLinkedObject(BaseEDIMessage faultMessage, ICommonFault fault, EnterpriseEDIMessage transmitMessage)
	{
		var result = false;
		switch (faultMessage.EM_LinkedObject)
		{
			case NctsDepartureMovementHeader departureMovementHeader:
				departureMovementHeader.BM_MessageStatus = NctsMessageStatusList.Codes.Rejected;
				LogHelper.Log(Events.InterchangeAcknowledged, $"Updated corresponding MovementHeader status to [{NctsMessageStatusList.Codes.Rejected}]. {fault.FaultName}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}].", incomingMessage: faultMessage, transmitMessage: transmitMessage);
				result = true;
				break;

			case NctsHeader nctsHeader:
				nctsHeader.BH_MessageStatus = NctsMessageStatusList.Codes.Rejected;
				LogHelper.Log(Events.InterchangeAcknowledged, $"Updated corresponding NctsHeader status to [{NctsMessageStatusList.Codes.Rejected}]. {fault.FaultName}. Error code = [{fault.ErrorCode}], Description = [{fault.Description}].", incomingMessage: faultMessage, transmitMessage: transmitMessage);
				result = true;
				break;
		}

		return result;
	}

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsErrors;
}
