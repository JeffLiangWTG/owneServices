using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(LicensingUserControl))]
	sealed class LicensingUserControlTest : TestCaseWithFactory
	{
		public void TestTabVisible()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = jobDeclartion.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using (var form = new JobDeclarationForm(jobDeclartion))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				using (var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl)
				{
					invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "LicensingTabPage");
					var licensingUserControl = invoiceLineUserControl.FindSingleOrDefault<LicensingUserControl>(c => c.Name == "LicensingUserControl");
					CombineAssertions(() =>
					{
						AssertNotNull("MessagesTabPage should be visible", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "MessagesTabPage"));
						AssertNull("CommonTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CommonTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("FoodAndDrugTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AlcoholTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("TypeApprovalTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});
					
					var invoiceLineLinkControllingMsgHeaders = line.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().FirstOrDefault();
					invoiceLineLinkControllingMsgHeaders.IsLinkedCMHeader = true;
					CombineAssertions(() =>
					{
						AssertNotNull("MessagesTabPage should be visible", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "MessagesTabPage"));
						AssertNotNull("CommonTabPage should be visible", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CommonTabPage"));
						AssertNull("FoodAndDrugTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("AlcoholTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("TypeApprovalTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});

					controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
					CombineAssertions(() =>
					{
						AssertNull("FoodAndDrugTabPage should be hide when NX101", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide when NX101", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("AlcoholTabPage should be hide when NX101", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("TypeApprovalTabPage should be hide when NX101", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNotNull("CertificateOfOriginTabPage should be visible when NX101", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});
					
					controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301_DN;
					CombineAssertions(() =>
					{
						AssertNull("FoodAndDrugTabPage should be hide when NX301_DN", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide when NX301_DN", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNotNull("AlcoholTabPage should be visible when NX301_DN", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide when NX301_DN", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});

					controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX301;
					CombineAssertions(() =>
					{
						AssertNull("FoodAndDrugTabPage should be hide when NX301", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide when NX301", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("AlcoholTabPage should be hide when NX301", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNotNull("TypeApprovalTabPage should be visible when NX301", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide when NX301", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});
					
					controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX401;
					CombineAssertions(() =>
					{
						AssertNull("FoodAndDrugTabPage should be hide when NX401", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNotNull("AnimalAndPlantTabPage should be visible when NX401", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("AlcoholTabPage should be hide when NX401", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("TypeApprovalTabPage should be hide when NX401", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide when NX401", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});
					
					controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX601;
					CombineAssertions(() =>
					{
						AssertNotNull("FoodAndDrugTabPage should be visible when NX601", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide when NX601", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("AlcoholTabPage should be hide when NX601", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("TypeApprovalTabPage should be hide when NX601", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide when NX601", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});

					controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX603;
					CombineAssertions(() =>
					{
						AssertNotNull("FoodAndDrugTabPage should be visible when NX603", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "FoodAndDrugTabPage"));
						AssertNull("AnimalAndPlantTabPage should be hide when NX603", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AnimalAndPlantTabPage"));
						AssertNull("AlcoholTabPage should be hide when NX603", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "AlcoholTabPage"));
						AssertNull("TypeApprovalTabPage should be hide when NX603", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "TypeApprovalTabPage"));
						AssertNull("CertificateOfOriginTabPage should be hide when NX603", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
					});
				}
			}
		}

		public void TestCertificateOfOriginTabPageVisible()
		{
			var jobDeclartion = Factory.NewWithValidTestData<JobDeclaration>();
			var controllingMessageHeader = jobDeclartion.CusEntryInstruction.ControllingMessageHeaders.AddNew();
			controllingMessageHeader.TW1_ControllingMessageType = ControllingMessageTypeList.Codes.NX101;
			var header = jobDeclartion.Invoices.AddNew();
			var line = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			using var form = new JobDeclarationForm(jobDeclartion);
			form.Show();
			Application.DoEvents();
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
			form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
			using var invoiceLineUserControl = (BaseInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
			invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "LicensingTabPage");
			var licensingUserControl = invoiceLineUserControl.FindSingleOrDefault<LicensingUserControl>(c => c.Name == "LicensingUserControl");
			AssertNull("CertificateOfOriginTabPage should be hide", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));

			var controllingMessageHeaderLinkInvoiceLine = controllingMessageHeader.ControllingMessageHeaderLinkInvoiceLines.Cast<ControllingMessageHeaderLinkInvoiceLine>().First();
			controllingMessageHeaderLinkInvoiceLine.Link = true;
			AssertNotNull("CertificateOfOriginTabPage should be visible when NX101", licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage"));
		}
	}
}
