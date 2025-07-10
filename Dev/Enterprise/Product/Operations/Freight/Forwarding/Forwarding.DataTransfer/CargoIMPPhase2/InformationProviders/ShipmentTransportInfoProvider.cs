using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	class ShipmentTransportInfoProvider : InformationProvider
	{
		public ShipmentTransportInfoProvider(ForwardingShipment shipment, Transport transport)
			: base(shipment.Factory)
		{
			this.transport = transport;
		}

		readonly Transport transport;

		public InformationResult<VoyageModes> VoyageMode
		{
			get
			{
				InformationResult<VoyageModes> result = new InformationResult<VoyageModes>(VoyageModes.None, transport.JW_TransportModeInfo);
				switch (transport.JW_TransportMode)
				{
					case Constants.TransportModes.Air:
						result.Value = VoyageModes.Air;
						break;
					case Constants.TransportModes.Rail:
						result.Value = VoyageModes.Rail;
						break;
					case Constants.TransportModes.Road:
						result.Value = VoyageModes.Road;
						break;
					case Constants.TransportModes.Sea:
						result.Value = VoyageModes.Sea;
						break;
				}

				return result;
			}
		}

		public InformationResult<ZString> DepartureLocation
		{
			get { return new InformationResult<ZString>(GetLocationCode(transport.JW_RL_NKLoadPort), transport.JW_RL_NKLoadPortInfo); }
		}

		public InformationResult<ZDateTime> ETD
		{
			get { return new InformationResult<ZDateTime>(transport.JW_ETDInfo); }
		}

		public InformationResult<ZString> ArrivalLocation
		{
			get { return new InformationResult<ZString>(GetLocationCode(transport.JW_RL_NKDiscPort), transport.JW_RL_NKDiscPortInfo); }
		}

		public InformationResult<ZDateTime> ETA
		{
			get { return new InformationResult<ZDateTime>(transport.JW_ETAInfo); }
		}

		public InformationResult<ZString> MasterBillNumber
		{
			get
			{
				if (transport.JW_ParentType == Core.Constants.TransportParentTypes.Consol &&
					transport.TransportSupporter.TransportMode == Core.Constants.TransportModes.Air &&
					transport.JW_TransportMode == Core.Constants.TransportModes.Air)
				{
					ForwardingConsol consol = (ForwardingConsol)transport.Parent;
					return new InformationResult<ZString>(consol.MasterBillAirlinePrefix + "-" + consol.MasterBillMAWB,
							consol.JK_MasterBillNumInfo);
				}

				return new InformationResult<ZString>(Res.GetString("6a047424-fbac-420d-a9ee-0b708f63c3b3", "Master Bill Number"));
			}
		}

		public InformationResult<ZString> CarrierCode
		{
			get
			{
				return new InformationResult<ZString>(Res.GetString("306b79dd-2c72-4b3a-bf8e-393ab175e877", "Carrier Code"));
			}
		}
	}
}
