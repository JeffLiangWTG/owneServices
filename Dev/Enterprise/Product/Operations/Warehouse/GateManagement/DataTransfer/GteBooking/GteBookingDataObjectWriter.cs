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
using Enterprise.Warehouse.Integration.CodeLists;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public class GteBookingDataObjectWriter : TopLevelDataObjectWriter<GteBooking, UniversalShipment>
	{
		public GteBookingDataObjectWriter(IDataWritingManager writeManager) : base(writeManager)
		{
		}

		protected override DataContextType GetTopLevelDataContextType() => DataContextType.GateBooking;

		protected override ZString GetEDIMessageSubType() => EDIMessageSubTypeList.Codes.XmlUniversalShipment;

		#region Populate

		protected override void PopulateDataObject(GteBooking booking, UniversalShipment dataObject)
		{
			dataObject.DataContext = DataContextFactory.New();
			dataObject.DataContext.AddDataSource(DataContextType.GateBooking, booking.GBK_ReferenceNumber);

			PopulateSlotDateTime(booking, dataObject);
			PopulateFacilityAddress(booking, dataObject);
			PopulateOrganizations(booking, dataObject);
			PopulateVehicleRun(booking, dataObject);

			var data = ProcessCollection(booking.GateMovementBookings, new GteGateMovementBookingDataObjectWriter(writeManager));
			dataObject.SetSubShipmentCollection(() => data != null ? new DataObjectList<UniversalShipment>(data) : null);
			dataObject.SetPreCarriageShipmentCollection(() => ProcessCollection(booking.VehicleMovementBookings, new GteVehicleMovementBookingDataObjectWriter(writeManager)));
		}

		void PopulateSlotDateTime(GteBooking booking, UniversalShipment dataObject)
		{
			var earliestGateMovementBooking = booking.GateMovementBookings.Cast<GteGateMovementBooking>().OrderBy(x => x.GBM_SlotStartTime).FirstOrDefault(x => x.GBM_SlotStartTime.IsValid);
			if (earliestGateMovementBooking != null)
			{
				dataObject.SlotDateTime = earliestGateMovementBooking.GBM_SlotStartTime.ToDateTime();
			}
		}

		void PopulateFacilityAddress(GteBooking booking, UniversalShipment dataObject)
		{
			var facility = booking.Facility;
			var address = facility.WarehouseAddress;
			switch (facility.WW_WarehouseType)
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

		void PopulateOrganizations(GteBooking booking, UniversalShipment dataObject)
		{
			dataObject.AddOrgAddress(writeManager, booking.TransportCompany, DocAddressType.TransportCompanyDocumentaryAddress);
			var bookingPartyDocumentaryAddress = booking.BookingPartyDocumentaryAddress;
			if (bookingPartyDocumentaryAddress != null)
			{
				dataObject.AddOrgAddress(writeManager, bookingPartyDocumentaryAddress, DocAddressType.BookingPartyDocumentaryAddress);
			}
		}

		void PopulateVehicleRun(GteBooking booking, UniversalShipment dataObject)
		{
			if (dataObject.VehicleRun is null)
			{
				dataObject.VehicleRun = new VehicleRun();
			}

			var vehicleRun = dataObject.VehicleRun;

			if (vehicleRun.CrewCollection is null)
			{
				vehicleRun.SetWriterStrategy(writeManager.WriterStrategy);
				vehicleRun.SetCrewCollection(() => new List<Crew>());
			}

			dataObject.VehicleRun.SetCrewCollection(() => ProcessCollection(booking.VehicleDriverBookings, new GteVehicleDriverBookingDataObjectWriter(writeManager)));
		}

		#endregion Populate
	}
}
