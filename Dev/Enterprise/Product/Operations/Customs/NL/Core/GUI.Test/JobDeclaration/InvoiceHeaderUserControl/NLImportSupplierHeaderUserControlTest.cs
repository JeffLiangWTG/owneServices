using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(NLImportSupplierHeaderUserControl))]
sealed class NLImportSupplierHeaderUserControlTest : LayoutCustomsSupplierHeaderUserControlAbstractTest<NLImportSupplierHeaderUserControl, JobDeclaration>
{
	public void TestPreviousDocumentsUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals(typeof(NLPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestSupportingDocumentsUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals(typeof(NLSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlTypeExposed());
		}
	}

	public void TestColumnsAdded()
	{
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			var columns = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.OfType<ZGridColumnInfo>().Select(x => x.ColumnName);
			AssertCollectionContains("JZ_UCR", columns);
		}
	}

	public void TestAdditionalInfoTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUSupplierHeaderUserControlTabPageCaptionAndUserControlType<NLImportSupplierHeaderUserControl>(Factory.New<JobDeclaration>(), "AdditionalInfoTabPage", "additionalInfosUserControl1", "[44] Additional Documents", typeof(InvoiceLineAdditionalInfosUserControlWithGrid));
	}

	public void TestIntracommunityReceiverFindBox()
	{
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			var column = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.ConsigneeOrgPK);
			CombineAssertions(() =>
			{
				AssertEquals("Column Visible", true, column.IsVisible);
				AssertEquals("Caption", "Intra-community Receiver", column.CaptionResourceString.Caption);
				AssertEquals("Group Key", "BD76BB97-A618-4966-A9AC-AD49F21513BF", column.GroupName.Key);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150), column.Width);
			});
		}
	}

	public void TestIntracommunityReceiverDropEdit()
	{
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			control.InitializeGridLayout();
			var column = control.JobComInvoiceHeadersBoundGrid.ColumnStyles.Cast<ZGridColumnInfo>().Single(x => x.ColumnName == JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress);
			CombineAssertions(() =>
			{
				AssertEquals("Column Visible", true, column.IsVisible);
				AssertEquals("Caption", "Intra-community Receiver Address", column.CaptionResourceString.Caption);
				AssertEquals("Group Key", "BD76BB97-A618-4966-A9AC-AD49F21513BF", column.GroupName.Key);
				AssertEquals("Width", CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180), column.Width);
			});
		}
	}

	public void TestValueIndicatorsUserControlType()
	{
		using (var control = new ImportSupplierHeaderUserControlForTest())
		{
			AssertEquals(typeof(NLValueIndicatorsUserControl), control.GetValueIndicatorsUserControlTypeExposed());
		}
	}

	public void TestValueIndicatorsTabPage()
	{
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			AssertEquals("[UCC 4/13] Value Indicators", control.FindSingle<ZTabPage>("ValueIndicatorsTabPage").CaptionResourceString.Caption);
		}
	}

	public void TestTabPagesOrder()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		using (var form = new ZForm(declaration))
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			control.SetDataBinding(declaration, "");
			form.Show();
			var tabPages = control.InvoiceTabControl.TabPages;
			AssertArrayEqualsByElements(new[]
			{
				"ComInvoiceDetailsTabPage",
				"SupportingDocumentsTabPage",
				"AdditionalInfoTabPage" ,
				"CustomFieldsTabPage"
			}, tabPages.Cast<ZTabPage>().Where(x => x.TabVisible).Select(x => x.Name).ToArray());
		}
	}

	public void TestSupportingDocumentsTabPageCaption()
	{
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			AssertEquals("Supporting Documents", control.FindSingle<ZTabPage>("SupportingDocumentsTabPage").CaptionResourceString.Caption);
		}
	}

	public void TestSetValueIndicatorsTabPageVisible()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine = invoice.InvoiceLines.AddNew();
		entryInstruction.CEI_Style = "H1";
		entryInstruction.ZG_IsHighValueOvrd = true;
		invoiceLine.JI_CEI = entryInstruction.PK;

		using (var form = new ZForm(declaration))
		using (var control = new NLImportSupplierHeaderUserControl())
		{
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();

			AssertEquals("When Valuation check box is checked, the valuation indicators tab page should be visible", true, control.InvoiceTabControl.FindSingle<ZTabPage>("ValueIndicatorsTabPage").TabVisible);
		}
	}

	protected override IEnumerable<string> ExpectedControlList => DefaultControlList.Except(new[] { "GroupInvoiceDropEdit" }).Union(new[] { "AgreedPlaceCodeFindBox", "TransportChargesMethodOfPaymentDropEdit" });

	sealed class ImportSupplierHeaderUserControlForTest : NLImportSupplierHeaderUserControl
	{
		public Type GetSupportingDocumentsUserControlTypeExposed() => GetSupportingDocumentsUserControlType();

		public Type GetPreviousDocumentsUserControlTypeExposed() => GetPreviousDocumentsUserControlType();

		public Type GetValueIndicatorsUserControlTypeExposed() => GetValueIndicatorsUserControlType();
	}
}
