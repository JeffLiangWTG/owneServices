using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(USNMFSVesselsAddInfo))]
	public class USNMFSVesselsAddInfofoTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new USNMFSVesselsAddInfo(NMFSDetail.HarvestingVessles.AddNew().B7_AddInfoDataInfo);
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
					nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
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
