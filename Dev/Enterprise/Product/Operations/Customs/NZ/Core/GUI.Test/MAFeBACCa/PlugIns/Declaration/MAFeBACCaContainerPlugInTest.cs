using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	public class MAFeBACCaContainerPlugInTest : BaseMAFeBACCaDeclarationPlugInTest
	{
		public void TestVisibilityOnMessageTypeChange()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var plugin = GetPluginToTest(declaration))
			{
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.CUS;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("plugin.Enabled", false, plugin.Enabled);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("plugin.Enabled", true, plugin.Enabled);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("plugin.Enabled", false, plugin.Enabled);
			}
		}

		public void TestGetsRightUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var plugin = new MAFeBACCaContainerPlugIn(declaration))
			{
				AssertEquals("plugin.UserControl.GetType()", typeof(MAFeBACCaContainerUserControl), plugin.UserControl.GetType());
			}
		}

		protected override BaseMAFeBACCaDeclarationPlugIn GetPluginToTest(JobDeclaration declaration)
		{
			return new MAFeBACCaContainerPlugIn(declaration);
		}
	}
}
