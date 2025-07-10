using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleEntryDataObjectWriter : DataObjectWriter<GteVehicleEntry, UniversalShipment>
	{
		public GteVehicleEntryDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override UniversalShipment PopulateDataObject(GteVehicleEntry vehicleEntry)
		{
			if (vehicleEntry.GVE_CancelledReason != "")
			{
				return null;
			}
			var dataObject = new UniversalShipment(writeManager.WriterStrategy);
			PopulateWarehouseLocation(vehicleEntry, dataObject);
			PopulateAdditionalInfo(vehicleEntry, dataObject);
			PopulateEntryWeight(vehicleEntry, dataObject);
			PopulateVehicleRun(vehicleEntry, dataObject);

			return dataObject;
		}

		void PopulateWarehouseLocation(GteVehicleEntry vehicleEntry, UniversalShipment dataObject)
		{
			var laneCode = vehicleEntry.GateLane.GLN_Code;
			var gateCode = vehicleEntry.GateLane.Gate.GTE_Code;

			dataObject.WarehouseLocation = gateCode + "|" + laneCode;
		}

		void PopulateAdditionalInfo(GteVehicleEntry vehicleEntry, UniversalShipment dataObject)
		{
			dataObject.AddAddInfo("IsIncoming", vehicleEntry.GVE_IsIncoming ? (ZString)bool.TrueString : (ZString)bool.FalseString);
		}

		void PopulateEntryWeight(GteVehicleEntry vehicleEntry, UniversalShipment dataObject)
		{
			dataObject.TotalWeight = vehicleEntry.GVE_Weight;
			if (!string.IsNullOrEmpty(vehicleEntry.GVE_WeightUQ))
			{
				dataObject.TotalWeightUnit = new UnitOfWeight() { Code = vehicleEntry.GVE_WeightUQ };
			}
		}

		void PopulateVehicleRun(GteVehicleEntry vehicleEntry, UniversalShipment dataObject)
		{
			dataObject.VehicleRun = new VehicleRun();
			dataObject.VehicleRun.SetWriterStrategy(writeManager.WriterStrategy);
			dataObject.VehicleRun.SetCrewCollection(() => new List<Crew>() {
				new Crew()
				{
					FullName = vehicleEntry.GVE_DriverName,
					LicenseNumber = vehicleEntry.GVE_DriverLicenseNumber,
				}
			});
		}
	}
}
