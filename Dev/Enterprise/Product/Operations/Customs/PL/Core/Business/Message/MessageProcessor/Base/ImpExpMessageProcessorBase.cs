using System;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.Business;

public abstract class ImpExpMessageProcessorBase<TDataProvider>(LoggingInformation logger) : BaseMessageProcessor<TDataProvider>(logger)
	where TDataProvider : class
{
	protected override string ApplicationCodeCore => ApplicationCodeList.Codes.PLCustoms;

	protected override LocatorBase Locator => new LocatorPLC();

	protected sealed override IMessageInterpreter<TDataProvider> CreateMessageInterpreter(BaseEDIMessage message)
		=> (IMessageInterpreter<TDataProvider>)Activator.CreateInstance(MessageInterpreterType, message.EM_LinkedObject);

	protected override IRelatedJob GetRelatedJob(BaseEDIMessage message, TDataProvider messageDataProvider) => message.EM_LinkedObject as IRelatedJob;

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportAcknowledgements;
}
