using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobComInvChargeCollection))]
	sealed class JobComInvChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<JobComInvChargeCollection, JobComInvCharge>
	{
		protected override JobComInvChargeCollection GetCollectionToTest()
		{
			return new JobComInvChargeCollection(Order);
		}

		protected override CargoWise.EntityFramework.BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<JobComInvCharge>();
		}

		Order Order
		{
			get { return order ?? (order = Factory.New<Order>()); }
		}
		Order order;
	}
}
