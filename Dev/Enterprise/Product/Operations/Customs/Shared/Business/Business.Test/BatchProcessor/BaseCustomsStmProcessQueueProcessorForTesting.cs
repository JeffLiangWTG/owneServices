using Enterprise.BatchProcessor;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.Business.BatchProcessor.Testing
{
	sealed class BaseCustomsStmProcessQueueProcessorForTesting : BaseCustomsStmProcessQueueBatchProcessor
	{
		public BaseCustomsStmProcessQueueProcessorForTesting(LoggingInformation logger) : base(logger)
		{
		}

		protected override string ProcessQueueApplicationCode => "~T~";

		protected override void ProcessQueuedItemCore(StmProcessQueue queuedItem)
		{
			Logger.LogWarning("Item has been processed.");
		}
	}
}
