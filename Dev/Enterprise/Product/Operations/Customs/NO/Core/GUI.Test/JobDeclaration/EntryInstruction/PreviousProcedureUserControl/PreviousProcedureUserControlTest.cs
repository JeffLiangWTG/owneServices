using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NO.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(PreviousProcedureUserControl))]
sealed class PreviousProcedureUserControlTest : TestCaseWithFactory
{
	public void TestDataSourceType()
	{
		using var control = new PreviousProcedureUserControl();
		AssertEquals("BindingSource.DataSourceType", typeof(CusEntryInstruction), control.BindingSource.DataSourceType);
	}

	public void TestControls()
	{
		using var control = new PreviousProcedureUserControl();
		CombineAssertions(() =>
		{
			_ = control.AssertContainsControl<ZGrid>("PreviousProceduresGrid", g =>
				g.WithBindTo("PreviousDocuments"));

			_ = control.AssertContainsControl<DynamicLayoutPanel>("PreviousProcedureHeaderDynamicPanel", h =>
				h.WithBindTo("PreviousDocumentMaster"));
		});
	}

	public void TestGridControlLayout()
	{
		using var control = new PreviousProcedureUserControlForTest();
		AssertType<PreviousProcedureGridLayout>(control.PreviousProcedureGridLayout_Exposed);
	}

	public void TestPreviousProcedureHeaderLayout()
	{
		using var control = new PreviousProcedureUserControlForTest();
		AssertType<PreviousProcedureHeaderLayout>(control.PreviousProcedureHeaderLayout_Exposed);
	}

	public void TestGridColumns()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();

		using var form = new ZForm(entryInstruction);
		using var control = new PreviousProcedureUserControlForTest();
		form.Controls.Add(control);
		form.Show();

		var grid = control.AssertContainsControl<ZGrid>("PreviousProceduresGrid");
		CombineAssertions(() =>
		{
			foreach (var (bindTo, width) in previousProcedureGridColumns)
			{
				var columnInfo = grid.GetColumnStyle(bindTo);
				AssertEquals($"{bindTo} column width", ControlDpiScalingHelper.ScaleToCurrentDpiX(width), columnInfo.Width);
				AssertEquals($"{bindTo} column visibility", expected: true, columnInfo.IsVisible);
			}
		});
	}

	public void TestHeaderControls() => CombineAssertions(() =>
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryInstructions.AddNew();
		using var form = new ZForm(declaration);
		using var control = new PreviousProcedureUserControlForTest();
		control.SetDataBinding(declaration, nameof(JobDeclaration.CustomsEntryInstructions));
		form.Controls.Add(control);
		form.Show();

		var headerDynamicLayout = control.AssertContainsControl<DynamicLayoutPanel>("PreviousProcedureHeaderDynamicPanel");
		headerDynamicLayout.AssertContainsControl<ZDropEdit>("PreviousProcedureDropEdit");
	});

	static readonly IEnumerable<(string bindTo, int width)> previousProcedureGridColumns = new (string bindTo, int width)[]
	{
		(PreviousDocument.Schema.CSI_Code,  50),
		(PreviousDocument.Schema.CSI_ReferenceNumber, 150),
		(PreviousDocument.Schema.CSI_ReferenceNumber2, 150),
		(PreviousDocument.Schema.CSI_LineNo, 70),
		(PreviousDocument.Schema.CSI_Quantity, 70),
		(PreviousDocument.Schema.CSI_Procedure, 100),
	};

	sealed class PreviousProcedureUserControlForTest : PreviousProcedureUserControl
	{
		public IGridColumnLayoutProvider PreviousProcedureGridLayout_Exposed => PreviousProcedureGridLayout;

		public IPanelLayoutProvider PreviousProcedureHeaderLayout_Exposed => PreviousProcedureHeaderLayout;
	}
}
