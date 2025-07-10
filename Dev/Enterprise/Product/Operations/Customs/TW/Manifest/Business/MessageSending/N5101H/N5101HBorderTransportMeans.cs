using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class N5101HBorderTransportMeans : ITransportMeans
	{
		public N5101HBorderTransportMeans(AsycudaManifestHeader header)
		{
			this.header = header;
		}

		readonly AsycudaManifestHeader header;

		ZDate ITransportMeans.ArrivalDateTime => ZDate.Empty;

		ZString ITransportMeans.TypeCode
		{
			get
			{
				ZString typeCode;
				typeCode = new Dictionary<ZString, ZString> {
						{ Core.Constants.TransportModes.Sea, Constants.TransportModeTypeCodes.Sea },
						{ Core.Constants.TransportModes.Air, Constants.TransportModeTypeCodes.Air }
					}.TryGetValue(header.AMA_TransportMode, out typeCode) ? typeCode : ZString.Empty;
				return typeCode;
			}
		}

		IEnumerable<ZString> ITransportMeans.ItineraryRoutingCountryCodes => null;

		ZString ITransportMeans.ID => header.AMA_LloydsNumber;

		ZString ITransportMeans.JourneyID => header.IsAir ? SharedHelper.FormatVoyageFlightNo(header.AMA_Voyage) : header.AMA_Voyage;

		ZString ITransportMeans.Registration => header.AMA_VehicleRegistration;

		ZString ITransportMeans.Name => ZString.Empty;

		ZString ITransportMeans.CallSignID => ZString.Empty;
	}
}
