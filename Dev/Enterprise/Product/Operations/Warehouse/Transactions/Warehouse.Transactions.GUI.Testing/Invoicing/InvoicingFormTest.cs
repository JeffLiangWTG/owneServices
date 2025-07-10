using System.Windows.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class InvoicingFormTest : WhsGuiTestCaseWithFactory
	{
		public void TestConstructor()
		{
			var invoice = Factory.New<WhsInvoice>();
			using (InvoicingForm form = new InvoicingForm(invoice))
			{
				AssertEquals("Posting not setup", true, form.SetupPostingCalledForTest);
				AssertNotNull("Job Invoicing not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull("EDocs not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
			}
		}

		#region TestEDocsModifySetToOffNotDisablePlugin

		public void TestEDocsModifySetToOffNotDisablePlugin()
		{
			Env.Security.eDocs.IsAllowed = true;
			Env.Security.eDocsModify.IsAllowed = false;

			var invoice = Factory.New<WhsInvoice>();
			using (var form = new InvoicingForm(invoice))
			{
				form.Show();
				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert("Plugin should not be disabled.", eDocPlugin.SecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		public void TestJobHeaderIsNull()
		{
			var invoice = Factory.New<WhsInvoice>();
			var mutex = JobHeader.GetMutex_ForTestOnly(invoice.PK);
			mutex.Lock();
			try
			{
				using (var form = new InvoicingForm(invoice))
				{
					form.Show();
					AssertNoExceptionThrown(() => Application.DoEvents());
				}
			}
			finally
			{
				mutex.Unlock();
			}
		}

		public void TestInvoice()
		{
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			using (InvoicingForm form = new InvoicingForm(invoice))
			{
				AssertEquals(invoice, form.InvoiceForTest);
			}
		}

		public void TestFormCaption()
		{
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			using (InvoicingForm form = new InvoicingForm(invoice))
			{
				AssertEquals("Periodic Invoice", form.FormCaption.Trim());
				invoice.ET_StorageJobNumber = "W00000001";
				AssertEquals("Periodic Invoice W00000001", form.FormCaption);
			}
		}

		public void TestMainTabRemoved()
		{
			WhsInvoice invoice = Factory.New<WhsInvoice>();
			using (InvoicingForm form = new InvoicingForm(invoice))
			{
				form.Show();
				UserIdleWorker.Flush();
				AssertEquals("Billing", form.TopLevelTabControlForTest.TabPages[0].Text);
			}
		}

		public void TestClientChangeEventHooked()
		{
			OrgHeader org1 = Factory.NewWithValidTestData<OrgHeader>();
			OrgHeader org2 = Factory.NewWithValidTestData<OrgHeader>();
			WhsInvoice invoice = Factory.New<WhsInvoice>();

			using (InvoicingForm form = new InvoicingForm(invoice))
			{
				form.Show();
				Application.DoEvents();

				invoice.JobHeader.LocalChargesPK = org1.PK;
				AssertEquals(org1.PK, invoice.ET_OH_Client);

				invoice.JobHeader.LocalChargesPK = org2.PK;
				AssertEquals(org2.PK, invoice.ET_OH_Client);
			}
		}
	}
}
