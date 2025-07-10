using Enterprise.Customs.EU.Module;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Module.Testing;

sealed class JobDeclarationFilterStripControlTest : EU.Module.Testing.JobDeclarationFilterStripControlTest
{
	public void TestPLAdditionalColumnsExist()
	{
		var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		var filterBO = new JobDeclarationFilterBusinessObject();
		using var module = new JobDeclarationModule();
		using var userControl = new JobDeclarationFilterStripControl(module, declarations, filterBO);
		var grid = userControl.FilteredGrid;
		AssertNotNull(grid.GetColumnStyle(JobDeclaration.Schema.EntryInstructionSubStyle));
	}
}
