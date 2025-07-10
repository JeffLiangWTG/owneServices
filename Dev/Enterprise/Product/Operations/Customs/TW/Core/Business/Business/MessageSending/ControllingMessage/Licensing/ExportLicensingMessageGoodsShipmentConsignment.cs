using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Freight.Business;

namespace Enterprise.Customs.TW.Business
{
	public class ExportLicensingMessageGoodsShipmentConsignment : LicensingMessageGoodsShipmentConsignment, ITransportMeans
	{
		public ExportLicensingMessageGoodsShipmentConsignment(CusTWControllingMessageHeader header) : base(header)
		{
		}

		protected override ZString GetArrivalTransportMeansTypeCodeCore() => ZString.Empty;

		protected override ILocation GetTranshipmentLocationCore()
		{
			var portCode = Declaration.Transports.Cast<Transport>().OrderBy(x => x.JW_LegOrder).FirstOrDefault(x => !x.IsDomestic)?.JW_RL_NKDiscPortForBinding ?? ZString.Empty;
			return new LocationWrapper(portCode);
		}

		protected override ILocation GetUnloadingLocationCore() => new LocationWrapper(Declaration.JE_RL_NKFinalDestination);

		protected override ILocation GetTransitDepartureCore() => new LocationWrapper();

		protected override ITransportMeans GetDepartureTransportMeans() => this;

		#region ITransportMeans

		ZDate ITransportMeans.ArrivalDateTime => default;

		ZString ITransportMeans.TypeCode => Declaration.JE_Calc_TWTransportCode;

		IEnumerable<ZString> ITransportMeans.ItineraryRoutingCountryCodes => default;

		ZString ITransportMeans.ID => SharedHelper.GetTransportID(Declaration);

		ZString ITransportMeans.JourneyID => default;

		ZString ITransportMeans.Registration => default;

		ZString ITransportMeans.Name => default;

		ZString ITransportMeans.CallSignID => default;

		#endregion
	}
}
