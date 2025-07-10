using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleEntryCollectionDataObjectReader))]
	public class GteVehicleEntryCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var booking = Factory.NewWithValidTestData<GteBooking>();
			booking.GBK_WW_Facility = WarehouseForTest.PK;
			Factory.SaveForTesting();

			var shipments = new Shipment[]
			{
				GetVehicleEntryShipmentForTest(true),
				GetVehicleEntryShipmentForTest(false),
			};

			var reader = new GteVehicleEntryCollectionDataObjectReader(vehicleMovement, booking, new DummyLogger(), Factory, shipments);
			reader.ReadIntoCollection();

			AssertEquals("Expect the VehicleMovement to have two VehicleEntries", 2, vehicleMovement.VehicleEntries.Count);
			AssertEquals("Expect the VehicleMovement to have an incoming and outgoing VehicleEntry", 2, vehicleMovement.VehicleEntries.Select(entry => entry.GVE_IsIncoming).Distinct().Count());
		}

		#region Helpers

		Shipment GetVehicleEntryShipmentForTest(bool isGateIn)
		{
			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.TotalWeight = 20;
			shipment.TotalWeightUnit = new UnitOfWeight() { Code = "LB" };
			shipment.WarehouseLocation = LaneForTest.Gate.GTE_Code + "|" + LaneForTest.GLN_Code;

			shipment.SetAddInfoCollection(() => new List<AddInfo>()
			{
				new AddInfo() { Key = "IsIncoming", Value = isGateIn ? "true" : "false" }
			});

			shipment.VehicleRun = new VehicleRun();
			shipment.VehicleRun.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.VehicleRun.SetCrewCollection(() => new List<Crew>()
			{
				new Crew() { FullName = "Jimothy", LicenseNumber = "1234 5678" }
			});

			return shipment;
		}

		GteLane LaneForTest
		{
			get
			{
				if (laneForTest == null)
				{
					laneForTest = Factory.NewWithValidTestData<GteLane>();
					laneForTest.GLN_Code = "LNE";
					laneForTest.Gate.GTE_Code = "GTE";
					laneForTest.Gate.GTE_GB_Branch = BranchForTest.PK;
					laneForTest.Gate.GTE_WW_Facility = WarehouseForTest.PK;
					Factory.SaveForTesting();
				}
				return laneForTest;
			}
			set => laneForTest = value;
		}
		GteLane laneForTest;

		GlbBranch BranchForTest
		{
			get
			{
				if (branchForTest == null)
				{
					branchForTest = Factory.NewWithValidTestData<GlbBranch>();
					Factory.SaveForTesting();
				}
				return branchForTest;
			}
			set => branchForTest = value;
		}
		GlbBranch branchForTest;

		IWhsWarehouse WarehouseForTest
		{
			get
			{
				if (warehouseForTest == null)
				{
					warehouseForTest = Factory.BOFactory.New<IWhsWarehouse>();
					((BusinessObject)warehouseForTest).FillWithValidTestData();
					warehouseForTest.WW_GB_RelatedCompanyBranch = BranchForTest.PK;
					Factory.SaveForTesting();
				}
				return warehouseForTest;
			}
			set => warehouseForTest = value;
		}
		IWhsWarehouse warehouseForTest;

		#endregion
	}
}
