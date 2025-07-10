using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportBookings.Business.Testing;

public class DummyCO2ePrePostCarriageWithDtbBooking : DummyWithDtbBooking, ICO2ePrePostCarriage
{
	public DummyCO2ePrePostCarriageWithDtbBooking(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public IPrePostCarriageLocation[] GetPreCarriageLocations(ZString hblDeliveryMode) => Array.Empty<IPrePostCarriageLocation>();

	public IPrePostCarriageLocation[] GetPostCarriageLocations(ZString hblDeliveryMode) => Array.Empty<IPrePostCarriageLocation>();

	public void OnTransportBookingCalculated(IDtbBooking booking)
	{
		onTransportBookingCalculatedCount++;
	}

	public void OnTransportBookingCO2eStatusChanged(IDtbBooking booking)
	{
		onTransportBookingCO2eStatusChangedCount++;
	}

	public void OnTransportBookingActiveStatusChanged(IDtbBooking booking)
	{
		onTransportBookingActiveStatusChangedCount++;
	}

	public bool RequiresPrePostCarriageLegs { get; set; }

	public int OnTransportBookingCalculatedCount => onTransportBookingCalculatedCount;
	int onTransportBookingCalculatedCount;

	public int OnTransportBookingCO2eStatusChangedCount => onTransportBookingCO2eStatusChangedCount;
	int onTransportBookingCO2eStatusChangedCount;

	public int OnTransportBookingActiveStatusChangedCount => onTransportBookingActiveStatusChangedCount;
	int onTransportBookingActiveStatusChangedCount;
}
