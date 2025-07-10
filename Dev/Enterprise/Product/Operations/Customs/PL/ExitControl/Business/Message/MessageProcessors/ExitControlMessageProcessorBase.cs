using System;
using CargoWise.EntityFramework;
using Enterprise.BatchProcessor;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.PL.Business;
using Enterprise.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.PL.ExitControl.Business;

public abstract class ExitControlMessageProcessorBase<TDataProvider>(LoggingInformation logger) : BaseMessageProcessor<TDataProvider>(logger)
	where TDataProvider : class
{
	protected sealed override string ApplicationCodeCore => ApplicationCodeList.Codes.PLCustomsExitControl;

	protected override LocatorBase Locator => new LocatorPLX();

	protected override IRelatedJob GetRelatedJob(BaseEDIMessage message, TDataProvider messageDataProvider) => ((CusExitReport)message.EM_LinkedObject).Header;

	protected sealed override IMessageInterpreter<TDataProvider> CreateMessageInterpreter(BaseEDIMessage message)
		=> (IMessageInterpreter<TDataProvider>)Activator.CreateInstance(MessageInterpreterType, message.EM_LinkedObject);

	protected override IRegistryItem GetEmailGroupRegistryItem() => EUCustomsDataRegistry.Instance.SendExportAcknowledgements;

	protected override BusinessObject GetLinkedObject(BaseEDIMessage message, TDataProvider dataProvider)
		=> message.EM_LinkedObject;
}
