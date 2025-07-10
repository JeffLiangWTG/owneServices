using System.Collections.Generic;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Module.Testing;

[TestedType(typeof(JobDeclarationFilterStripControl))]
sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
{
	public void TestAddedColumns() => CombineAssertions(() =>
	{
		var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		using (var module = new JobDeclarationModule())
		using (var userControl = new JobDeclarationFilterStripControl(module, declarations, new JobDeclarationFilterBusinessObject()))
		{
			var grid = userControl.FilteredGrid;
			grid.SetDataBinding(declarations, string.Empty);
			var addedColumns = new List<(string, string)>()
				{
					(JobDeclaration.Schema.PhaseStatus, "Phase Status"),
					(JobDeclaration.Schema.PhaseStatusDescription, "Phase Status Description"),
				};

			foreach (var (column, caption) in addedColumns)
			{
				AssertEquals("Caption", caption, grid.GetColumnCaption(column));
				Assert("Not Visible", !grid.GetColumnStyle(column).IsVisible);
			}
		}
	});
}
