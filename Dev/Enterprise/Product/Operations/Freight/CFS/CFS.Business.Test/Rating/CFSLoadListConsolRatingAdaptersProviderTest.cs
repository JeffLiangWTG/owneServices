using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSLoadListConsolRatingAdaptersProviderTest : TestCaseWithFactory
	{
		public void TestNoAdditionalJobsExist()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			var provider = ((IRatingSupporter)loadList).AdaptersProvider;
			AssertEquals(0, provider.GetAdditionalJobs().Count);

			var shipment1 = loadList.Shipments.AddNew();
			var shipment2 = loadList.Shipments.AddNew();
			AssertEquals(0, provider.GetAdditionalJobs().Count);
		}
	}
}
