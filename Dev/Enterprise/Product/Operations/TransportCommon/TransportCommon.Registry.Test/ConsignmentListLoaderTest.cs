using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportCommon.Integration;

namespace Enterprise.TransportCommon.Registry.Testing
{
	public class ConsignmentListLoaderTest : TestCaseWithFactory
	{
		#region TestTransportBookingChargeCodeSubGroups

		public void TestTransportBookingChargeCodeSubGroups()
		{
			var consignmentListLoader = ObjectFactory.Get<IConsignmentListLoader>();
			var jobServiceList = consignmentListLoader.GetTransportBookingJobServices();

			AssertNotNull(jobServiceList);
			AssertEquals(15, jobServiceList.Count);
		}

		#endregion

		#region

		public void TestLandTransportChargeCodeSubGroups()
		{
			var consignmentListLoader = ObjectFactory.Get<IConsignmentListLoader>();
			var jobServiceList = consignmentListLoader.GetTransportJobServices();

			AssertNotNull(jobServiceList);
			AssertEquals(15, jobServiceList.Count);
		}

		#endregion
	}
}
