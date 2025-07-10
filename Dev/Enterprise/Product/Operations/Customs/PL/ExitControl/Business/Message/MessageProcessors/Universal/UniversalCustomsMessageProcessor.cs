using Enterprise.Messaging.Business.MessageProcessor;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

[assembly: UniversalCustomsMessageProcessor(ApplicationCode.PLCustomsExitControl, typeof(Enterprise.Customs.PL.ExitControl.Business.UniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.PL.ExitControl.Business;

sealed class UniversalCustomsMessageProcessor : Enterprise.Customs.PL.Business.UniversalCustomsMessageProcessorBase<MessageProcessorFactory>;
