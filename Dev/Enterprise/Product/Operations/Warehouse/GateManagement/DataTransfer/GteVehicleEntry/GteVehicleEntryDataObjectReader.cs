using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleEntryDataObjectReader : ShipmentDataObjectReader<GteVehicleEntry>
	{
		public GteVehicleEntryDataObjectReader(GteVehicleMovement vehicleMovement, GteBooking booking, Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObject, logger, factory)
		{
			this.vehicleMovement = Argument.NotNull(vehicleMovement, nameof(vehicleMovement));
			this.booking = Argument.NotNull(booking, nameof(booking));
		}
		readonly GteVehicleMovement vehicleMovement;
		readonly GteBooking booking;

		public override DataContextType DataContextType => DataContextType.GateVehicleMovement;

		protected override IMatchingBusinessEntityFinder<GteVehicleEntry> GetCombinedReferenceMatcher() => null;

		protected override GteVehicleEntry GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			var isIncoming = dataObject.GetAdditionalInfoOrDefault("IsIncoming") ?? "";
			if (bool.TryParse(isIncoming, out var isIncomingBoolean))
			{
				var query = new ZQuery(GteVehicleEntrySchema.GVE_IsIncoming, isIncomingBoolean)
					.AddToFilter(new ZQuery(GteVehicleEntrySchema.GVE_CancelledReason, ""))
					.AddToFilter(new ZQuery(GteVehicleEntrySchema.GVE_GVM_VehicleMovement, vehicleMovement.PK));
				return factory.LoadTop1<GteVehicleEntry>(query);
			}
			return null;
		}

		protected override void PopulateBusinessObject(GteVehicleEntry vehicleEntry)
		{
			PopulateEntryLane(vehicleEntry);
			PopulateEntryDirection(vehicleEntry);
			PopulateEntryWeight(vehicleEntry);
			PopulateDriverDetails(vehicleEntry);
		}

		void PopulateEntryLane(GteVehicleEntry vehicleEntry)
		{
			var addressLine = dataObject.WarehouseLocation.ToString();
			if (!string.IsNullOrEmpty(addressLine) && addressLine.Length == 7)
			{
				var laneCode = addressLine.Substring(4, 3);
				var gateCode = addressLine.Substring(0, 3);

				var gates = factory.Load<GteGate>(new ZQuery(GteGateSchema.GTE_Code, gateCode));
				var lanes = factory.Load<GteLane>(new ZQuery(GteLaneSchema.GLN_Code, laneCode).AddToFilter(JoinCondition.And, GteLaneSchema.GLN_GTE_Gate, gates.Select(gate => gate.PK)));
				var lane = lanes.FirstOrDefault(lane => lane.Gate.GTE_WW_Facility == booking.GBK_WW_Facility) ?? lanes.FirstOrDefault(lane => lane.Gate.GTE_GB_Branch == booking.Facility.WW_GB_RelatedCompanyBranch);
				if (lane != null)
				{
					SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_GLN_Lane, lane.PK);
				}
			}

			if (vehicleEntry.GateLane == null)
			{
				throw new DataObjectReadFailureException(Res.GetString("252c3609-ac9d-459d-9556-ca416309f203", "Could not find a valid Lane for the given location. Warehouse Location value was {0}", addressLine));
			}
		}

		void PopulateEntryDirection(GteVehicleEntry vehicleEntry)
		{
			var isIncoming = dataObject.GetAdditionalInfoOrDefault("IsIncoming") ?? "";
			var validIncomingValue = bool.TryParse(isIncoming, out var isIncomingBoolean);

			if (!validIncomingValue)
			{
				throw new DataObjectReadFailureException(Res.GetString("fe22ff8e-bf74-45c1-b1a3-b4e699dad4d6", "Vehicle Entry must have Is Incoming as true or false. Is Incoming was: {0}", isIncoming));
			}

			SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_IsIncoming, isIncomingBoolean);
		}

		void PopulateEntryWeight(GteVehicleEntry vehicleEntry)
		{
			var entryWeight = dataObject.TotalWeight ?? -1;
			var weightUnit = dataObject.TotalWeightUnit?.Code?.ToString() ?? "";

			if (entryWeight > 0)
			{
				SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_Weight, entryWeight);
			}

			if (!string.IsNullOrEmpty(weightUnit))
			{
				SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_WeightUQ, weightUnit);
			}

			if (vehicleEntry.GVE_Weight > 0 && string.IsNullOrEmpty(vehicleEntry.GVE_WeightUQ))
			{
				SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_WeightUQ, "NA");
			}
		}

		void PopulateDriverDetails(GteVehicleEntry vehicleEntry)
		{
			var license = dataObject.VehicleRun?.CrewCollection?.FirstOrDefault()?.LicenseNumber;
			var driver = dataObject.VehicleRun?.CrewCollection?.FirstOrDefault()?.FullName;

			if (string.IsNullOrEmpty(license) || string.IsNullOrEmpty(driver))
			{
				var driverBooking = booking.VehicleDriverBookings.FirstOrDefault() as GteVehicleDriverBooking;
				if (driverBooking != null)
				{
					license = (string.IsNullOrEmpty(license) && string.IsNullOrEmpty(vehicleEntry.GVE_DriverLicenseNumber)) ? driverBooking.GBD_DriverLicenseNumber : license;
					driver = (string.IsNullOrEmpty(driver) && string.IsNullOrEmpty(vehicleEntry.GVE_DriverName)) ? driverBooking.GBD_DriverName : driver;
				}
			}

			if (!string.IsNullOrEmpty(license))
			{
				SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_DriverLicenseNumber, license);
			}
			else if (string.IsNullOrEmpty(vehicleEntry.GVE_DriverLicenseNumber))
			{
				throw new DataObjectReadFailureException(Res.GetString("ad4d4f1a-93a7-458a-9b53-8b4a94632711", "Vehicle Entry must specify a Driver License"));
			}

			if (!string.IsNullOrEmpty(driver))
			{
				SetValue(vehicleEntry, GteVehicleEntrySchema.GVE_DriverName, driver);
			}
			else if (string.IsNullOrEmpty(vehicleEntry.GVE_DriverName))
			{
				throw new DataObjectReadFailureException(Res.GetString("38aaec0f-2112-4c18-a9cb-ecaf595677a7", "Vehicle Entry must specify a Driver Name"));
			}
		}
	}
}
