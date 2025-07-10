using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business.N5301
{
	public class DepartureTransportMeans : ITransportMeans
	{
		public DepartureTransportMeans(CusInBondMoveHeader cusInBondMoveHeader)
		{
			this.cusInBondMoveHeader = cusInBondMoveHeader;
		}

		readonly CusInBondMoveHeader cusInBondMoveHeader;

		public ZDate ArrivalDateTime => ZDate.Empty;

		public ZString TypeCode => ZString.Empty;

		public IEnumerable<ZString> ItineraryRoutingCountryCodes => null;

		public ZString ID => cusInBondMoveHeader.BM_ExportLadenOn.IsEmpty ? cusInBondMoveHeader.BM_ConveyanceNumber : cusInBondMoveHeader.BM_ExportLadenOnDescription;

		public ZString JourneyID => CommonHelper.ConvertToNilWhenEmpty(cusInBondMoveHeader.BM_ConveyanceNumber);

		public ZString Registration => cusInBondMoveHeader.BM_TransportAtDeparture;

		public ZString Name => cusInBondMoveHeader.BM_ExportLadenOn;

		public ZString CallSignID => ZString.Empty;
	}
}
