using Enterprise.Customs.PL.Business;
using Enterprise.Messaging.Business.MessageProcessor;
using ApplicationCode = Enterprise.Messaging.Integration.ApplicationCodeList.Codes;

[assembly: UniversalCustomsMessageProcessor(ApplicationCode.PLCustoms, typeof(UniversalCustomsMessageProcessor))]

namespace Enterprise.Customs.PL.Business;

sealed class UniversalCustomsMessageProcessor : UniversalCustomsMessageProcessorBase<MessageProcessorFactory>;
