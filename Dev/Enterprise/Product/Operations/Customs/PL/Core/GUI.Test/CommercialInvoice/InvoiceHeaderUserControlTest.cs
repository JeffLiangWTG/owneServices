using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.PL.GUI.Testing;

sealed class InvoiceHeaderUserControlTest : Customs.GUI.Testing.InvoiceHeaderUserControlAbstractTest
{
	public void TestControlsVisibility()
	{
		using (var control = GetNewInvoiceHeaderUserControl())
		{
			BaseJobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			control.Invoice = invoice;
			var visibleControl = control.Controls.Find("JZ_InvoiceCurrExRateCalcEdit", true).Single();
			AssertEquals("JZ_InvoiceCurrExRateCalcEdit should not be visible", false, visibleControl.Visible);
			visibleControl = control.Controls.Find("ValuationMethodDropEdit", true).Single();
			AssertEquals("ValuationMethodDropEdit should be visible", true, visibleControl.Visible);
			visibleControl = control.Controls.Find("JZ_ValuationCodeDropEdit", true).Single();
			AssertEquals("JZ_ValuationCodeDropEdit should be visible", true, visibleControl.Visible);
		}
	}

	public void TestControlsVisibility_Export()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var control = GetNewInvoiceHeaderUserControl())
			{
				control.Invoice = invoice;
				form.Controls.Add(control);
				form.Show();
				var visibleControl = control.Controls.Find("ExportJZ_IncoTermDropDownEdit", true).Single();
				AssertEquals("ExportJZ_IncoTermDropDownEdit should be visible", true, visibleControl.Visible);
				visibleControl = control.Controls.Find("JZ_IncoTermPlaceTextBox", true).Single();
				AssertEquals("JZ_IncoTermPlaceTextBox should be visible", true, visibleControl.Visible);
				visibleControl = control.Controls.Find("AgreedPlaceCodeFindBox", true).Single();
				AssertEquals("AgreedPlaceCodeFindBox should be visible", true, visibleControl.Visible);
				visibleControl = control.Controls.Find("ExportIncoTermExplainButton", true).Single();
				AssertEquals("ExportIncoTermExplainButton should be visible", true, visibleControl.Visible);
				visibleControl = control.Controls.Find("JZ_IncoTermDropDownEdit", true).Single();
				AssertEquals("JZ_IncoTermDropDownEdit should not be visible", false, visibleControl.Visible);
				visibleControl = control.Controls.Find("IncoTermExplainButton", true).Single();
				AssertEquals("IncoTermExplainButton should not be visible", false, visibleControl.Visible);
			}
		}
	}

	public void TestControlsVisibility_NotExport()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
		BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
		using (var form = new ZForm(declaration))
		{
			using (var control = GetNewInvoiceHeaderUserControl())
			{
				control.Invoice = invoice;
				form.Controls.Add(control);
				form.Show();
				var visibleControl = control.Controls.Find("ExportJZ_IncoTermDropDownEdit", true).Single();
				AssertEquals("ExportJZ_IncoTermDropDownEdit should not be visible", false, visibleControl.Visible);
				visibleControl = control.Controls.Find("JZ_IncoTermPlaceTextBox", true).Single();
				AssertEquals("JZ_IncoTermPlaceTextBox should not be visible", false, visibleControl.Visible);
				visibleControl = control.Controls.Find("AgreedPlaceCodeFindBox", true).Single();
				AssertEquals("AgreedPlaceCodeFindBox should not be visible", false, visibleControl.Visible);
				visibleControl = control.Controls.Find("ExportIncoTermExplainButton", true).Single();
				AssertEquals("ExportIncoTermExplainButton should not be visible", false, visibleControl.Visible);
				visibleControl = control.Controls.Find("JZ_IncoTermDropDownEdit", true).Single();
				AssertEquals("JZ_IncoTermDropDownEdit should be visible", true, visibleControl.Visible);
				visibleControl = control.Controls.Find("IncoTermExplainButton", true).Single();
				AssertEquals("IncoTermExplainButton should be visible", true, visibleControl.Visible);
			}
		}
	}

	protected override Customs.GUI.CommonInvoiceHeaderUserControl GetNewInvoiceHeaderUserControl() => new InvoiceHeaderUserControl();
}
