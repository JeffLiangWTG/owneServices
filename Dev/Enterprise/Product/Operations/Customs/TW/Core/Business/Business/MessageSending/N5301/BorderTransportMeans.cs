using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class BorderTransportMeans : ITransportMeans
	{
		public BorderTransportMeans(CusInBondHeader header)
		{
			this.header = header;
		}

		readonly CusInBondHeader header;

		public ZDate ArrivalDateTime => header.BH_ETA.Date;

		public ZString TypeCode => header.BH_ImportTransportMode.Right(1);

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

		public ZString ID => header.BH_ImportConveyanceName.IsEmpty ? header.BH_UniqueVoyageIdentifier : header.ImportConveyanceNameDescription;

		public ZString JourneyID => CommonHelper.ConvertToNilWhenEmpty(header.BH_UniqueVoyageIdentifier);

		public ZString Registration => header.BH_VoyageNumber;

		public ZString Name => header.BH_ImportConveyanceName;

		public ZString CallSignID => ZString.Empty;
	}
}
