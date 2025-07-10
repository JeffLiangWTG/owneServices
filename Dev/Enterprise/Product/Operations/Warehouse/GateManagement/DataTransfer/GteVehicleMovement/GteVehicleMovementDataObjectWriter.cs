using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.GateManagement.Business;
using Enterprise.Warehouse.GateManagement.Integration.Interfaces;
using Enterprise.Warehouse.Integration.CodeLists;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteVehicleMovementDataObjectWriter : TopLevelDataObjectWriter<GteVehicleMovement, UniversalShipment>, IGteVehicleMovementDataObjectWriter
	{
		public GteVehicleMovementDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.GateVehicleMovement;

		protected override void PopulateDataObject(GteVehicleMovement vehicleMovement, UniversalShipment dataObject)
		{
			var gateInEntry = vehicleMovement.VehicleEntries.FirstOrDefault(entry => entry.GVE_CancelledReason == "" && entry.GVE_IsIncoming)
					?? vehicleMovement.VehicleEntries.FirstOrDefault(entry => entry.GVE_IsIncoming);
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.GateVehicleMovement, gateInEntry?.GVE_GateActionNumber ?? ZString.Empty);

			PopulateOrganizations(vehicleMovement, dataObject);
			PopulateWarehouseLocation(vehicleMovement, dataObject);
			PopulateFacilityAddress(vehicleMovement, dataObject);
			PopulateVehicleRun(vehicleMovement, dataObject);
			PopulateVehicleEntryTimes(vehicleMovement, dataObject);

			dataObject.SetRelatedShipmentCollection(() => ProcessCollection(vehicleMovement.VehicleEntries, new GteVehicleEntryDataObjectWriter(writeManager)));

			var data = ProcessCollection(vehicleMovement.GateMovements, new GteGateMovementDataObjectWriter(writeManager));
			dataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
		}

		void PopulateOrganizations(GteVehicleMovement vehicleMovement, UniversalShipment dataObject)
		{
			var transportCompany = vehicleMovement.GateMovements[0].GateMovementBooking.Booking.TransportCompany;
			dataObject.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			dataObject.AddOrgAddress(writeManager, transportCompany, DocAddressType.TransportCompanyDocumentaryAddress);
		}

		void PopulateFacilityAddress(GteVehicleMovement vehicleMovement, UniversalShipment dataObject)
		{
			if (!vehicleMovement.GVM_WL_Location.IsValid)
			{
				return;
			}

			var facility = vehicleMovement.WarehouseLocation?.Warehouse;
			var address = facility?.WarehouseAddress;
			switch (facility?.WW_WarehouseType)
			{
				case WarehouseTypes.Codes.ContainerYard:
					dataObject.AddOrgAddress(writeManager, address, DocAddressType.LocalCartageYard);
					break;
				case WarehouseTypes.Codes.Transit:
					dataObject.AddOrgAddress(writeManager, address, DocAddressType.ArrivalCFSAddress);
					dataObject.AddOrgAddress(writeManager, address, DocAddressType.DepartureCFSAddress);
					break;
				default:
					break;
			}
		}

		void PopulateWarehouseLocation(GteVehicleMovement vehicleMovement, UniversalShipment dataObject)
		{
			if (vehicleMovement.GVM_WL_Location.IsValid)
			{
				dataObject.WarehouseLocation = vehicleMovement.WarehouseLocation.ToLocationString();
			}
		}

		void PopulateVehicleRun(GteVehicleMovement vehicleMovement, UniversalShipment dataObject)
		{
			dataObject.VehicleRun = new VehicleRun() { Vehicle = new Vehicle() };
			if (vehicleMovement.GVM_VehicleRegistration.Length > 0)
			{
				dataObject.VehicleRun.Vehicle.Registration = new Registration { Number = vehicleMovement.GVM_VehicleRegistration };
			}

			if (vehicleMovement.VehicleType != null)
			{
				dataObject.VehicleRun.Vehicle.VehicleType = new CodeDescriptionPair10Char
				{
					Code = vehicleMovement.VehicleType.RC_Code,
					Description = vehicleMovement.VehicleType.RC_DescriptionMultilingual
				};
			}
		}

		void PopulateVehicleEntryTimes(GteVehicleMovement vehicleMovement, UniversalShipment dataObject)
		{
			var hasGateIn = vehicleMovement.GateInVehicleEntry != null && vehicleMovement.GateInVehicleEntry.GVE_EntryTime.IsValid;
			var hasGateOut = vehicleMovement.GateOutVehicleEntry != null && vehicleMovement.GateOutVehicleEntry.GVE_EntryTime.IsValid;
			if (hasGateIn || hasGateOut)
			{
				dataObject.SetDateCollection(() => new List<Date>());
			}

			if (hasGateIn)
			{
				dataObject.DateCollection.Add(Date.New(DateType.Start, ZBool.False, vehicleMovement.GateInVehicleEntry.GVE_EntryTime));
			}

			if (hasGateOut)
			{
				dataObject.DateCollection.Add(Date.New(DateType.End, ZBool.False, vehicleMovement.GateOutVehicleEntry.GVE_EntryTime));
			}
		}
	}
}
