using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleEntry))]
	public sealed class GteVehicleEntryTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGateActionNumberGeneration()
		{
			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			transportCompany.OH_Code = "ABC";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GVE_GateActionNumber = "";

			Factory.Save();
			AssertEquals("Expected GVE_GateActionNumber to be populated by number fountain if empty.", "GVE00001000", vehicleEntry.GVE_GateActionNumber);

			vehicleEntry.GVE_GateActionNumber = "GVE3000";
			Factory.Save();
			AssertEquals("Expected GVE_GateActionNumber to be unchanged by number fountain if not empty.", "GVE3000", vehicleEntry.GVE_GateActionNumber);
		}
	}
}
