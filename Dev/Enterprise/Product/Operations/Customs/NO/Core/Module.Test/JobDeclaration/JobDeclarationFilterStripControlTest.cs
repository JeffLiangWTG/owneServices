using Enterprise.Customs.NO.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(JobDeclarationFilterStripControl))]
sealed class JobDeclarationFilterStripControlTest : Customs.Module.Testing.JobDeclarationFilterStripControlTest
{
	public void TestAddedColumns() => CombineAssertions(() =>
	{
		var declarations = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
		using var module = new JobDeclarationModule();
		using var userControl = new JobDeclarationFilterStripControl(module, declarations, new JobDeclarationFilterBusinessObject());
		var grid = userControl.FilteredGrid;
		grid.SetDataBinding(declarations, "");

		var expectedColumns = new[]
		{
			(column: JobDeclaration.Schema.PhaseStatus, caption: "Phase Status"),
			(column: JobDeclaration.Schema.PhaseStatusDescription, caption: "Phase Status Description"),
		};

		foreach (var (column, caption) in expectedColumns)
		{
			var columnStyleInfo = grid.GetColumnStyle(column);
			AssertNotNull($"Column Style Info: {column}", columnStyleInfo);
			AssertEquals($"Caption: {column}", caption, grid.GetColumnCaption(column));
			AssertEquals($"Column Visibility: {column}", expected: false, columnStyleInfo.IsVisible);
			AssertEquals($"Column ReadOnly: {column}", expected: true, columnStyleInfo.IsReadOnly);
		}
	});
}

