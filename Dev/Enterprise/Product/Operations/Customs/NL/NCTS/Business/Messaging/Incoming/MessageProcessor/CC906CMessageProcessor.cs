using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Customs.NL.MessageContracts.MessageProviders;
using CargoWise.Customs.NL.MessageDefinitions.CC906C;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC906CMessageProcessor : NCTSResponseMessageProcessor<ICC906CDataProvider>
{
	public CC906CMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override bool SetNewCustomsStatus => false;

	protected override bool SetNewPhase => false;

	protected override ZString NewMessageStatus => LogicalStatusList.Codes.Error;

	protected override IMessageInterpreter<ICC906CDataProvider> Interpreter => new CC906CMessageInterpreter();

	protected override BusinessObject FindParentOfMessage(EDIMessage message) => FindParentOfMessageByLRNFallbackOnMRNFallbackOnCorrelationID(message, messageStatusArray: new string[2] { LogicalStatusList.Codes.Acknowledged, LogicalStatusList.Codes.Sent });

	protected override string GetLogMessageForFailingToLinkMessageToParentJob(EDIMessage message) => GetLogMessageForFailingToLinkMessageToParentJob_LRNFallbackOnMRNFallbackOnCorrelationID(message);

	protected override ICC906CDataProvider GetMessageDataProvider(EDIMessage message) => message.GetCachedInboundProvider<Cc906CType, CC906CDataProvider>();
}
