using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCommissionCalculationQueue))]
	sealed class OrgCommissionCalculationQueueTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var queue = Factory.New<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = agreement.PK;

			return queue;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var agreement = factory.NewWithValidTestData<OrgCommissionAgreement>();
			var queue = factory.New<OrgCommissionCalculationQueue>();
			queue.CAQ_CA0 = agreement.PK;

			return queue;
		}
	}
}
