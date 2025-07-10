using CargoWise.Application;
using CargoWise.Schema;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Test
{
	[TestedType(typeof(DocARInvoiceLine))]
	sealed class USDocARInvoiceLineTest : AccountingDocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoiceLine.New(line, Factory) };
		}

		public void TestISFHeader()
		{
			CusISFHeader iSFHeader = Factory.New<CusISFHeader>();
			JobHeader job = GetInvoiceJob(iSFHeader, invoice);
			line.AL_JH = job.PK;
			var invoiceLineWrapper = DocARInvoiceLine.New(line, Factory);
			AssertNotNull(invoiceLineWrapper.ISFHeader);
		}

		public void TestISFInvoiceLineType()
		{
			CusISFHeader header = Factory.New<CusISFHeader>();
			header.BF_EntryNumber = "1234";
			invoice.AH_ConsolidatedInvoiceRef = header.BF_EntryNumber;
			JobHeader job = GetInvoiceJob(header, invoice);
			line.AL_JH = job.PK;
			var invoiceLineWrapper = DocARInvoiceLine.New(line, Factory);
			Assert("Invoice Type", invoiceLineWrapper.IsISFJob);
		}

		ARInvoice invoice;
		ARInvoiceLine line;

		protected override void SetUp()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);

			invoice = Factory.New<ARInvoice>();
			line = (ARInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			base.SetUp();
		}

		JobHeader GetInvoiceJob(IJobInvoicingPlugIn plugIn, InvoicingBase invoice)
		{
			var job = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			job.JH_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(plugIn.TableName);
			job.JH_ParentID = plugIn.PK;
			invoice.AH_JH = job.PK;
			return job;
		}
	}
}
