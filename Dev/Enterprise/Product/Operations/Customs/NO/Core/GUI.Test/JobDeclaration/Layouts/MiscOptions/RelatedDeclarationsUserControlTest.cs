using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing
{
	[TestedType(typeof(RelatedDeclarationsUserControl))]
	sealed class RelatedDeclarationsUserControlTest : TestCaseWithFactory
	{
		public void TestGrid()
		{
			using var control = new RelatedDeclarationsUserControl();
			using var form = new ZForm();
			form.Controls.Add(control);
			form.SetDataBinding(Factory.New<JobDeclaration>(), ".");
			form.Show();
			CombineAssertions(() =>
			{
				var grid = control.AssertContainsControl<ZGrid>("RelatedDeclarationsGrid");
				var columns = grid.Columns;
				foreach (var column in GridColumnDetails)
				{
					AssertEquals($"Caption for {column.Name}", column.Caption, grid.GetColumnCaption(column.Name));
				}
			});
		}

		(string Name, string Caption)[] GridColumnDetails => new[]
		{
			(nameof(JobDeclaration.JE_DeclarationReference), "Job Number"),
			(nameof(JobDeclaration.JE_MessageType), "Shpt Type"),
			(nameof(JobDeclaration.JE_MessageSubType), "Declaration Type"),
			(nameof(JobDeclaration.JE_EntryStatus), "Entry Status"),
			(nameof(JobDeclaration.JE_CopyStatusCaption), "Type")
		};
	}
}
