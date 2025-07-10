using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business
{
	public class LicensingMessageTransportMeans : ITransportMeans
	{
		protected JobDeclaration Declaration { get; }

		public LicensingMessageTransportMeans(JobDeclaration declaration)
		{
			Declaration = Argument.NotNull(declaration, nameof(declaration));
		}

		public ZDate ArrivalDateTime => GetArrivalDateTimeCore();

		protected virtual ZDate GetArrivalDateTimeCore() => Declaration.JE_DateOfArrival.Date;

		public ZString TypeCode
		{
			get
			{
				var result = ZString.Empty;
				switch (Declaration.JE_TransportMode)
				{
					case TransportTypeList.Codes.Sea:
						result = BorderTransportMeansTypeCodes._1;
						break;
					case TransportTypeList.Codes.Air:
						result = BorderTransportMeansTypeCodes._4;
						break;
				}
				return result;
			}
		}

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => Declaration.Itineraries.Cast<ItineraryData>().Select(x => x.CY_Code);

		public ZString ID => GetIDCore();

		protected virtual ZString GetIDCore() => SharedHelper.GetTransportID(Declaration);

		public ZString JourneyID => Declaration.JE_VoyageFlightNo;

		public ZString Registration => Declaration.JE_VesselArrivalReg;

		public ZString Name => null;

		public ZString CallSignID => null;
	}
}
