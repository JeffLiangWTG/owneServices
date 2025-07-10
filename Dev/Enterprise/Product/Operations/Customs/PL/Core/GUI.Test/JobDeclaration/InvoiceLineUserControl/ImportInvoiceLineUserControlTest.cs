using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(ImportInvoiceLineUserControl))]
sealed class ImportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (ImportInvoiceLineUserControl control = new ImportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be import", nameof(Customs.GUI.DeclarationType.Import), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestDescriptionTextBoxes()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("JI_Description TextBox", true, control.FindSingleOrDefault<LongTextControl>("DescriptionLongTextControl").Visible);
				AssertEquals("JI_NDescription TextBox", true, control.FindSingleOrDefault<LongTextControl>("UnicodeDescriptionLongTextControl").Visible);
			});
		}
	}

	public void TestGetAdditionalInfosUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(AdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlType_Exposed());
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(LayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetOrganizationsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(EU.GUI.PlugIn.InvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlType_Exposed());
		}
	}

	public void TestGetValuationIndicatorsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(EU.GUI.InvoiceLineValuationIndicatorDropEditsUserControl), control.GetValuationIndicatorsUserControlType_Exposed());
		}
	}

	public void TestControlsVisibility()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			CombineAssertions(() =>
			{
				AssertEquals("zTextBoxAddtionalProcedureCodeAsString should be visible", true, control.FindSingle<ZTextBox>("AdditionalProcedureCodesAsStringTextBox").Visible);
				AssertEquals("zButtonMoreAdditionalProcedureCode should be visible", true, control.FindSingle<ZButton>("AdditionalProcedureCodesEditButton").Visible);
				AssertEquals("RequestedCustomsProcedureCodeDropEdit should be visible", true, control.FindSingle<ZDropEdit>("RequestedCustomsProcedureCodeDropEdit").Visible);
				AssertEquals("PreviousCustomsProcedureCodeDropEdit should be visible", true, control.FindSingle<ZDropEdit>("PreviousCustomsProcedureCodeDropEdit").Visible);
				if (control.FindSingleOrDefault<ZCodeFindBox>("CPCFindBox") is ZCodeFindBox cpcFindBox) // when not using control bag layout
				{
					AssertEquals("CPCFindBox should not be visible", false, cpcFindBox.Visible);
				}
			});
		}
	}

	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfosTabPage", "additionalInfosUserControl1", "[44] Additional Info", typeof(AdditionalInfosUserControlWithGrid));
	}

	public void TestControlsReadOnly()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();

			var invoiceLine = declaration.Invoices.First().InvoiceLines.Cast<JobComInvoiceLine>().First();
			var guaranteesGrid = control.FindSingle<ZGrid>("CustomsInvoiceLinesBoundGrid");
			guaranteesGrid.SelectSingleElement(invoiceLine);
			var previousProcedureControl = control.FindSingle<ZDropEdit>("PreviousCustomsProcedureCodeDropEdit");

			CombineAssertions(() =>
			{
				AssertEquals("PreviousCustomsProcedureCodeDropEdit should be read only", true, previousProcedureControl.ReadOnly);
				invoiceLine.JI_Procedure = "1000000";
				AssertEquals("PreviousCustomsProcedureCodeDropEdit should not be read only", false, previousProcedureControl.ReadOnly);
			});
		}
	}

	public void TestCarInfoTabPage()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			AssertEquals("Car Information", control.CarInfoTabPage.CaptionResourceString.Caption);
		}
	}

	public void TestCarInfoTabControl()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			AssertEquals("FilteredInvoiceLines", control.CarInfoUserControl.GetBindingMember());
		}
	}

	public void TestGridColumns()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			var dec = GetDeclaration();
			control.JobDeclaration = dec;
			control.InitializeGridLayout();

			var lineGrid = control.CustomsInvoiceLinesBoundGrid;
			CombineAssertions(() =>
			{
				AssertEquals("JI_NDescription Visible", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NDescription).IsVisible);
				AssertEquals("JI_ValuationDateOverride Visible", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationDateOverride).IsVisible);
				AssertEquals("JI_DateForDutyOverride Visible", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_DateForDutyOverride).IsVisible);
				AssertEquals("JI_ValuationCode Visible", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationCode).IsVisible);
				AssertEquals("JI_ValuationCode IsMandatory", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationCode).IsMandatory);
			});
		}
	}

	public void TestDefaultColumnsForGrid()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = GetDeclaration();
			control.InitializeGridLayout();

			var lineGrid = control.CustomsInvoiceLinesBoundGrid;
			var styles = lineGrid.ColumnStyles;
			AssertEquals(25, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_ValuationCode)));
		}
	}

	JobDeclaration GetDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		return declaration;
	}
}

class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
{
	public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();

	public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();

	public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();

	public Type GetOrganizationsUserControlType_Exposed() => base.GetOrganizationsUserControlType();

	public Type GetValuationIndicatorsUserControlType_Exposed() => base.GetValuationIndicatorsUserControlType();
}
