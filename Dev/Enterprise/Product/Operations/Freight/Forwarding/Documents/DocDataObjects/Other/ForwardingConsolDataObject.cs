using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	sealed class ForwardingConsolDataObject : DocDataObject, IForwardingConsolDataObject
	{
		public ForwardingConsolDataObject(object identifier)
			: base(identifier)
		{
		}

		#region ConsolNumber

		public ZString ConsolNumber
		{
			get => consolNumber;
			set
			{
				if (SetNonPersistentPropertyValue(ConsolNumberInfo, ref consolNumber, value))
				{
					Validate(ConsolNumberInfo);
				}
			}
		}

		ZString consolNumber;

		public ZPropertyInfo ConsolNumberInfo => GetZPropertyInfo(nameof(ConsolNumber));

		#endregion

		#region TransportMode

		public ICodeDescription TransportMode
		{
			get => transportMode;
			set => transportMode = SetChild(transportMode, value);
		}

		ICodeDescription transportMode;

		#endregion

		#region LoadPort

		public IUnloco LoadPort
		{
			get => loadPort;
			set => loadPort = SetChild(loadPort, value);
		}

		IUnloco loadPort;

		#endregion

		#region DischargePort

		public IUnloco DischargePort
		{
			get => dischargePort;
			set => dischargePort = SetChild(dischargePort, value);
		}

		IUnloco dischargePort;

		#endregion

		#region PortOfFirstLoading

		public IUnloco PortOfFirstLoading
		{
			get => portOfFirstLoading;
			set => portOfFirstLoading = SetChild(portOfFirstLoading, value);
		}

		IUnloco portOfFirstLoading;

		#endregion

		#region PortOfLastDischarge

		public IUnloco PortOfLastDischarge
		{
			get => portOfLastDischarge;
			set => portOfLastDischarge = SetChild(portOfLastDischarge, value);
		}

		IUnloco portOfLastDischarge;

		#endregion

		#region PlaceOfReceipt

		public IUnloco PlaceOfReceipt
		{
			get => placeOfReceipt;
			set => placeOfReceipt = SetChild(placeOfReceipt, value);
		}

		IUnloco placeOfReceipt;

		#endregion

		#region PlaceOfDelivery

		public IUnloco PlaceOfDelivery
		{
			get => placeOfDelivery;
			set => placeOfDelivery = SetChild(placeOfDelivery, value);
		}

		IUnloco placeOfDelivery;

		#endregion

		#region FirstVoyageFlightNumber

		public ZString FirstVoyageFlightNumber
		{
			get => firstVoyageFlightNumber;
			set
			{
				if (SetNonPersistentPropertyValue(FirstVoyageFlightNumberInfo, ref firstVoyageFlightNumber, value))
				{
					Validate(FirstVoyageFlightNumberInfo);
				}
			}
		}
		ZString firstVoyageFlightNumber;

		public ZPropertyInfo FirstVoyageFlightNumberInfo => GetZPropertyInfo(nameof(FirstVoyageFlightNumber));

		#endregion

		#region DepartureCFS

		public IAddress DepartureCFS
		{
			get => departureCFS;
			set => departureCFS = SetChild(departureCFS, value);
		}

		IAddress departureCFS;

		#endregion

		#region ArrivalCFS

		public IAddress ArrivalCFS
		{
			get => arrivalCFS;
			set => arrivalCFS = SetChild(arrivalCFS, value);
		}

		IAddress arrivalCFS;

		#endregion

		#region Transports

		public ITransports Transports
		{
			get => transports;
			set => transports = SetChild(transports, value);
		}
		ITransports transports;

		#endregion
	}
}
