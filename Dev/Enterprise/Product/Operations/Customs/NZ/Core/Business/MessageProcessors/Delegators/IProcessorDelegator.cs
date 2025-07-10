using Enterprise.BatchProcessor;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.Business.MessageProcessors
{
	interface IProcessorDelegator
	{
		bool CanProcess(NZCMessage message);
		void Process(LoggingInformation logger, NZCMessage message);
		string MessageFriendlyName { get; }
	}
}
