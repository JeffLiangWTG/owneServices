using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(ExportInvoiceLineUserControl))]
sealed class ExportInvoiceLineUserControlTest : TestCaseWithFactory
{
	public void TestColumnLayoutContextForInvoiceLinesGrid()
	{
		using (ExportInvoiceLineUserControl control = new ExportInvoiceLineUserControl())
		{
			AssertEquals("Column layout context should be export", nameof(Customs.GUI.DeclarationType.Export), control.CustomsInvoiceLinesBoundGrid.ColumnLayoutContext);
		}
	}

	public void TestJI_AdditionalSupplementsTextBoxVisible()
	{
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();
			var additionalProcedureCodesTextBox = control.FindSingle<AdditionalSupplementaryCodesAndGDMUserControl>("AdditionalSupplementaryCodesAndGDMUserControl");

			Assert("Additional procedures text box visible", additionalProcedureCodesTextBox.Visible);
		}
	}

	public void TestGDMLink()
	{
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var gdmLink = control.Controls.Find("GDMLink", true).Single();
			AssertEquals("GDMLink is visible", true, gdmLink.Visible);
		}
	}

	public void TestCountryOfDestination()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			var destinationCodeFindBox = control.FindSingle<ZCodeFindBox>("DestinationCodeFindBox", 4);

			CombineAssertions(() =>
			{
				AssertEquals("ModuleID of DestinationCodeFindBox", ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList, destinationCodeFindBox.ModuleID);
				AssertNull("BindToList of DestinationCodeFindBox", destinationCodeFindBox.BindToList);
			});
		}
	}

	public void TestTabPagesOrder()
	{
		using (var form = new ZForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();
			var tabPages = control.LineDetailTabControl.TabPages;
			AssertArrayEqualsByElements(new[] { "NewLineDetailsTabPage", "OrganizationsTabPage", "LineChargesTabPage", "SupportingDocumentsTabPage" , "AdditionalInfosTabPage",
				"PreviousDocumentsTabPage", "PackagesPivotTabPage", "DangerousGoodsTabPage", "AuthorisationsTabPage", "CustomFieldsTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(NLSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlTypeExposed());
		}
	}

	public void TestAdditionalDocumentsControlVisibility()
	{
		using (var form = new ZForm())
		using (var control = new ExportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var additionalInfoTabPage = control.FindSingle<ZTabPage>("AdditionalInfosTabPage");
			control.LineDetailTabControl.SelectedTab = additionalInfoTabPage;

			var allControls = additionalInfoTabPage.FindAll<ZUserControl>();
			var additionalDocumentsUserControl = additionalInfoTabPage.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
			AssertNotNull("InvoiceLineAdditionalInfosUserControlWithGrid", additionalDocumentsUserControl);
			Assert("InvoiceLineAdditionalInfosUserControlWithGrid must be visible", additionalDocumentsUserControl.Visible);
		}
	}

	public void TestZG_TransNatureVisibility()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var control = new ExportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();

			var transactionNatureDropEdit = control.FindSingle<ZDropEdit>("TransactionNatureDropEdit");
			Assert("[24] Tran. Nature should be visible.", transactionNatureDropEdit.Visible);
		}
	}

	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ExportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(InvoiceLineAdditionalInfosUserControlWithGrid));
	}

	public void TestCountryOfExportInGrid()
	{
		using (var control = new ExportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			var lineGrid = control.CustomsInvoiceLinesBoundGrid;
			var styles = lineGrid.ColumnStyles;

			AssertEquals("Country of Export should be visible in the grid.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.JI_RN_NKCountryOfExport).IsVisible);
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(NLPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestGetPreviousDocumentsTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/1] Previous Documents", control.GetPreviousDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetSupportingDocumentsTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/3] Supporting Documents", control.GetSupportingDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetPackagesTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("Packages", control.GetPackagesTabPageCaptionExposed());
		}
	}

	public void TestGetLineChargesTabPageCaption()
	{
		using (var control = new ExportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 4/9] Charges", control.GetLineChargesTabPageCaptionExposed());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JobComInvoiceLines.AddNew();
	}

	JobDeclaration declaration;

	class ExportInvoiceLineUserControlForTest : ExportInvoiceLineUserControl
	{
		public Type GetSupportingDocumentsUserControlTypeExposed() => base.GetSupportingDocumentsUserControlType();
		public Type GetPreviousDocumentsUserControlTypeExposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetOrganizationsUserControlTypeExposed() => base.GetOrganizationsUserControlType();
		public string GetPreviousDocumentsTabPageCaptionExposed() => base.GetPreviousDocumentsTabPageCaption(null).Caption;
		public string GetSupportingDocumentsTabPageCaptionExposed() => base.GetSupportingDocumentsTabPageCaption(null).Caption;
		public string GetPackagesTabPageCaptionExposed() => base.GetPackagesTabPageCaption(null).Caption;
		public string GetLineChargesTabPageCaptionExposed() => base.GetLineChargesTabPageCaption(null).Caption;
	}
}
