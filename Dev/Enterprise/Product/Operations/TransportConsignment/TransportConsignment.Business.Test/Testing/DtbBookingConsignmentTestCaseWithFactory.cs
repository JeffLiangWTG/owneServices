using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	public class DtbBookingConsignmentTestCaseWithFactory : TestCaseWithFactory
	{
		public void TestServiceBranch()
		{
			var bookingConsignment = Factory.New<DtbBookingConsignment>();
			var iHaveServices = (IHaveServices)bookingConsignment;
			AssertEquals("Service branch is defaulted", Env.CurrentBranch.PK, iHaveServices.ServiceBranch.PK);

			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			bookingConsignment.KM_GB_Branch = branch.PK;
			AssertEquals("Service branch", branch.PK, iHaveServices.ServiceBranch.PK);
		}

		public void TestIPackingParent_IsPackingJobReadOnly()
		{
			var bookingConsignment = Factory.New<DtbBookingConsignment>();
			AssertEquals(false, ((IPackingParent)bookingConsignment).IsPackingJobReadOnly);
		}

		#region Implementation

		protected TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
