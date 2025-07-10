using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using static Enterprise.Freight.Forwarding.Documents.DocDataObjects.DocDataConstants;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.IL
{
	public static class GatePassMovementHelper
	{
		public static ZString GetCargoIdentifierTypeCode(string transportMode)
			=> transportMode switch
			{
				Constants.TransportModes.Sea => ILCargoIdentifierType.SeaDealImport,
				Constants.TransportModes.Road => ILCargoIdentifierType.LandDealImport,
				Constants.TransportModes.Air => ILCargoIdentifierType.AirBillOfLoadingImport,
				_ => ZString.Empty
			};

		public static ZString GetProcessType(string countryCode)
			=> countryCode switch
			{
				Constants.CountryCodes.Israel => ILProcessType.Import,
				_ => ILProcessType.Export,
			};

		public static ZString GetCargoIdentifierKey1(ForwardingConsol consol, ZString transportMode)
		{
			var arrivalTransport = consol?.Transports.ArrivalTransport;
			if (arrivalTransport == null)
			{
				return ZString.Empty;
			}

			return transportMode.ToString() switch
			{
				Constants.TransportModes.Road => arrivalTransport.JW_ArrivalPortRouteId,
				Constants.TransportModes.Sea => arrivalTransport.JW_ArrivalPortRouteId,
				Constants.TransportModes.Air => GetAirCargoIdentifierKey1(arrivalTransport),
				_ => ZString.Empty
			};
		}

		public static ZString GetAirCargoIdentifierKey2(ForwardingConsol consol)
			=> consol == null ? ZString.Empty : consol.MasterBillAirlinePrefix + ILCargoIdentifierType.MasterBillPartSeparator + consol.MasterBillMAWB;

		static ZString GetAirCargoIdentifierKey1(Freight.Business.Transport arrivalTransport)
			=> (arrivalTransport.JW_ATD.IsEmpty, arrivalTransport.JW_ETD.IsEmpty) switch
			{
				(true, true) => ZString.Empty,
				(true, false) => arrivalTransport.JW_ETD.Year.ToString(),
				(false, true) => arrivalTransport.JW_ATD.Year.ToString(),
				(false, false) => arrivalTransport.JW_ATD.Year.ToString(),
			};
	}
}
