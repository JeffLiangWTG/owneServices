using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class USFWSLicenseAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			var list = License.AddInfoLookups.Countries;
			AssertEquals("Countries", typeof(RefCountryCollection), list.GetType());
		}

		public void TestLicenseTypes()
		{
			var list = License.AddInfoLookups.LicenseTypes;
			AssertEquals("LicenseTypes", Factory.GetCachedValue<FWSLicenseTypeList>(), list);
		}

		#region Implementation

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
				}
				return declaration;
			}
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

		FWSHeader Header
		{
			get
			{
				if (fwsHeader == null)
				{
					fwsHeader = InvoiceLine.FWSHeaders.AddNew();
				}
				return fwsHeader;
			}
		}
		FWSHeader fwsHeader;

		FWSLicense License
		{
			get { return license ?? (license = Header.Licenses.AddNew()); }
		}
		FWSLicense license;

		#endregion
	}
}
