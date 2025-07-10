using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class DtbRoutePlannerSelectedEntitiesTest : TestCase
	{
		#region TestConstruction

		public void TestConstruction()
		{
			var runSheetPK = ZGuid.NewZGuid();
			var carrierPK = ZGuid.NewZGuid();
			var driverPK = ZGuid.NewZGuid();
			var vehiclePK = ZGuid.NewZGuid();

			var selections = new DtbRoutePlannerSelectedEntities(runSheetPK, carrierPK, driverPK, vehiclePK);
			AssertEquals(runSheetPK, selections.RunSheetPK);
			AssertEquals(carrierPK, selections.CarrierPK);
			AssertEquals(driverPK, selections.DriverPK);
			AssertEquals(vehiclePK, selections.VehiclePK);
		}

		#endregion
	}
}
