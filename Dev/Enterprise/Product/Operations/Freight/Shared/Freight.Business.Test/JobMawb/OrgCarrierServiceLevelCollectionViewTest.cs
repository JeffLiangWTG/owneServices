using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(JobMawb.OrgCarrierServiceLevelCollectionView))]
	sealed class OrgCarrierServiceLevelCollectionViewTest : BusinessObjectCollectionViewTestCase<JobMawb.OrgCarrierServiceLevelCollectionView>
	{
		protected override JobMawb.OrgCarrierServiceLevelCollectionView GetCollectionToTest()
		{
			var serviceLevels = new OrgCarrierServiceLevelCollection(Factory);
			var mawb = Factory.NewWithValidTestData<JobMawb>();
			return new JobMawb.OrgCarrierServiceLevelCollectionView(serviceLevels, mawb.ServiceLevelFilter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<OrgCarrierServiceLevel>();
		}
	}
}
