using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbConsignmentRunSheetLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestTransportCos

		public void TestTransportCos()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertEquals(typeof(LocalTransportCollection), runSheet.Lookups.TransportCos.GetType());
		}

		#endregion

		#region TestTruckDrivers

		public void TestTruckDrivers()
		{
			var runSheet = Factory.New<DtbConsignmentRunSheet>();
			AssertEquals(typeof(StaffDriverCollection), runSheet.Lookups.TruckDrivers.GetType());
		}

		#endregion
	}
}
