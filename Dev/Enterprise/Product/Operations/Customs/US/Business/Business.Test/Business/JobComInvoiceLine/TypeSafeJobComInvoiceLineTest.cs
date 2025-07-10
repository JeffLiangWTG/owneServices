using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TypeSafeJobComInvoiceLineTest : TestCaseWithFactory
	{
		public void TestApportionChargeCollection()
		{
			AssertEquals(typeof(InvoiceLineApportionChargeCollection), invoiceLine.ApportionedCharges.GetType());
		}

		public void TestCusEntryLine()
		{
			CusEntryHeader entry = declaration.CustomsEntryHeaders.AddNew();
			CusEntryLine entryline = entry.MergedLines.AddNew();
			invoiceLine.JI_CL = entryline.PK;
			AssertEquals(typeof(CusEntryLine), invoiceLine.CusEntryLine.GetType());
		}

		public void TestPackageDetails()
		{
			var reconDec = Factory.New<JobDeclaration>();
			var reconDecWrapper = new ReconDeclaration(reconDec);
			var originalEntry = reconDecWrapper.OriginalEntries.AddNew();
			var invoice = originalEntry.Invoice;
			invoice.InvoiceLines.AddNew();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var reconDecLoaded = factory2.Load<JobDeclaration>(reconDec.PK);
			var reconDecLoadedWrapper = new ReconDeclaration(reconDecLoaded);
			reconDecLoadedWrapper.LoadChildEditableObjects();
			reconDecLoadedWrapper.RunPreSaveValidation();

			AssertEquals(0, factory2.GetTableHitCount(ZArchitecture.Schema.CusInvPackSchema.Constants.TableName));
		}

		public void TestValidation()
		{
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			declaration.US_EnableINB = true;
			AssertEquals(typeof(FormalImportJobComInvoiceLineValidation), invoiceLine.Validation.GetType());

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(typeof(ExportJobComInvoiceLineValidation), invoiceLine.Validation.GetType());
		}

		JobDeclaration declaration;
		JobComInvoiceHeader invoice;
		JobComInvoiceLine invoiceLine;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
		}
	}
}
