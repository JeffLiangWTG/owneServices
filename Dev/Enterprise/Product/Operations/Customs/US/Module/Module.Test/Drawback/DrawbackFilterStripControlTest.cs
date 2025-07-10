using Enterprise.Customs.US.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.US.Module.Testing
{
	class DrawbackFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
	{
		public void TestFilterGridColorContextKey()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new DrawbackModule())
			using (var filterControl = new DrawbackFilterStripControl(declarations, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals(ModuleIDs.Customs.US.Drawback.Name, filterControl.FilteredGrid.ColorContextKey);
			}
		}
	}
}
