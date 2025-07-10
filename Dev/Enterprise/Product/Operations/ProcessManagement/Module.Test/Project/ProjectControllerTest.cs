using Enterprise.Environment;
using Enterprise.MasterFiles.Module.Testing;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ProcessManagement.Module.Test
{
	[TestedType(typeof(ProjectController))]
	class ProjectControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.Project;
		}
		public void TestCRMSecurityCheckpoints()
		{
			var bizObjWithoutAccess = Factory.NewWithValidTestData<Project>();
			CRMSecurityProviderTest<Project>.AssertController(Controller, bizObjWithoutAccess, Env.Security.ProjectCRMSecurity);
		}
	}
}
