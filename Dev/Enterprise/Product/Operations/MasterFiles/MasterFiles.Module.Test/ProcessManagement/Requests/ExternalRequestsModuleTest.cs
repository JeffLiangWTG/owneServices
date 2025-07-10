using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ExternalRequestsModule))]
	sealed class ExternalRequestsModuleTest
		: TestCaseWithFactory
	{
		public void TestModule()
		{
			using var module = new ExternalRequestsModule();

			AssertEquals(ModuleIDs.ExternalRequests, module.ID);
			AssertEquals(Env.Security.ExternalRequests, module.SecurityCheckpoint);
			AssertEquals(Env.Licence.OrderManager, module.LicenceCheckPoint);
			AssertNull("goto/Requests", module.Url);
		}
	}
}
