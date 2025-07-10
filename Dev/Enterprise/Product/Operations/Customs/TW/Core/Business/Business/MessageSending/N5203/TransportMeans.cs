using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class TransportMeans : ITransportMeans
	{
		public TransportMeans(CusEntryHeader entryHeader)
		{
			var header = Argument.NotNull(entryHeader, nameof(entryHeader));
			declaration = Argument.NotNull(header.Declaration, nameof(header.Declaration));
		}

		readonly JobDeclaration declaration;

		public ZDate ArrivalDateTime => ZDate.Empty;

		public ZString TypeCode
		{
			get
			{
				var result = ZString.Empty;
				var transportMode = declaration.JE_TransportMode;
				if (transportMode == TransportTypeList.Codes.Air)
				{
					result = BorderTransportMeansTypeCodes._4;
				}
				else if (transportMode == TransportTypeList.Codes.Sea)
				{
					result = BorderTransportMeansTypeCodes._1;
				}
				return result;
			}
		}

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => declaration.Itineraries.Cast<ItineraryData>().Select(x => x.CY_Code);

		public ZString ID => ZString.Empty;

		public ZString JourneyID => ZString.Empty;

		public ZString Registration => ZString.Empty;

		public ZString Name => ZString.Empty;

		public ZString CallSignID => ZString.Empty;
	}
}
