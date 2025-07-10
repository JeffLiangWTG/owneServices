using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobTradeLaneVoyageValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNB_OH()
		{
			JobVoyage voyage = Factory.New<JobVoyage>();

			OrgHeader nonPrincipalOrg = Factory.New<OrgHeader>();
			nonPrincipalOrg.OH_Code = "NonPrincipal";
			nonPrincipalOrg.OH_IsShippingProvider = false;

			Factory.Save();

			JobTradeLaneVoyage jobTradeLaneVoyage1 = Factory.New<JobTradeLaneVoyage>();
			jobTradeLaneVoyage1.NB_JV = voyage.PK;
			jobTradeLaneVoyage1.NB_OH = nonPrincipalOrg.PK;

			AssertHasError(jobTradeLaneVoyage1.NB_OHInfo, "Enter a valid Principal.");

			OrgHeader principalOrg = Factory.New<OrgHeader>();
			principalOrg.OH_Code = "Principal";
			principalOrg.OH_IsShippingProvider = true;
			principalOrg.CompanyData.OB_CRIsShipsAgencyPrincipal = true;
			Factory.Save();

			jobTradeLaneVoyage1.NB_OH = principalOrg.PK;

			AssertNoErrors(jobTradeLaneVoyage1.NB_OHInfo);

			JobTradeLaneVoyage jobTradeLaneVoyage2 = Factory.New<JobTradeLaneVoyage>();
			jobTradeLaneVoyage2.NB_JV = voyage.PK;
			jobTradeLaneVoyage2.NB_OH = principalOrg.PK;

			AssertHasError(jobTradeLaneVoyage2.NB_OHInfo, "This principal already exists on this schedule.");
		}
	}
}
