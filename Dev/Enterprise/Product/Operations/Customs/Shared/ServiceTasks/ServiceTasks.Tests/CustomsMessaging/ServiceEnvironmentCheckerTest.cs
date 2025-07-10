using Enterprise.Customs.Business.BatchProcessor;

namespace Enterprise.Customs.ServiceTasks.Testing
{
	class ServiceEnvironmentCheckerTest : Business.BatchProcessor.Testing.BaseEnvironmentCheckerTest
	{
		protected override BaseEnvironmentChecker GetEnvironmentChecker()
		{
			return new ServiceEnvironmentChecker();
		}
	}
}
