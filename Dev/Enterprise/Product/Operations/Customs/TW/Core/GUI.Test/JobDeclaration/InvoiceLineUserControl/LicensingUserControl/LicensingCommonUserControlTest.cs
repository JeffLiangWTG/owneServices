using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Customs.TW.Business.Testing.InvoiceLineLinkControllingMsgHeaderCollectionTest;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class LicensingCommonUserControlTest : TestCaseWithFactory
	{
		public void TestBindingMembers()
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
				licensingUserControl.LicensingTabControl.SelectedTab = licensingUserControl.FindSingleOrDefault<ZTabPage>(c => c.Name == "CommonTabPage");
				var licensingCommonUserControl = licensingUserControl.FindSingleOrDefault<LicensingCommonUserControl>(c => c.Name == "LicensingCommonUserControl");
				var bingdingSource = licensingCommonUserControl.BindingSource;
				var innerPackDescriptionLongTextControl = licensingCommonUserControl.FindSingle<ZTextBox>(c => c.Name == "JI_InnerPackDescriptionTextBox");
				AssertEquals("BindingMember", "FilteredInvoiceLines.JI_InnerPackDescription", bingdingSource.GetBindingMember(innerPackDescriptionLongTextControl));
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
			ControllingMsgHeaderTestHelper.AddControllingAgencysTo(entryInstruction.ControllingMessageHeaders, new string[] { "XX", "20", "IF", "DN", "CD" });
			invoiceLine.InvoiceLineLinkControllingMsgHeaders.Cast<InvoiceLineLinkControllingMsgHeader>().First(x => x.ControllingAgency == "XX").IsLinkedCMHeader = true;
		}

		JobDeclaration jobDeclaration;
		JobComInvoiceLine invoiceLine;
	}
}
