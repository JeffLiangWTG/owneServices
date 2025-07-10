using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryInstructionGridUserControlTest : TestCaseWithFactory
{
	public void TestGridLayout()
	{
		using (var frm = new ZForm(declaration))
		using (var control = new EntryInstructionGridUserControl())
		{
			frm.Controls.Add(control);
			frm.Show();

			var columns = new string[]
			{
				CusEntryInstruction.Schema.CEI_Style,
				CusEntryInstruction.Schema.CEI_SubStyle,
				CusEntryInstruction.Schema.CEI_Description,
				CusEntryInstruction.Schema.CEI_Procedure,
				CusEntryInstruction.Schema.ZG_TransNature
			};

			var entryInstructionsGrid = control.FindSingle<ZGrid>("EntryInstructionsGrid");
			AssertContainsExactElementsInAnyOrder("Correct Columns", columns, entryInstructionsGrid.Columns.GetVisibleColumnMappingNames());
		}
	}

	public void TestGridColumnStyles()
	{
		using (var frm = new ZForm(declaration))
		using (var control = new EntryInstructionGridUserControl())
		{
			frm.Controls.Add(control);
			frm.Show();
			var grid = control.FindSingle<ZGrid>("EntryInstructionsGrid");

			CombineAssertions(() =>
			{
				AssertType<ZDropEditColumnStyleInfo>("CEI_Style column type", grid.GetColumnStyle(nameof(CusEntryInstruction.Schema.CEI_Style)));
				AssertType<ZDropEditColumnStyleInfo>("CEI_SubStyle column type", grid.GetColumnStyle(nameof(CusEntryInstruction.Schema.CEI_SubStyle)));
				AssertType<ZTextBoxColumnStyleInfo>("CEI_Description column type", grid.GetColumnStyle(nameof(CusEntryInstruction.Schema.CEI_Description)));
				AssertType<ZDropEditColumnStyleInfo>("CEI_Procedure column type", grid.GetColumnStyle(nameof(CusEntryInstruction.Schema.CEI_Procedure)));
				AssertType<ZDropEditColumnStyleInfo>("ZG_TransNature column type", grid.GetColumnStyle(nameof(CusEntryInstruction.Schema.ZG_TransNature)));
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
