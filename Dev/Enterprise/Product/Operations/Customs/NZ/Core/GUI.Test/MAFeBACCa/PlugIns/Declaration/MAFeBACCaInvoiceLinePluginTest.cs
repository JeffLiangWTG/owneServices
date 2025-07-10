using Enterprise.Customs.NZ.Business;
using Enterprise.Customs.NZ.Business.Declaration;

namespace Enterprise.Customs.NZ.GUI.MAFeBACCa.Testing
{
	public class MAFeBACCaInvoiceLinePluginTest : BaseMAFeBACCaDeclarationPlugInTest
	{
		public void TestGetsRightMenuAndUserControl()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			using (var plugin = new MAFeBACCaInvoiceLinePlugIn(declaration))
			{
				AssertEquals("plugin.UserControl.GetType()", typeof(MAFeBACCaInvoiceLineUserControl), plugin.UserControl.GetType());
			}
		}

		protected override BaseMAFeBACCaDeclarationPlugIn GetPluginToTest(JobDeclaration declaration)
		{
			return new MAFeBACCaInvoiceLinePlugIn(declaration);
		}
	}
}
