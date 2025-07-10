using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.GateManagement.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.GateManagement.DataTransfer.Test
{
	[TestedType(typeof(GteVehicleEntryDataObjectWriter))]
	public class GteVehicleEntryDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestGivenAllFieldsSetGateIn_WhenGenerateUXML_ThenUXMLFieldsAreCorrect()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GateLane.GLN_Code = "LNE";
			vehicleEntry.GateLane.Gate.GTE_Code = "GTE";
			vehicleEntry.GVE_Weight = 20;
			vehicleEntry.GVE_WeightUQ = "KG";
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			var writer = new GteVehicleEntryDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement)));
			var shipment = writer.GetDataObject(vehicleEntry);

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry.Lane should be mapped correctly", "GTE|LNE", shipment.WarehouseLocation);
				AssertEquals("GteVehicleEntry.Weight should be mapped correctly", (ZDecimal)20, shipment.TotalWeight);
				AssertEquals("GteVehicleEntry.WeightUQ should be mapped correctly", "KG", shipment.TotalWeightUnit.Code);
				AssertEquals("GteVehicleEntry.Is_Incoming should be mapped correctly", bool.TrueString, shipment.GetAdditionalInfoOrDefault("IsIncoming"));
				AssertEquals("GteVehicleEntry.DriverLicenseNumber should be mapped correctly", "1234 5678", shipment.VehicleRun.CrewCollection[0].LicenseNumber);
				AssertEquals("GteVehicleEntry.DriverName should be mapped correctly", "Jimothy", shipment.VehicleRun.CrewCollection[0].FullName);
			});
		}

		public void TestGivenAllFieldsSetGateOut_WhenGenerateUXML_ThenUXMLFieldsAreCorrect()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GateLane.GLN_Code = "LNE";
			vehicleEntry.GateLane.Gate.GTE_Code = "GTE";
			vehicleEntry.GVE_Weight = 20;
			vehicleEntry.GVE_WeightUQ = "KG";
			vehicleEntry.GVE_IsIncoming = false;
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			var writer = new GteVehicleEntryDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement)));
			var shipment = writer.GetDataObject(vehicleEntry);

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry.Lane should be mapped correctly", "GTE|LNE", shipment.WarehouseLocation);
				AssertEquals("GteVehicleEntry.Weight should be mapped correctly", (ZDecimal)20, shipment.TotalWeight);
				AssertEquals("GteVehicleEntry.WeightUQ should be mapped correctly", "KG", shipment.TotalWeightUnit.Code);
				AssertEquals("GteVehicleEntry.Is_Incoming should be mapped correctly", bool.FalseString, shipment.GetAdditionalInfoOrDefault("IsIncoming"));
				AssertEquals("GteVehicleEntry.DriverLicenseNumber should be mapped correctly", "1234 5678", shipment.VehicleRun.CrewCollection[0].LicenseNumber);
				AssertEquals("GteVehicleEntry.DriverName should be mapped correctly", "Jimothy", shipment.VehicleRun.CrewCollection[0].FullName);
			});
		}

		public void TestGivenWeightZeroAndNoUnit_WhenGenerateUXML_ThenUXMLFieldsAreCorrect()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GateLane.GLN_Code = "LNE";
			vehicleEntry.GateLane.Gate.GTE_Code = "GTE";
			vehicleEntry.GVE_Weight = 0;
			vehicleEntry.GVE_WeightUQ = "";
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			var writer = new GteVehicleEntryDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement)));
			var shipment = writer.GetDataObject(vehicleEntry);

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry.Lane should be mapped correctly", "GTE|LNE", shipment.WarehouseLocation);
				AssertEquals("GteVehicleEntry.Weight should be mapped correctly", (ZDecimal)0, shipment.TotalWeight);
				AssertEquals("GteVehicleEntry.WeightUQ should be mapped correctly", null, shipment.TotalWeightUnit);
				AssertEquals("GteVehicleEntry.Is_Incoming should be mapped correctly", bool.TrueString, shipment.GetAdditionalInfoOrDefault("IsIncoming"));
				AssertEquals("GteVehicleEntry.DriverLicenseNumber should be mapped correctly", "1234 5678", shipment.VehicleRun.CrewCollection[0].LicenseNumber);
				AssertEquals("GteVehicleEntry.DriverName should be mapped correctly", "Jimothy", shipment.VehicleRun.CrewCollection[0].FullName);
			});
		}

		public void TestGivenWeightZeroAndUnit_WhenGenerateUXML_ThenUXMLFieldsAreCorrect()
		{
			var vehicleMovement = Factory.NewWithValidTestData<GteVehicleMovement>();
			var vehicleEntry = Factory.NewWithValidTestData<GteVehicleEntry>();
			vehicleEntry.GVE_GVM_VehicleMovement = vehicleMovement.PK;
			vehicleEntry.GateLane.GLN_Code = "LNE";
			vehicleEntry.GateLane.Gate.GTE_Code = "GTE";
			vehicleEntry.GVE_Weight = 0;
			vehicleEntry.GVE_WeightUQ = "KG";
			vehicleEntry.GVE_IsIncoming = true;
			vehicleEntry.GVE_DriverLicenseNumber = "1234 5678";
			vehicleEntry.GVE_DriverName = "Jimothy";

			var writer = new GteVehicleEntryDataObjectWriter(new DataWritingManager(new ActionInfo(null, vehicleMovement)));
			var shipment = writer.GetDataObject(vehicleEntry);

			CombineAssertions(() =>
			{
				AssertEquals("GteVehicleEntry.Lane should be mapped correctly", "GTE|LNE", shipment.WarehouseLocation);
				AssertEquals("GteVehicleEntry.Weight should be mapped correctly", (ZDecimal)0, shipment.TotalWeight);
				AssertEquals("GteVehicleEntry.WeightUQ should be mapped correctly", "KG", shipment.TotalWeightUnit.Code);
				AssertEquals("GteVehicleEntry.Is_Incoming should be mapped correctly", bool.TrueString, shipment.GetAdditionalInfoOrDefault("IsIncoming"));
				AssertEquals("GteVehicleEntry.DriverLicenseNumber should be mapped correctly", "1234 5678", shipment.VehicleRun.CrewCollection[0].LicenseNumber);
				AssertEquals("GteVehicleEntry.DriverName should be mapped correctly", "Jimothy", shipment.VehicleRun.CrewCollection[0].FullName);
			});
		}
	}
}
