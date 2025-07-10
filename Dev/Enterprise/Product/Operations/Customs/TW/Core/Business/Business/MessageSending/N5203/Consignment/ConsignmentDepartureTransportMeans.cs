using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class DepartureTransportMeans : ITransportMeans
	{
		public DepartureTransportMeans(JobDeclaration declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public ZDate ArrivalDateTime => ZDate.Empty;

		public ZString TypeCode => declaration?.JE_Calc_TWTransportCode ?? ZString.Empty;

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

		public ZString ID => declaration?.Vessel?.RV_LloydsNumber ?? ZString.Empty;

		public ZString JourneyID => ZString.Empty;

		public ZString Registration => ZString.Empty;

		public ZString Name => declaration?.JE_VesselName ?? ZString.Empty;

		public ZString CallSignID => ZString.Empty;
	}
}
