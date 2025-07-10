using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(CommercialInvoiceForm))]
	sealed class CommercialInvoiceFormTest : Customs.GUI.Testing.CommercialInvoiceFormAbstractTest
	{
		[ExpectNoExceptions]
		public void TestDefaultModeValidateHasNoException()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			var line = invoice.InvoiceLines.AddNew();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				line.RunPreSaveValidation();
			}
		}

		public void TestViewModeVisible()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (var form = new CommercialInvoiceForm(invoice))
			{
				Assert(!form.ACECertifiedACEViewModeMenuItem.Visible);
				Assert(!form.ACECertifiedACSViewModeMenuItem.Visible);
				Assert(!form.ACSViewModeMenuItem.Visible);
			}

			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var form2 = new CommercialInvoiceForm(invoice))
			{
				Assert(form2.ACECertifiedACEViewModeMenuItem.Visible);
				Assert(form2.ACECertifiedACSViewModeMenuItem.Visible);
				Assert(form2.ACSViewModeMenuItem.Visible);
			}
		}

		public void TestViewModeForNewInvoice()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			var line = invoice.InvoiceLines.AddNew();
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				var declaration = invoice.JobDeclaration;
				AssertEquals(declaration.JE_MessageType, Customs.Business.JobMessageTypeList.Codes.Import);
				int count1 = line.OGAAgencyRequirements.Count;
				Assert(count1 > 0);
				form.ACECertifiedACSViewModeMenuItem.PerformClick();
				int count2 = line.OGAAgencyRequirements.Count;
				Assert(count1 > count2);
			}
		}

		public void TestViewModeClick()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (var form = new CommercialInvoiceForm(invoice))
			{
				var declaration = invoice.JobDeclaration;
				form.ACSViewModeMenuItem.PerformClick();
				AssertEquals(JobApplicationCodeList.Codes.ACS, declaration.JE_ApplicationCode);
				AssertEquals(CargoReleaseTypeList.Codes.ACS, declaration.US_CargoReleaseType);
				form.ACECertifiedACSViewModeMenuItem.PerformClick();
				AssertEquals(JobApplicationCodeList.Codes.ACE, declaration.JE_ApplicationCode);
				AssertEquals(CargoReleaseTypeList.Codes.ACS, declaration.US_CargoReleaseType);
				form.ACECertifiedACEViewModeMenuItem.PerformClick();
				AssertEquals(JobApplicationCodeList.Codes.ACE, declaration.JE_ApplicationCode);
				AssertEquals(CargoReleaseTypeList.Codes.ACE, declaration.US_CargoReleaseType);
			}
		}

		public void TestViewModeClickACEToACSDataShouldNotDelete()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var declaration = invoice.JobDeclaration;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.US_FDAIndicator = "D";
			var fdaLIne = invoiceLine.ACE_FDALines.AddNew();
			fdaLIne.US_Remarks = "Test";
			using (var form = new CommercialInvoiceForm(invoice))
			{
				form.ACECertifiedACEViewModeMenuItem.PerformClick();
				AssertEquals(form.CurrentViewMode, ViewMode.ACECertifiedACE);
				form.ACSViewModeMenuItem.PerformClick();
				AssertEquals(form.CurrentViewMode, ViewMode.ACS);
				form.ACECertifiedACEViewModeMenuItem.PerformClick();
				AssertEquals(form.CurrentViewMode, ViewMode.ACECertifiedACE);
				Assert(invoiceLine.ACE_FDALines.Count > 0);
			}
		}

		public void TestInvoiceHeaderUserControl()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				AssertEquals(typeof(InvoiceHeaderUserControl), form.HeaderTabPage.Controls["InvoiceHeaderUserControl"].GetType());
			}
		}

		public void TestInvoiceLinesGridHasInvoiceColumnRemoved()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				Customs.GUI.BaseInvoiceLineUserControl invoiceLineUserControl = form.InvoiceLineUserControl;
				CheckColumnHasBeenRemoved(form.InvoiceLineUserControl, JobComInvoiceLine.Schema.JI_Calc_Invoice);
				CheckColumnHasBeenRemoved(form.InvoiceLineUserControl, JobComInvoiceLine.Schema.JI_Calc_MergedLineNumber);
				CheckColumnHasBeenRemoved(form.InvoiceLineUserControl, JobComInvoiceLine.Schema.JI_Calc_EntryNumber);
				CheckColumnHasBeenRemoved(form.InvoiceLineUserControl, JobComInvoiceLine.Schema.JI_Calc_XTN);
			}
		}

		public void TestAIIRelatedTabVisibility()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				AssertEquals("AII tabs should be invisible for export", false, form.AIITabPage.TabVisible);
				AssertEquals("Organisation tabs should be invisible for export", false, form.OrganisationTabPage.TabVisible);
				invoice.JZ_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("AII tabs should be visible for import", true, form.AIITabPage.TabVisible);
				AssertEquals("Organisation tabs should be visible for import", true, form.OrganisationTabPage.TabVisible);
				AssertEquals("Index of AII tab page", 2, form.MainTabControl.TabPages.IndexOf(form.AIITabPage));
				AssertEquals("Index of Organisation tab page", 3, form.MainTabControl.TabPages.IndexOf(form.OrganisationTabPage));
			}
		}

		public void TestDefaultingButtonsAreHidden()
		{
			JobComInvoiceHeader invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.AIITabPage;
				InvoiceHeaderAIIUserControl userControl = (InvoiceHeaderAIIUserControl)form.AIITabPage.Controls[0];
				userControl.AIIOtherDetailsTabControl.SelectedTab = userControl.RelatedDataTabPage;
				AssertEquals("DefaultRelatedDocumentsButton should not be visible on a commercial invoice form", false, userControl.DefaultRelatedDocumentsButton.Visible);
				AssertEquals("RelatedBillPanel should not be visible", false, userControl.RelatedBillPanel.Visible);
			}
		}

		public void TestNoExceptionThrownDuringSave_WI00134914()
		{
			var orgHeader = Factory.New<MasterFiles.Business.OrgHeader>();
			orgHeader.OH_Code = "INCORGTST";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "INCEXPPGA";
			var relOrg = product.RelatedOrganisations.AddNew();
			relOrg.OU_OH = orgHeader.PK;
			relOrg.OU_Relationship = MasterFiles.Business.OrgPartRelation.RelationshipTypes.Both;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CI_TariffNum = USCTariff.CottonFeeApplicable;
			pivot.CD_AMSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_ExportCertificateNo = "14244441321";
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportATF.US_FFLNumber = "TEST NUMBER";
			pivot.CD_DEAIndicator = OGAIndicatorList.Codes.Declared;
			var productDEA = pivot.DEAHeaders.AddNew();
			productDEA.US_DrugCode = "ABCD";
			pivot.CD_PSTIndicator = OGAIndicatorList.Codes.Declared;
			pivot.CD_EPAConsentNumber = "123132131";
			pivot.CD_FWSIndicator = OGAIndicatorList.Codes.Declared;
			pivot.ExportFWS.US_ConfirmationNum = "123156GF123";
			pivot.CD_NMFSHMSIndicator = OGAIndicatorList.Codes.Declared;
			var productNMFS = pivot.NMFSLines.AddNew();
			productNMFS.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			pivot.CD_TTBIndicator = OGAIndicatorList.Codes.Disclaimed;
			var productTTB = pivot.TTBLines.AddNew();
			productTTB.US_NumberForIRC = "TP-OH-77777";
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_OH_Buyer = orgHeader.PK;
			invoice.JZ_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			invoice.US_TariffType = TariffTypeList.Codes.ScheduleB;
			var invoiceLine = invoice.InvoiceLines.AddNew();
			Factory.Save();
			invoiceLine.JI_PartNo = "INCEXPPGA";
			using (CommercialInvoiceForm form = new CommercialInvoiceForm(invoice))
			{
				form.Show();
				form.MainTabControl.SelectedTab = form.LinesTabPage;
				var invoiceLineUserControl = (USExportInvoiceLineUserControl)form.InvoiceLineUserControl;
				foreach (Control userControl in invoiceLineUserControl.LineDetailTabControl.Controls)
				{
					if (userControl is Customs.GUI.BaseDeclarationTabPage)
					{
						invoiceLineUserControl.LineDetailTabControl.SelectedTab = userControl as Customs.GUI.BaseDeclarationTabPage;
					}
				}

				AssertNoExceptionThrown(() =>
				{
					form.FireSaveButton();
				});
			}
		}

		protected override Customs.GUI.CommercialInvoiceForm GetNewCommercialInvoiceForm()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			Factory.Save();
			var form = new CommercialInvoiceForm(invoice);
			form.LinesTabPage.TabVisible = false;
			return form;
		}

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => control.Name == "USDCurrencyTextBox";
	}
}
