using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	public interface IForwardingConsolDataObject
	{
		ZString ConsolNumber { get; }
		ICodeDescription TransportMode { get; }
		IUnloco LoadPort { get; }
		IUnloco DischargePort { get; }
		IUnloco PortOfFirstLoading { get; }
		IUnloco PortOfLastDischarge { get; }
		IUnloco PlaceOfReceipt { get; }
		IUnloco PlaceOfDelivery { get; }
		ZString FirstVoyageFlightNumber { get; }
		IAddress DepartureCFS { get; }
		IAddress ArrivalCFS { get; }
		ITransports Transports { get; }
	}
}
