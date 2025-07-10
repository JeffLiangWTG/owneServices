using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.GUI;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

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

	public void TestValueIndicatorsTab()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();
			var valueIndicators = control.FindSingleOrDefault<ZTabPage>("ValueIndicatorsTabPage");
			CombineAssertions(() =>
			{
				AssertNotNull(valueIndicators);
				AssertEquals("ValueIndicatorsTabPage", true, valueIndicators.TabVisible);
			});
			valueIndicators.Show();

			var tabPartyRelationShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabPartyRelationShipCheckBox");
			CombineAssertions(() =>
			{
				AssertNotNull(tabPartyRelationShipCheckBox);
				AssertEquals("TabPartyRelationShipCheckBox", true, tabPartyRelationShipCheckBox.Visible);
			});

			var tabRestrictionsShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabRestrictionsShipCheckBox");
			CombineAssertions(() =>
			{
				AssertNotNull(tabRestrictionsShipCheckBox);
				AssertEquals("TabRestrictionsShipCheckBox", true, tabRestrictionsShipCheckBox.Visible);
			});

			var tabSaleConditionsShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabSaleConditionsShipCheckBox");
			CombineAssertions(() =>
			{
				AssertNotNull(tabSaleConditionsShipCheckBox);
				AssertEquals("TabSaleConditionsShipCheckBox", true, tabSaleConditionsShipCheckBox.Visible);
			});

			var tabDisposalAccrualShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabDisposalAccrualShipCheckBox");
			CombineAssertions(() =>
			{
				AssertNotNull(tabDisposalAccrualShipCheckBox);
				AssertEquals("TabDisposalAccrualShipCheckBox", true, tabDisposalAccrualShipCheckBox.Visible);
			});
		}
	}

	public void TestValueIndicatorsTabValues()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Import;
		var invoice = declaration.Invoices.AddNew();
		var invoiceLine1 = invoice.InvoiceLines.AddNew();
		invoiceLine1.RelatedIndicator = true;
		invoiceLine1.RelatedIndicator2 = true;
		invoiceLine1.RelatedIndicator3 = false;
		invoiceLine1.RelatedIndicator4 = false;
		var invoiceLine2 = invoice.InvoiceLines.AddNew();
		invoiceLine2.RelatedIndicator = false;
		invoiceLine2.RelatedIndicator2 = false;
		invoiceLine2.RelatedIndicator3 = true;
		invoiceLine2.RelatedIndicator4 = true;

		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();
			var valueIndicators = control.FindSingleOrDefault<ZTabPage>("ValueIndicatorsTabPage");
			valueIndicators.Show();
			var tabPartyRelationShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabPartyRelationShipCheckBox");
			AssertEquals("For the first invoice line, the TabPartyRelationShipCheckBox should be checked", true, tabPartyRelationShipCheckBox.Checked);
			var tabRestrictionsShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabRestrictionsShipCheckBox");
			AssertEquals("For the first invoice line, the TabRestrictionsShipCheckBox should be checked", true, tabRestrictionsShipCheckBox.Checked);
			var tabSaleConditionsShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabSaleConditionsShipCheckBox");
			AssertEquals("For the first invoice line, the TabSaleConditionsShipCheckBox should not be checked", false, tabSaleConditionsShipCheckBox.Checked);
			var tabDisposalAccrualShipCheckBox = valueIndicators.FindSingleOrDefault<ZCheckBox>("TabDisposalAccrualShipCheckBox");
			AssertEquals("For the first invoice line, the TabDisposalAccrualShipCheckBox should not be checked", false, tabDisposalAccrualShipCheckBox.Checked);

			invoiceLine1.Delete();

			AssertEquals("For the second invoice line, the TabPartyRelationShipCheckBox should not be checked", false, tabPartyRelationShipCheckBox.Checked);
			AssertEquals("For the second invoice line, the TabRestrictionsShipCheckBox should not be checked", false, tabRestrictionsShipCheckBox.Checked);
			AssertEquals("For the second invoice line, the TabSaleConditionsShipCheckBox should be checked", true, tabSaleConditionsShipCheckBox.Checked);
			AssertEquals("For the second invoice line, the TabDisposalAccrualShipCheckBox should be checked", true, tabDisposalAccrualShipCheckBox.Checked);
		}
	}

	public void TestJI_AdditionalSupplementsTextBoxVisible()
	{
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();
			var additionalProcedureCodesTextBox = control.FindSingle<AdditionalSupplementaryCodesAndGDMUserControl>("AdditionalSupplementaryCodesAndGDMUserControl");

			Assert("Additional procedures text box visible", additionalProcedureCodesTextBox.Visible);
		}
	}

	public void TestZG_TransNatureVisibility()
	{
		using (var form = new JobDeclarationForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();
			form.Controls.Add(control);
			form.Show();

			var transactionNatureDropEdit = control.FindSingle<ZDropEdit>("TransactionNatureDropEdit");
			Assert("[24] Tran. Nature should be visible.", transactionNatureDropEdit.Visible);
		}
	}

	public void TestTransNatureInGrid()
	{
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			control.InitializeGridLayout();

			var lineGrid = control.CustomsInvoiceLinesBoundGrid;
			var styles = lineGrid.ColumnStyles;

			AssertEquals(22, styles.IndexOf(lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_TransNature)));

			AssertEquals("ZG_TransNature should be visible in the grid.", true, lineGrid.GetColumnStyle(JobComInvoiceLine.Schema.ZG_TransNature).IsVisible);
		}
	}

	public void TestCountryOfDestinationVisible()
	{
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			control.JobDeclaration = declaration;
			form.Controls.Add(control);
			form.Show();
			var destinationCodeFindBox = control.FindSingle<ZCodeFindBox>("DestinationUsingZZRefCusCodeListCodeFindBox");

			Assert("Country Of Destination FindBox", destinationCodeFindBox.Visible);
		}
	}

	public void TestGetSupportingDocumentsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(NLSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlTypeExposed());
		}
	}

	public void TestAdditionalDocumentsControlVisibility()
	{
		using (var form = new ZForm())
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var additionalInfoTabPage = control.FindSingle<ZTabPage>("AdditionalInfosTabPage");
			control.LineDetailTabControl.SelectedTab = additionalInfoTabPage;

			var additionalDocumentsUserControl = additionalInfoTabPage.FindSingle<ZDynamicControlCreationUserControl>("additionalInfosUserControl1");
			AssertNotNull("InvoiceLineAdditionalInfosUserControlWithGrid", additionalDocumentsUserControl);
			Assert("InvoiceLineAdditionalInfosUserControlWithGrid must be visible", additionalDocumentsUserControl.Visible);
		}
	}

	public void TestAdditionalInfosTabPageCaptionAndUserControlType()
	{
		EUDynamicControlTestHelper.AssertEUInvoiceLineUserControlTabPageCaptionAndUserControlType<ImportInvoiceLineUserControl>(declaration, "AdditionalInfosTabPage", "additionalInfosUserControl1", "Additional Documents", typeof(InvoiceLineAdditionalInfosUserControlWithGrid));
	}

	public void TestTabPagesOrder()
	{
		using (var form = new ZForm(declaration))
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.SetDataBinding(declaration, ZString.Empty);
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();
			var tabPages = control.LineDetailTabControl.TabPages;
			AssertArrayEqualsByElements(new[] { "NewLineDetailsTabPage", "OrganizationsTabPage", "LineChargesTabPage", "SupportingDocumentsTabPage" , "AdditionalInfosTabPage",
				"PreviousDocumentsTabPage", "PackagesPivotTabPage", "ValueIndicatorsTabPage", "AuthorisationsTabPage", "CustomFieldsTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
		}
	}

	public void TestGetPreviousDocumentsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(NLPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
		}
	}

	public void TestGetOrganizationsUserControlType()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals(typeof(ImportInvoiceLineOrganizationsUserControl), control.GetOrganizationsUserControlTypeExposed());
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
		using (var control = new ImportInvoiceLineUserControl())
		{
			form.Controls.Add(control);
			control.JobDeclaration = declaration;
			form.Show();

			AssertEquals("When Valuation check box is checked, the valuation indicators tab page should be visible", true, control.LineDetailTabControl.FindSingle<ZTabPage>("ValueIndicatorsTabPage").TabVisible);
		}
	}

	public void TestGetPreviousDocumentsTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/1] Previous Documents", control.GetPreviousDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetSupportingDocumentsTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 2/3] Supporting Documents", control.GetSupportingDocumentsTabPageCaptionExposed());
		}
	}

	public void TestGetPackagesTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("Packages", control.GetPackagesTabPageCaptionExposed());
		}
	}

	public void TestGetLineChargesTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 4/9] Charges", control.GetLineChargesTabPageCaptionExposed());
		}
	}

	public void TestGetValueIndicatorsTabPageCaption()
	{
		using (var control = new ImportInvoiceLineUserControlForTest())
		{
			AssertEquals("[UCC 4/13] Value Indicators", control.GetValueIndicatorsTabPageCaptionExposed());
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
		var invoiceHeader = declaration.Invoices.AddNew();
		invoiceHeader.JobComInvoiceLines.AddNew();
	}
	JobDeclaration declaration;

	class ImportInvoiceLineUserControlForTest : ImportInvoiceLineUserControl
	{
		public Type GetSupportingDocumentsUserControlTypeExposed() => base.GetSupportingDocumentsUserControlType();
		public Type GetPreviousDocumentsUserControlTypeExposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetOrganizationsUserControlTypeExposed() => base.GetOrganizationsUserControlType();
		public string GetPreviousDocumentsTabPageCaptionExposed() => base.GetPreviousDocumentsTabPageCaption(null).Caption;
		public string GetSupportingDocumentsTabPageCaptionExposed() => base.GetSupportingDocumentsTabPageCaption(null).Caption;
		public string GetPackagesTabPageCaptionExposed() => base.GetPackagesTabPageCaption(null).Caption;
		public string GetLineChargesTabPageCaptionExposed() => base.GetLineChargesTabPageCaption(null).Caption;
		public string GetValueIndicatorsTabPageCaptionExposed() => base.GetValueIndicatorsTabPageCaption(null).Caption;
	}
}
