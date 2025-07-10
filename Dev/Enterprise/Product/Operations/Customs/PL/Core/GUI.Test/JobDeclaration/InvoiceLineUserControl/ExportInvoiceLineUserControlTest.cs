using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (ExportInvoiceLineUserControl control = new ExportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestGetAdditionalInfosUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(AdditionalInfosUserControlWithGrid), control.GetAdditionalInfosUserControlType_Exposed());
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(LayoutPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(LayoutSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlType_Exposed());
		}
	}

	public void TestGetInvoiceLineAuthorisationsUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(InvoiceLineAuthorisationsUserControl), control.GetInvoiceLineAuthorisationsUserControlType_Exposed());
		}
	}

	public void TestDescriptionTextBoxes()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
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

	public void TestControlsVisibility()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
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

	public void TestControlsReadOnly()
	{
		var declaration = GetDeclaration();
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
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

	public void TestAdditionalInfosTabPage()
	{
		using (var userControl = new ExportInvoiceLineUserControl())
		{
			var tabPage = userControl.FindSingle<ZTabPage>("AdditionalInfosTabPage");
			AssertEquals("AdditionalInfosTabPage visible", true, tabPage.TabVisible);
		}
	}

	public void TestGridColumns()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			var dec = GetDeclaration();
			control.JobDeclaration = dec;
			control.InitializeGridLayout();

			var lineGrid = control.CustomsInvoiceLinesBoundGrid;
			AssertEquals("JI_NDescription Visible", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_NDescription).IsVisible);
		}
	}

	JobDeclaration GetDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		declaration.Invoices.AddNew().InvoiceLines.AddNew();
		return declaration;
	}
}

sealed class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
{
	public Type GetAdditionalInfosUserControlType_Exposed() => base.GetAdditionalInfosUserControlType();
	public Type GetPreviousDocumentsUserControlType_Exposed() => base.GetPreviousDocumentsUserControlType();
	public Type GetSupportingDocumentsUserControlType_Exposed() => base.GetSupportingDocumentsUserControlType();
	public Type GetInvoiceLineAuthorisationsUserControlType_Exposed() => base.GetInvoiceLineAuthorisationsUserControlType();
}
