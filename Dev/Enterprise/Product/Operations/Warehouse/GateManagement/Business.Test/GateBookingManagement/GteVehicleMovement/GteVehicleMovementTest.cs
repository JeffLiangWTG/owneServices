using System;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.Business.Test
{
	[TestedType(typeof(GteVehicleMovement))]
	public sealed class GteVehicleMovementTest : EnterpriseBusinessObjectTestCase
	{
		public void TestVehicleMovementAndVehicleEntries()
		{
			var gateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateIn.GVE_IsIncoming = true;
			gateIn.GVE_EntryTime = DateTimeOffset.Now;

			var gateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			gateOut.GVE_IsIncoming = false;
			gateOut.GVE_EntryTime = DateTimeOffset.Now;

			var draftGateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			draftGateIn.GVE_IsIncoming = true;

			var cancelledGateIn = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateIn.GVE_IsIncoming = true;
			cancelledGateIn.GVE_EntryTime = DateTimeOffset.Now;
			cancelledGateIn.GVE_CancelledReason = "Cancelled";
			cancelledGateIn.GVE_CancelledTime = DateTimeOffset.Now.AddMinutes(5);
			cancelledGateIn.GVE_GS_NKCancelledBy = "ASA";

			var cancelledGateOut = Factory.NewWithValidTestData<GteVehicleEntry>();
			cancelledGateOut.GVE_IsIncoming = false;
			cancelledGateOut.GVE_EntryTime = DateTimeOffset.Now;
			cancelledGateOut.GVE_CancelledReason = "Cancelled";
			cancelledGateOut.GVE_CancelledTime = DateTimeOffset.Now.AddMinutes(5);
			cancelledGateOut.GVE_GS_NKCancelledBy = "ASA";

			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			vehicleMovement.VehicleEntries.Add(gateIn);
			vehicleMovement.VehicleEntries.Add(gateOut);
			vehicleMovement.VehicleEntries.Add(draftGateIn);
			vehicleMovement.VehicleEntries.Add(cancelledGateIn);
			vehicleMovement.VehicleEntries.Add(cancelledGateOut);

			AssertEquals("Expected GateInVehicleEntry to return gateIn", gateIn, vehicleMovement.GateInVehicleEntry);
			AssertEquals("Expected GateOutVehicleEntry to return gateOut", gateOut, vehicleMovement.GateOutVehicleEntry);
		}

		public void TestGetFacilityTypes()
		{
			var gvm = Factory.NewWithValidTestData<GteVehicleMovement>();
			AddFacilityTypeToGVM(gvm, WarehouseTypes.Codes.ContainerYard, "WW1");
			AddFacilityTypeToGVM(gvm, WarehouseTypes.Codes.Transit, "WW2");
			AddFacilityTypeToGVM(gvm, WarehouseTypes.Codes.Transit, "WW3");

			Factory.Save();
			AssertContainsExactElementsInAnyOrder(new[] { WarehouseTypes.Codes.ContainerYard, WarehouseTypes.Codes.Transit }, gvm.FacilityTypes);
		}

		void AddFacilityTypeToGVM(IGteVehicleMovement gvm, string facilityType, string facilityID)
		{
			var whs = Factory.NewWithValidTestData<WhsWarehouse>();
			whs.WW_WarehouseType = facilityType;
			whs.WW_WarehouseCode = facilityID;

			var gbk = Factory.NewWithValidTestData<GteBooking>();
			gbk.GBK_WW_Facility = whs.PK;

			var gbm = Factory.NewWithValidTestData<GteGateMovementBooking>();
			gbm.GBM_GBK_Booking = gbk.PK;

			var ggm = Factory.NewWithValidTestData<GteGateMovement>();
			ggm.GGM_GBM_MovementBooking = gbm.PK;
			ggm.GGM_GVM_VehicleMovement = gvm.PK;
		}
	}
}
