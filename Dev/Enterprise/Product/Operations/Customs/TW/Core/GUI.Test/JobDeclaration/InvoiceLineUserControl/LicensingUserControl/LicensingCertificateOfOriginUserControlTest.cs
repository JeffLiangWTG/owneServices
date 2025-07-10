using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class LicensingCertificateOfOriginUserControlTest : TestCaseWithFactory
	{
		public void TestCalcEditProperties()
		{
			using (var form = new JobDeclarationForm(jobDeclaration))
			{
				form.Show();
				Application.DoEvents();
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.DeclarationTabPage;
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = form.CustomsBrokerageUserControl.InvoiceLinesTabPage;
				var invoiceLineUserControl = (ImportInvoiceLineUserControl)form.CustomsBrokerageUserControl.InvoiceLinesUserControl;
				invoiceLineUserControl.LineDetailTabControl.SelectedTab = invoiceLineUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "LicensingTabPage");
				var licensingUserControl = invoiceLineUserControl.FindSingleOrDefault<LicensingUserControl>(c => c.Name == "LicensingUserControl");
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CertificateOfOriginTabPage");
				var control = licensingUserControl.FindSingleOrDefault<LicensingCertificateOfOriginUserControl>(c => c.Name == "LicensingCertificateOfOriginUserControl");
				var bingdingSource = control.BindingSource;
				var shippingMarksLongTextControl = control.FindSingle<Customs.GUI.LongTextControl>(c => c.Name == "ShippingMarksLongTextControl");
				AssertEquals("BindingMember", "FilteredInvoiceLines.NX101ShippingMarks", bingdingSource.GetBindingMember(shippingMarksLongTextControl));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_TransportMode = Customs.Business.TransportTypeList.Codes.Sea;
			var entryInstruction = jobDeclaration.CusEntryInstruction;
			invoiceLine = jobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction.PK;
			var cmHeader = entryInstruction.ControllingMessageHeaders.AddNew();
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First().IsLinkedCMHeader = true;
			cmHeader.TW1_ControllingMessageType = "NX101";
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
	}
}
