using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.PL.Business;

public class PLOutboundMessageProcessorLegacy(LoggingInformation logger) : OutgoingMessageProcessor(logger)
{
	protected override ZQuery MessageFilter => messageFilter ??= GetMessageFilterQuery();

	ZQuery messageFilter;

	static ZQuery GetMessageFilterQuery() => new ZQuery()
		.AddToFilter(EDIMessageSchema.EM_ApplicationCode, new[]
		{
			EDIInterchange.ApplicationCodes.PLCustoms,
			EDIInterchange.ApplicationCodes.PLCustomsNCTS,
			EDIInterchange.ApplicationCodes.PLCustomsExitControl,
			EDIInterchange.ApplicationCodes.PLCustomsPUESCEmailSystem
		});

	protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages) => new PLInterchangeProviderLegacy(readyMessages, Logger);
}
