using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USCountriesAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUSCountryList()
		{
			AssertNotNull(Lookups.USCountryList);
		}

		USCountriesAddInfoLookups Lookups
		{
			get { return LaceyCountry.AddInfoLookups; }
		}

		LaceyCountry LaceyCountry
		{
			get
			{
				if (laceyCountry == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					var pga = invoiceLine.LaceyActLines.AddNew();
					laceyCountry = pga.LaceyCountries.AddNew();
				}
				return laceyCountry;
			}
		}
		LaceyCountry laceyCountry;
	}
}
