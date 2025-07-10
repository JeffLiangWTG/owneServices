using System.Windows.Forms;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(EntrySelectionForm))]
sealed class EntrySelectionFormTest : ZFormBasherTest
{
	public void TestDataSourceType()
	{
		using (var form = (EntrySelectionForm)GetFormToBashCore())
		{
			AssertEquals(typeof(JobDeclaration), form.DataSourceType);
		}
	}

	public void TestFieldVisibilityAndBindings()
	{
		using (var form = (EntrySelectionForm)GetFormToBashCore())
		{
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("EntrySelectionGridControl.Visible", true, form.EntrySelectionGrid.Visible);
				AssertEquals("EntrySelectionGridControl.BindTo", "EntrySelections", form.EntrySelectionGrid.BindTo);

				AssertEquals("EntrySelectionGridGroupBox.Visible", true, form.EntrySelectionGridGroupBox.Visible);

				AssertEquals("OK.Visible", true, form.OkButton.Visible);
				AssertEquals("Cancel.Visible", true, form.CloseButton.Visible);
			});
		}
	}

	public void TestEntrySelectionGrid()
	{
		using (var form = (EntrySelectionForm)GetFormToBashCore())
		{
			AssertEquals(RemoveAction.NoRemovePossible, form.EntrySelectionGrid.RemoveAction);
		}
	}

	public void TestEntrySelectionGrid_Columns()
	{
		using (var form = (EntrySelectionForm)GetFormToBashCore())
		{
			form.Show();

			var grid = form.EntrySelectionGrid;

			CombineAssertions(() =>
			{
				var selectedColumnStyle = grid.GetColumnStyle(nameof(EntrySelection.Selected));
				AssertType<ZCheckBoxColumnStyleInfo>("Selected column type", selectedColumnStyle);
				AssertEquals("Selected column width", 70, selectedColumnStyle.Width);

				var entryNumberColumnStyle = grid.GetColumnStyle(nameof(EntrySelection.EntryNumber));
				AssertType<ZTextBoxColumnStyleInfo>("Entry Number column type", entryNumberColumnStyle);
				AssertEquals("Entry Number column width", 200, entryNumberColumnStyle.Width);
			});
		}
	}

	protected override string CountryCode => Core.Constants.CountryCodes.Netherlands;

	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		Factory.Save();
#pragma warning disable CW1199 // Do Not Use Unnecessary Resource String In Unit Tests --> We need this for the TestFormIsFullyTranslatable to succeed
		return new EntrySelectionForm(declaration, Res.GetData("5F933C8E-9B2E-4E84-9EFD-5F5598B79AB6", "Test title"));
#pragma warning restore CW1199 // Do Not Use Unnecessary Resource String In Unit Tests
	}
}
