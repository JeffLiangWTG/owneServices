using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	public abstract class BaseMAFeBACCaDeclarationPlugInTest : TestCaseWithFactory
	{
		public void TestVisibilityOnConstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
			using (var plugin = GetPluginToTest(declaration))
			{
				AssertEquals("plugin.Enabled", false, plugin.Enabled);
			}

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var plugin = GetPluginToTest(declaration))
			{
				AssertEquals("plugin.Enabled", true, plugin.Enabled);
			}
		}

		protected abstract BaseMAFeBACCaDeclarationPlugIn GetPluginToTest(JobDeclaration declaration);
	}
}
