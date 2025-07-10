using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Module.Testing
{
	class ProtestFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFilterGridColorContextKey()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new ProtestModule())
			using (var filterControl = new ProtestFilterStripControl(declarations, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals("", filterControl.FilteredGrid.ColorContextKey);
			}
		}
	}
}
