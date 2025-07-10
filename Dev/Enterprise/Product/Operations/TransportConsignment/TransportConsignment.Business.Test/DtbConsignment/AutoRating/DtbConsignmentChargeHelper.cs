using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	class DtbConsignmentChargeHelperHelper : TestCaseWithFactory
	{
		public void TestGetConsignment()
		{
			var consignment = Helper.CreateConsignment();
			var job = Helper.CreateJobHeader(consignment);
			var charge = Helper.CreateJobCharge(job);

			AssertEquals(consignment, DtbConsignmentChargeHelper.GetConsignment(charge));

			var job2 = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var charge2 = Helper.CreateJobCharge(job2);
			AssertNull(DtbConsignmentChargeHelper.GetConsignment(charge2));
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}
		TransportConsignmentTestHelper helper;
	}
}
