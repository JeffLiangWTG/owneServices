using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UpdateNotesPortalModule))]
	sealed class UpdateNotesPortalModuleTest : TestCaseWithFactory
	{
		public void TestModule()
		{
			using var module = new UpdateNotesPortalModule();

			AssertEquals(ModuleIDs.UpdateNotesPortal, module.ID);
			AssertEquals(Env.Security.None, module.SecurityCheckpoint);
			AssertEquals(Env.Licence.AlwaysAllow, module.LicenceCheckPoint);
			AssertNull("Url property should return null", module.Url);
		}

		public void TestModule_Show()
		{
			var helpMenuProviderMock =  new Mock<IHelpMenuProvider>();

			using (ObjectFactory.Substitute(helpMenuProviderMock.Object))
			{
				using var module = new UpdateNotesPortalModule();

				AssertNoExceptionThrown(module.Show);

				helpMenuProviderMock.Verify(m => m.ShowWiseTechAcademy(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
			}
		}
	}
}
