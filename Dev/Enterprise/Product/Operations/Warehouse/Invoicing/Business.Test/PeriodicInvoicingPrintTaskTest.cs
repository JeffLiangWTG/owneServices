using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;

namespace Enterprise.Warehouse.Invoicing.Business.Test
{
	public class PeriodicInvoicingPrintTaskTest : TestCaseWithFactory
	{
		public void TestPrintTaskIncludesAllDocuments()
		{
			TestPrintTaskIncludesAllDocumentsCore(setFactory: false);
		}

		public void TestPrintTaskIncludesAllDocuments_SameInvoiceFactory()
		{
			TestPrintTaskIncludesAllDocumentsCore(setFactory: true);
		}

		void TestPrintTaskIncludesAllDocumentsCore(bool setFactory)
		{
			var warehouse = Factory.NewWithValidTestData<WhsWarehouse>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var invoicing = Factory.NewWithValidTestData<PeriodicInvoicing>();
			invoicing.ET_WW = warehouse.PK;
			invoicing.ET_OH_Client = org.PK;
			invoicing.AutoRateJobHeader(null);
			invoicing.PostInvoice();

			var invoice = Factory.NewWithValidTestData<ARInvoice>();
			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_JH = invoicing.JobHeader.PK;

			if (!setFactory)
			{
				Factory.Save();
			}
			invoice.AH_Desc = "Jusrt some change";

			var factory = setFactory ? invoice.Factory : null;
			using (var task = new PeriodicInvoicingPrintTaskForTest(new InvoicingBase[] { invoice }, factory))
			{
				AssertEquals("Precondition", setFactory, factory == invoice.Factory);
				AssertEquals("Precondition", true, invoice.HasChanges);
				task.Run();
				AssertEquals(1, task.TaskCount);
				AssertEquals(2, task.LastPack.Count);
				AssertEquals("Invoice", task.LastPack[0].Name);
				AssertEquals("Invoice Detail", task.LastPack[1].Name);
				AssertEquals("Should not call save", true, invoice.HasChanges);
			}

			DeleteJobHeadersWithoutAnyCharges(invoicing);
		}

		void DeleteJobHeadersWithoutAnyCharges(PeriodicInvoicing invoice)
		{
			if (invoice.JobHeader != null && !invoice.JobHeader.IsInDatabase && !(invoice.JobHeader.Charges.Count > 0))
			{
				invoice.JobHeader.Delete();
			}
		}
	}

	public class PeriodicInvoicingPrintTaskForTest : PeriodicInvoicingPrintTask
	{
		public PeriodicInvoicingPrintTaskForTest(InvoicingBase[] invoices, BusinessObjectFactory factory)
			: base(invoices, factory)
		{
		}

		protected override void AddExtraDocumentsToPack(InvoicingBase invoice, DocumentPack pack)
		{
			base.AddExtraDocumentsToPack(invoice, pack);
			LastPack = pack;
		}

		internal DocumentPack LastPack;
	}
}
