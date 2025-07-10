using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public abstract class BaseNctsMessageProcessor<TDataProvider>(LoggingInformation logger) : BaseMessageProcessor<TDataProvider>(logger)
	where TDataProvider : class
{
	protected sealed override string ApplicationCodeCore => ApplicationCodeList.Codes.PLCustomsNCTS;

	protected override LocatorBase Locator => new LocatorNCTS();

	protected sealed override IMessageInterpreter<TDataProvider> CreateMessageInterpreter(BaseEDIMessage message)
		=> CreateMessageInterpreter(message.GetRelatedCommonMovementHeader());

	protected virtual IMessageInterpreter<TDataProvider> CreateMessageInterpreter(NctsCommonMovementHeader movementHeader)
		=> (IMessageInterpreter<TDataProvider>)Activator.CreateInstance(MessageInterpreterType, movementHeader);

	protected override IRelatedJob GetRelatedJob(BaseEDIMessage message, TDataProvider messageDataProvider) => message.GetRelatedNctsHeader();

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendNctsAcknowledgements;
}
