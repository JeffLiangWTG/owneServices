using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC917C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC917CMessageProcessor : NCTSResponseMessageProcessor<ICC917CDataProvider>
{
	public CC917CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewCustomsStatus => false;

	protected override bool SetNewPhase => false;

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Error;

	protected override IMessageInterpreter<ICC917CDataProvider> Interpreter => new CC917CMessageInterpreter();

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => base.FindParentOfMessageByLRNFallbackOnMRNFallbackOnCorrelationID(message);

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRNFallbackOnCorrelationID(message);

	protected override ICC917CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc917CType, CC917CDataProvider>();
}
