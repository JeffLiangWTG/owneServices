using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using Enterprise.Messaging.InterchangeProviders;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.NL.Business;

public class NLOutgoingMessageProcessor : OutgoingMessageProcessor
{
	public NLOutgoingMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override InterchangeProviderBase CreateNewInterchangeProvider(NonDependentEDIMessageCollection readyMessages)
	{
		return new NLInterchangeProvider(readyMessages);
	}

	protected override ZQuery MessageFilter => messageFilter ?? (messageFilter = GetMessageFilterQuery());
	ZQuery messageFilter;

	static ZQuery GetMessageFilterQuery()
	{
		var result = new ZQuery();
		result.AddToFilter(EDIMessageSchema.EM_ApplicationCode, NLEDIMessage.ApplicationCodes.NLCustoms);
		result.AddToFilter(EDIMessageSchema.EM_MessageType, GetOutgoingMessageTypes());
		return result;
	}

	public static string[] GetOutgoingMessageTypes() => new[]
	{
		NLEDIMessageTypes.Codes.DMS,
		NLEDIMessageTypes.Codes.NCT
	};
}
