using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	class USNMFSVesselsAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnitOfMeasureList()
		{
			AssertEquals("UnitOfMeasureList", Factory.GetCachedValue<ABIUnitOfMeasureList>(), AddInfo.Lookups.UnitOfMeasureList);
		}

		#region Implementation

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

		USNMFSVesselsAddInfo AddInfo
		{
			get { return addInfo ?? (addInfo = new USNMFSVesselsAddInfo(NMFSDetail.HarvestingVessles.AddNew().B7_AddInfoDataInfo)); }
		}
		USNMFSVesselsAddInfo addInfo;

		NMFSLine NMFSLine
		{
			get
			{
				if (nmfsLine == null)
				{
					nmfsLine = InvoiceLine.NMFSLines.AddNew();
					nmfsLine.US_ProgramType = NMFSProgramCodeList.Codes.SIM;
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
