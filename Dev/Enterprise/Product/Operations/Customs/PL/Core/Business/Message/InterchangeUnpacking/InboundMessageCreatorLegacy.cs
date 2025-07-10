using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business;

class InboundMessageCreatorLegacy(LoggingInformation logger) : PLInterchangeUnpacker, IInboundMessageCreator
{
	public LoggingInformation Logger { get; } = Argument.NotNull(logger, nameof(logger));

	public void CreateMessagesForInterchange(EDIInterchange interchange)
	{
		if (interchange.Factory.FindTransmittedMessageBySessionGuid(Constants.AllSupportedApplications, interchange.EI_SessionGUID) is not { } transmitMessage)
		{
			LogError((NoResString)"Unable to find transmitted message!");
			return;
		}

		var unpackResult = Unpack(interchange, outgoingInterchange: transmitMessage.Interchange, transmitMessage, Logger);
		if (!unpackResult.IsSuccess)
		{
			LogError(unpackResult.ErrorReason);
		}
		return;

		void LogError(ZString errorMessage)
		{
			interchange.EI_Status = EDIInterchange.Status.Error;
			interchange.Logs.AddNew(Events.ErrorReport, errorMessage);
			logger.Log(LogType.Error, $"{errorMessage} Interchange ID: {interchange.PK}");
		}
	}
}
