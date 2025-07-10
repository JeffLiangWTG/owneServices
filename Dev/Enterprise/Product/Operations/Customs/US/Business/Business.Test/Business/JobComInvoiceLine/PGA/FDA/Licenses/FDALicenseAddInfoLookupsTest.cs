using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FDALicenseAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountries()
		{
			AssertType<RefCountryCollection>(lookups.Countries);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var invoiceLine = Factory.New<JobComInvoiceLine>();
			var fdaLine = invoiceLine.ACE_FDALines.AddNew();
			fdaLine.US_ProgramCode = FDAProgramCodeList.Codes.FOO;
			var addInfo = new FDALicenseAddInfo(fdaLine.Licenses.AddNew().B7_AddInfoDataInfo);
			lookups = new FDALicenseAddInfoLookups(addInfo);
		}
		FDALicenseAddInfoLookups lookups;
	}
}
