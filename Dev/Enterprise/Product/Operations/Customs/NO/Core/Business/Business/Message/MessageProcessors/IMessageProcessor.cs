using Enterprise.BatchProcessor;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

interface IMessageProcessor
{
	void ProcessMessage(EDIMessage message, LoggingInformation logger);
}
