using Quartz;

namespace CargoWise.RefDbRepo.Staging.Schedulers.Common
{
	[DisallowConcurrentExecution]
	public class SingleInstanceQuartzAppRunner : QuartzProcessorRunner
	{
	}
}
