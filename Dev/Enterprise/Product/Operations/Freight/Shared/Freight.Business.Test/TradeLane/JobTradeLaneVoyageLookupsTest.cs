using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class JobTradeLaneVoyageLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestHeaders()
		{
			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			AssertEquals(jobTradeLaneVoyage.Factory, jobTradeLaneVoyage.Lookups.Headers.Factory);
			AssertEquals(typeof(ShipsAgencyPrincipalCollection), jobTradeLaneVoyage.Lookups.Headers.GetType());
		}

		public void TestJobTradeLaneList()
		{
			JobTradeLaneVoyage jobTradeLaneVoyage = Factory.New<JobTradeLaneVoyage>();
			AssertEquals(jobTradeLaneVoyage.Factory, jobTradeLaneVoyage.Lookups.JobTradeLaneList.Factory);
			AssertEquals(typeof(JobTradeLaneCollection), jobTradeLaneVoyage.Lookups.JobTradeLaneList.GetType());
		}
	}
}
