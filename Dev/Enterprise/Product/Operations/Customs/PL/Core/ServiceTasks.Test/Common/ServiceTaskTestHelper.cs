using Enterprise.Environment;
using ServiceManager.Integration.ServiceTasks.CW;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static NUnit.Framework.Assertion;

namespace Enterprise.Customs.PL.ServiceTasks.Testing;

static class ServiceTaskTestHelper
{
	public static void AssertNoEnvironmentExceptionThrown(string serviceTaskCode, bool canRunInAnyBranch, ServiceProviderImpl service)
	{
		using (Env.Instance.TemporaryServiceTaskContext(serviceTaskCode, canRunInAnyBranch: canRunInAnyBranch))
		{
			AssertNoExceptionThrown(service.RunTask);
		}
	}
}
