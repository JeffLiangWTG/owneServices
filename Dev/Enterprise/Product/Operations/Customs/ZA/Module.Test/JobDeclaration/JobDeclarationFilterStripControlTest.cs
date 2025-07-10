using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.Module.Testing
{
	sealed class JobDeclarationFilterStripControlTest : TestCaseWithFactory
	{
		public void TestFilteredGridColumns()
		{
			var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBusinessObject = new JobDeclarationFilterBusinessObject();
			using (var form = new ZForm())
			using (var filterStrip = new JobDeclarationFilterStripControl(null, declarations, filterBusinessObject))
			{
				form.Controls.Add(filterStrip);
				form.Show();
				var filteredGrid = filterStrip.FilteredGrid;
				AssertNull(filteredGrid.Columns[JobDeclaration.Schema.JE_DateOfFirstArrival]);
				AssertEquals(true, filteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_DateOfFirstArrival).IsUnavailable);
				AssertNull(filteredGrid.Columns[JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival]);
				AssertEquals(true, filteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_RL_NKPortOfFirstArrival).IsUnavailable);
				AssertNotNull(filteredGrid.Columns["CombinedUCREntryNumbers"]);
				AssertNotNull(filteredGrid.Columns["CombinedReleasePrintIndicator"]);
			}
		}
	}
}
