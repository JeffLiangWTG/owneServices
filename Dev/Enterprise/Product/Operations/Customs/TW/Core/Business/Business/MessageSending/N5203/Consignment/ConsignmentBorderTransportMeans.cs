using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5203
{
	public class BorderTransportMeans : ITransportMeans
	{
		public BorderTransportMeans(JobDeclaration declaration)
		{
			this.declaration = Argument.NotNull(declaration, "declaration");
		}

		readonly JobDeclaration declaration;

		public ZDate ArrivalDateTime => ZDate.Empty;

		public ZString TypeCode => ZString.Empty;

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

		public ZString ID => ZString.Empty;

		public ZString JourneyID => SharedHelper.GetVoyageFlightNo(declaration);

		public ZString Registration => declaration.JE_VesselArrivalReg;

		public ZString Name => ZString.Empty;

		public ZString CallSignID => declaration.Vessel?.RV_RadioCallSign ?? ZString.Empty;
	}
}
