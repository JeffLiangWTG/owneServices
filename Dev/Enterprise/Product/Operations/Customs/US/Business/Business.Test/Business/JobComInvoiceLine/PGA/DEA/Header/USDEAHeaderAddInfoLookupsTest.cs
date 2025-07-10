using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USDEAHeaderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			AssertNotNull(Header.AddInfoLookups.Countries);
		}

		public void TestFormTypes()
		{
			AssertNotNull(Header.AddInfoLookups.FormTypes);
		}

		public void TestWeightUQList()
		{
			AssertNotNull(Header.AddInfoLookups.WeightUQList);
		}

		DEAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.JobComInvoiceLines.AddNew();
					header = invoiceLine.DEAHeaders.AddNew();
				}
				return header;
			}
		}
		DEAHeader header;
	}
}
