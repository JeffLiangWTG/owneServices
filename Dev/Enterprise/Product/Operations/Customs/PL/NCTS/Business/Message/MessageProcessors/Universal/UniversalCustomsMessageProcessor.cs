using Enterprise.Messaging.Business.MessageProcessor;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

[assembly: UniversalCustomsMessageProcessor(ApplicationCode.PLCustomsNCTS, typeof(Enterprise.Customs.PL.NCTS.Business.UniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.PL.NCTS.Business;

sealed class UniversalCustomsMessageProcessor : PL.Business.UniversalCustomsMessageProcessorBase<MessageProcessorFactory>;
