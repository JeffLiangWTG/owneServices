using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class TransportMeansWrapper : ITransportMeans
	{
		public TransportMeansWrapper(ZString journeyID) : this(journeyID, ZString.Empty)
		{
		}

		public TransportMeansWrapper(ZString journeyID, ZString name)
		{
			this.journeyID = journeyID;
			this.name = name;
		}

		readonly ZString journeyID;
		readonly ZString name;

		public ZDate ArrivalDateTime => ZDate.Empty;

		public ZString TypeCode => ZString.Empty;

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

		public ZString ID => ZString.Empty;

		public ZString JourneyID => journeyID;

		public ZString Registration => ZString.Empty;

		public ZString Name => name;

		public ZString CallSignID => ZString.Empty;
	}
}
