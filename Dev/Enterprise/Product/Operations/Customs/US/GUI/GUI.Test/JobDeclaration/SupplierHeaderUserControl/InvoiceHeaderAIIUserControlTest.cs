using System;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(InvoiceHeaderAIIUserControl))]
	sealed class InvoiceHeaderAIIUserControlTest : ImportCustomsUserControlBasherAbstractTest
	{
		public void TestTermsOfDeliveryLocationVisibility()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			JobComInvoiceHeader invoice = declaration.FilteredInvoices.AddNew();

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl invoiceControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				invoiceControl.InvoiceTabControl.SelectedTab = invoiceControl.ElectronicInvoiceTabPage;

				invoiceControl.InvoiceTabControl.SelectedTab = invoiceControl.ElectronicInvoiceTabPage;
				invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.Other;

				InvoiceHeaderAIIUserControl aiiUserControl = (InvoiceHeaderAIIUserControl)invoiceControl.ElectronicInvoiceTabPage.Controls[0];

				AssertEquals(true, aiiUserControl.TermsOfDeliveryLocationTextBox.Visible);
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationCountryCodeFindBox.Visible);
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationScheduleDCodeFindBox.Visible);

				invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleD;
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationTextBox.Visible);
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationCountryCodeFindBox.Visible);
				AssertEquals(true, aiiUserControl.TermsOfDeliveryLocationScheduleDCodeFindBox.Visible);
				AssertEquals(false, aiiUserControl.US_TermsOfDeliveryLocationScheduleKCodeFindBox.Visible);

				invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ISOCountryCode;
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationTextBox.Visible);
				AssertEquals(true, aiiUserControl.TermsOfDeliveryLocationCountryCodeFindBox.Visible);
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationScheduleDCodeFindBox.Visible);
				AssertEquals(false, aiiUserControl.US_TermsOfDeliveryLocationScheduleKCodeFindBox.Visible);

				invoice.US_TermsOfDeliveryLocationIndicator = TermsOfDeliveryLocationCodeIndicators.Codes.ScheduleK;
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationTextBox.Visible);
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationCountryCodeFindBox.Visible);
				AssertEquals(false, aiiUserControl.TermsOfDeliveryLocationScheduleDCodeFindBox.Visible);
				AssertEquals(true, aiiUserControl.US_TermsOfDeliveryLocationScheduleKCodeFindBox.Visible);
			}
		}

		public void TestDefaultRelatedDocumentsButtonClick()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.US_EnableAII = true;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_MasterBill = "M081232123";
			declaration.JE_HouseBill = "H023223434";

			using (JobDeclarationForm form = new JobDeclarationForm(declaration))
			{
				form.Show();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoicesTabPage;
				USImportSupplierHeaderUserControl userControl = (USImportSupplierHeaderUserControl)form.CustomsBrokerageUserControl.SupplierHeaderUserControl;
				userControl.InvoiceTabControl.SelectedTab = userControl.ElectronicInvoiceTabPage;

				InvoiceHeaderAIIUserControl aiiUserControl = (InvoiceHeaderAIIUserControl)userControl.ElectronicInvoiceTabPage.Controls[0];

				aiiUserControl.AIIOtherDetailsTabControl.SelectedTab = aiiUserControl.RelatedDataTabPage;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				aiiUserControl.DefaultRelatedDocumentsButton.PerformClick();
				AssertEquals("Please select an invoice header.", UnitTestUserNotification.Instance.LastMessage.Text);

				JobComInvoiceHeader invoice = declaration.FilteredInvoices.AddNew();
				JobComInvoiceHeader invoice2 = declaration.FilteredInvoices.AddNew();
				AssertEquals(0, invoice.RelatedDocuments.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				userControl.InvoiceHeadersBoundGrid.ListManager.Position = 0;
				aiiUserControl.DefaultRelatedDocumentsButton.PerformClick();
				AssertEquals("Related Documents", 2, invoice.RelatedDocuments.Count);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Type UserControlToBashType => typeof(InvoiceHeaderAIIUserControl);
	}
}
