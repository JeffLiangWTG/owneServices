using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(EntryInstructionDetailsUserControl))]
sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
{
	public void TestEntryInstructionGrid_Import() => AssertEntryInstructionGrid(Common.Shared.SharedJobMessageTypeList.Codes.Import);

	public void TestEntryInstructionGrid_Export() => AssertEntryInstructionGrid(Common.Shared.SharedJobMessageTypeList.Codes.Export);

	public void TestDetailsUserControl_Import() => AssertDetailsUserControl(Common.Shared.SharedJobMessageTypeList.Codes.Import);

	public void TestDetailsUserControl_Export() => AssertDetailsUserControl(Common.Shared.SharedJobMessageTypeList.Codes.Export);

	void AssertEntryInstructionGrid(string messageType)
	{
		declaration.JE_MessageType = messageType;
		using var form = new ZForm(declaration);
		using var userControl = new EntryInstructionDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();
		CombineAssertions(() =>
		{
			var grid = userControl.AssertContainsControl<ZGrid>("EntryInstructionsGrid", x => x
				.WithBindTo("CustomsEntryInstructions")
			);
			AssertEquals("Columns.Count", 4, grid.ColumnStyles.Count);

			foreach (var (bindTo, casing, width) in entryInstructionControlsGrid)
			{
				var columnInfo = grid.GetColumnStyle(bindTo);
				AssertEquals($"{bindTo} column character case", casing, columnInfo.CharacterCasing);
				AssertEquals($"{bindTo} column width", ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
				AssertEquals($"{bindTo} column visibility", expected: true, columnInfo.IsVisible);
			}
		});
	}

	void AssertDetailsUserControl(string messageType)
	{
		declaration.JE_MessageType = messageType;
		using var form = new ZForm(declaration);
		using var userControl = new EntryInstructionDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();

		var tabControl = userControl.AssertContainsControl<ZTabControl>("EntryInstructionTabControl");
		var detailsTab = tabControl.AssertContainsControl<ZTabPage>("DetailsTabPage");
		_ = detailsTab.AssertContainsControl<DynamicLayoutPanel>("DetailsUserControl", x => x.WithBindTo("CustomsEntryInstructions"));
	}

	public void TestTabControl()
	{
		using var userControl = new EntryInstructionDetailsUserControl();
		var tabControl = userControl.AssertContainsControl<ZTabControl>("EntryInstructionTabControl");
		AssertEquals("Number Of Tabs", 2, tabControl.TabCount);
	}

	public void TestPreviousDocsTabPage()
	{
		CreateRefCusProcedureCodeDataForTest();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		entryInstruction.CEI_Procedure = "0471";

		using var form = new ZForm(declaration);
		using var userControl = new EntryInstructionDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();

		var tabControl = userControl.AssertContainsControl<ZTabControl>("EntryInstructionTabControl");
		tabControl.SelectTab("PreviousProcedureTabPage");
		var tabPage = tabControl.AssertContainsControl<ZTabPage>("PreviousProcedureTabPage");

		var hostedControl = tabPage.AssertContainsControl<ZDynamicControlCreationUserControl>("PreviousProcedureHostedControl",
			x => x.WithBindTo("CustomsEntryInstructions"));
		AssertEquals("PreviousProcedureHostedControl.UserControlType", typeof(PreviousProcedureUserControl), hostedControl.UserControlType);
	}

	public void TestPreviousDocsTab_Visible()
	{
		CreateRefCusProcedureCodeDataForTest();
		declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using var form = new ZForm(declaration);
		using var userControl = new EntryInstructionDetailsUserControl();
		form.Controls.Add(userControl);
		form.Show();

		userControl.EntryInstructionsGrid.Select(0);

		var tabControl = userControl.AssertContainsControl<ZTabControl>("EntryInstructionTabControl");

		const string PreviousProcedureTabPageName = "PreviousProcedureTabPage";

		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = "0471";
			AssertEquals("Tabs Count when Procedure With OutOfWarehouse=True", 2, tabControl.TabPages.Count);
			tabControl.SelectTab(PreviousProcedureTabPageName);
			tabControl.AssertContainsControl<ZTabPage>(PreviousProcedureTabPageName);

			entryInstruction.CEI_Procedure = "5710";
			AssertEquals("Tabs Count when Procedure With OutOfWarehouse=False", 1, tabControl.TabPages.Count);
			tabControl.AssertDoesNotContainsControl<ZTabPage>(PreviousProcedureTabPageName);
		});
	}

	void CreateRefCusProcedureCodeDataForTest()
	{
		var referenceTestDataHelper = new UniversalReferenceTestDataHelper(Factory);
		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "04",
			previousProcedureCode: "71",
			concession: ZString.Empty,
			description: "Description1",
			shipmentType: "IMP",
			outOfWarehouse: true);

		_ = referenceTestDataHelper.CreateRefCusProcedure(dataGroupingCode: Core.Constants.CountryCodes.Norway,
			category: ZString.Empty,
			procedureCode: "57",
			previousProcedureCode: "10",
			concession: ZString.Empty,
			description: "Description 2",
			shipmentType: "IMP",
			outOfWarehouse: false);
	}

	static readonly IEnumerable<(string bindTo, CharacterCasing? casing, int width)> entryInstructionControlsGrid = new (string bindTo, CharacterCasing? casing, int width)[]
	{
		(Customs.Business.AutoCusEntryInstruction.Schema.CEI_Style, CharacterCasing.Upper, 47),
		(Customs.Business.AutoCusEntryInstruction.Schema.CEI_Description, CharacterCasing.Normal, 186),
		(Customs.Business.AutoCusEntryInstruction.Schema.CEI_Procedure, CharacterCasing.Upper, 74),
		("ProcedureDescription", CharacterCasing.Normal, 580),
	};

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
	}
	JobDeclaration declaration;
}
