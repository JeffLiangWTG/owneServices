using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class InvoiceStructureChangedAnnouncerTest : TestCaseWithFactory
	{
		public void TestClearDirtyStatus()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			using (InvoiceStructureChangedAnnouncer announcer = new InvoiceStructureChangedAnnouncer(declaration))
			{
				AssertEquals("No change yet", false, announcer.IsDirty);

				declaration.Invoices.AddNew();

				AssertEquals("should be dirty now", true, announcer.IsDirty);

				announcer.ClearDirtyStatus();
				AssertEquals("cleared", false, announcer.IsDirty);
			}
		}

		public void TestMarkAsDirtyWhenChildGroupIsEntered()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();

			using (InvoiceStructureChangedAnnouncer announcer = new InvoiceStructureChangedAnnouncer(declaration))
			{
				AssertEquals(false, announcer.IsDirty);

				declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
				AssertEquals(true, announcer.IsDirty);
			}
		}

		public void TestMarkAsDirtyWhenInvoiceJZ_JZ_GroupInvoiceFKChanged()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader groupInvoice = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (InvoiceStructureChangedAnnouncer announcer = new InvoiceStructureChangedAnnouncer(declaration))
			{
				AssertEquals(false, announcer.IsDirty);

				invoice.JZ_JZ_GroupInvoiceFK = groupInvoice.PK;
				AssertEquals(true, announcer.IsDirty);
			}
		}

		public void TestMarkAsDirtyWhenAnInvoiceOrGroupIsDeleted()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseJobComInvoiceGroupHeader groupInvoice = declaration.TopGroupInvoice.JobComInvoiceGroupHeaders.AddNew();
			BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();

			using (InvoiceStructureChangedAnnouncer announcer = new InvoiceStructureChangedAnnouncer(declaration))
			{
				AssertEquals(false, announcer.IsDirty);

				invoice.Delete();
				AssertEquals(true, announcer.IsDirty);

				announcer.ClearDirtyStatus();
				AssertEquals(false, announcer.IsDirty);

				groupInvoice.Delete();
				AssertEquals(true, announcer.IsDirty);
			}
		}
	}
}
