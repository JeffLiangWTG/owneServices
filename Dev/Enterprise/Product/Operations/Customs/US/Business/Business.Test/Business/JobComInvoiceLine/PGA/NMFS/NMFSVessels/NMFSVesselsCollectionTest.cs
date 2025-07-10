using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(NMFSVesselsCollection))]
	public class NMFSVesselsCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowAddNew()
		{
			var collection = NMFSDetail.HarvestingVessles;
			AssertEquals(typeof(NMFSVesselsCollection), collection.GetType());
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes._370;
			AssertEquals(false, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.AMR;
			AssertEquals(false, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.HMS;
			AssertEquals(false, collection.AllowNew);
			NMFSLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
			NMFSLine.US_SourceType = "HBA";
			AssertEquals(false, collection.AllowNew);
			NMFSLine.US_SourceType = "HCF";
			AssertEquals(true, collection.AllowNew);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new NMFSVesselsCollection(NMFSDetail);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		JobComInvoiceHeader Invoice
		{
			get { return invoice ?? (invoice = Declaration.Invoices.AddNew()); }
		}
		JobComInvoiceHeader invoice;

		JobComInvoiceLine InvoiceLine
		{
			get { return invoiceLine ?? (invoiceLine = Invoice.JobComInvoiceLines.AddNew()); }
		}
		JobComInvoiceLine invoiceLine;

		NMFSLine NMFSLine
		{
			get
			{
				if (nmfsLine == null)
				{
					nmfsLine = InvoiceLine.NMFSLines.AddNew();
					nmfsLine.US_SourceType = "HCF";
				}
				return nmfsLine;
			}
		}
		NMFSLine nmfsLine;

		NMFSHarvestingDetail NMFSDetail
		{
			get
			{
				if (nmfsDetail == null)
				{
					nmfsDetail = NMFSLine.HarvestingDetails.AddNew();
				}
				return nmfsDetail;
			}
		}

		NMFSHarvestingDetail nmfsDetail;

		#endregion
	}
}
