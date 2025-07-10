using System;
using Enterprise.Accounting.Business;
using Enterprise.Customs.US.ISF.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Accounting.DocRollUpSort;
using Enterprise.DocumentWrappers.Testing.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.DocumentWrappers.Test.DocumentWrappers
{
	[TestedType(typeof(DocARInvoiceLineForRollUp))]
	sealed class USDocARInvoiceLineForRollUpTest : AccountingDocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocARInvoiceLineForRollUp.New(Factory) };
		}

		public void TestOperationsJob()
		{
			var objectCreator = new TestObjectCreator(Factory);
			var chargeCodePK = objectCreator.CC1.PK;
			DocARInvoiceLineForRollUp invoiceLineWrapper = null;
			Action<IJobInvoicingPlugIn> createNewJob = (IJobInvoicingPlugIn plugin) =>
			{
				objectCreator = new TestObjectCreator(Factory);
				JobHeader jobHeader = objectCreator.CreateJob(plugin, false);
				invoiceLineWrapper = DocARInvoiceLineForRollUp.New(Factory);
				invoiceLineWrapper.JobHeader = DocJobHeader.New(jobHeader, Factory);
			};

			CusISFHeader isfHeader = Factory.New<CusISFHeader>();
			isfHeader.BF_JobReference = "4444";
			createNewJob(isfHeader);
			AssertEquals("Enterprise.Customs.US.DocumentWrappers.FreightWrapperFromISF", invoiceLineWrapper.OperationsJob.GetType().ToString());
			AssertEquals("4444", invoiceLineWrapper.OperationsJob.JobNumber);
		}
	}
}
