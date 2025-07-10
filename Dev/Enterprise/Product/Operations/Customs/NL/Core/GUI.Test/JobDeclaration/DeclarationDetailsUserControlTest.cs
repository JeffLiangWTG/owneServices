using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(JobDeclarationUserControl))]
sealed class DeclarationDetailsUserControlTest : TestCaseWithFactory
{
	public void TestDeclarationDetailsGrid_Order()
	{
		var declaration = Factory.New<JobDeclaration>();
		using (var frm = new ZForm(declaration))
		using (var userControl = new DeclarationDetailsUserControl())
		{
			frm.Controls.Add(userControl);
			frm.Show();

			var columns = new string[]
			{
				CusEntryHeader.Schema.EntryNumber,
				nameof(CusEntryHeader.CH_BGMReference),
				CusEntryHeader.Schema.CH_EntryStatus,
				CusEntryHeader.Schema.DMSFallbackIsActive
			};

			var grid = userControl.DeclarationDetailsGrid;
			AssertContainsExactElementsInExactOrder(columns, grid.Columns.GetVisibleColumnMappingNames());
		}
	}

	public void TestDeclarationDetailsGrid_Columns()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var grid = userControl.DeclarationDetailsGrid;
			CombineAssertions(() =>
			{
				var entryNumberColumnStyle = grid.GetColumnStyle(nameof(CusEntryHeader.Schema.EntryNumber));
				AssertType<ZTextBoxColumnStyleInfo>("EntryNumber column type", entryNumberColumnStyle);
				AssertEquals("EntryNumber column width", 150, entryNumberColumnStyle.Width);

				var localReferenceNumberColumnStyle = grid.GetColumnStyle(nameof(CusEntryHeader.CH_BGMReference));
				AssertType<ZTextBoxColumnStyleInfo>("LocalReferenceNumber column type", localReferenceNumberColumnStyle);
				AssertEquals("LocalReferenceNumber column width", 100, localReferenceNumberColumnStyle.Width);

				var entryStatusColumnStyle = grid.GetColumnStyle(nameof(CusEntryHeader.Schema.CH_EntryStatus));
				AssertType<ZTextBoxColumnStyleInfo>("CH_EntryStatus column type", entryStatusColumnStyle);
				AssertEquals("CH_EntryStatus column width", 100, entryStatusColumnStyle.Width);

				var fallbackIsActiveColumnStyle = grid.GetColumnStyle(nameof(CusEntryHeader.Schema.DMSFallbackIsActive));
				AssertType<ZCheckBoxColumnStyleInfo>("DMSFallbackIsActive column type", fallbackIsActiveColumnStyle);
				AssertEquals("DMSFallbackIsActive column width", 60, fallbackIsActiveColumnStyle.Width);
			});
		}
	}

	public void TestEntryStatusDropEdit()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var control = userControl.EntryStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindingMember", "CustomsEntryHeaders.CH_EntryStatus", control.GetBindingMember());
			});
		}
	}

	public void TestPhaseStatusDropEdit()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var control = userControl.PhaseStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindingMember", "CustomsEntryHeaders.CH_PhaseStatus", control.GetBindingMember());
			});
		}
	}

	public void TestMessagingStatusDropEdit()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var control = userControl.MessagingStatusDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", control);
				AssertEquals("BindingMember", "CustomsEntryHeaders.CH_Status", control.GetBindingMember());
			});
		}
	}

	public void TestAcceptanceDateTextBoxt()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var control = userControl.AcceptanceDateTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindingMember", "CustomsEntryHeaders.CusEntryNumberIssueDate", control.GetBindingMember());
			});
		}
	}

	public void TestReleaseDateTextBox()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var control = userControl.ReleaseDateTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindingMember", "CustomsEntryHeaders.CH_EntryReleaseDate", control.GetBindingMember());
			});
		}
	}

	public void TestExitEUDateTextBox()
	{
		using (var userControl = new DeclarationDetailsUserControl())
		{
			var control = userControl.ExitEUDateTextBox;
			CombineAssertions(() =>
			{
				AssertType<ZTextBox>("Type", control);
				AssertEquals("BindingMember", "CustomsEntryHeaders.CH_ExitDate", control.GetBindingMember());
			});
		}
	}
}
